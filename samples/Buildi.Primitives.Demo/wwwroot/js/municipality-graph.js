window.municipalityGraph = {
    _state: null,
    _nodesG: null,
    _edgesG: null,
    _nodes: null,
    _counties: null,

    highlightCodes(codes) {
        const nodesG = this._nodesG;
        const edgesG = this._edgesG;
        const nodes = this._nodes;
        const counties = this._counties;
        if (!nodesG || !nodes) return;

        const codeSet = new Set(codes);
        const originCode = codes[0];

        nodesG.querySelectorAll('g').forEach(g => {
            const n = nodes[+g.dataset.idx];
            const c = g.querySelector('circle');
            const t = g.querySelector('text');
            if (n.code === originCode) {
                g.style.opacity = '1';
                c.setAttribute('r', 8);
                c.setAttribute('stroke', '#dc3545');
                c.setAttribute('stroke-width', '3');
                c.style.filter = 'drop-shadow(0 0 6px rgba(220,53,69,.5))';
                t.style.opacity = '1'; t.style.fill = '#dc3545'; t.style.fontWeight = '700'; t.style.fontSize = '11px';
            } else if (codeSet.has(n.code)) {
                g.style.opacity = '1';
                c.setAttribute('r', 5);
                c.setAttribute('stroke', '#198754');
                c.setAttribute('stroke-width', '2');
                c.style.filter = '';
                t.style.opacity = '1'; t.style.fill = '#198754'; t.style.fontWeight = '600'; t.style.fontSize = '9px';
            } else {
                g.style.opacity = '0.1';
                c.setAttribute('r', 3.5);
                c.setAttribute('stroke', '#fff');
                c.setAttribute('stroke-width', '1');
                c.style.filter = '';
                t.style.opacity = '0'; t.style.fill = '#6c757d'; t.style.fontWeight = ''; t.style.fontSize = '7px';
            }
        });

        edgesG.querySelectorAll('line').forEach(line => {
            const sn = nodes[+line.dataset.source];
            const tn = nodes[+line.dataset.target];
            const relevant = codeSet.has(sn.code) && codeSet.has(tn.code);
            line.style.opacity = relevant ? '0.6' : '0.03';
            if (relevant) line.style.strokeWidth = '1.5';
        });
    },

    init(containerId, nodesJson, countiesJson) {
        const container = document.getElementById(containerId);
        if (!container) return;

        const nodes = JSON.parse(nodesJson);
        const counties = JSON.parse(countiesJson);
        const cosLat = Math.cos(62 * Math.PI / 180);

        const state = {
            nodes, counties, cosLat,
            k: 3, edges: [], adjacency: [],
            hoveredIdx: null, highlightedCounty: null,
            transform: { x: 0, y: 0, scale: 1 },
            isPanning: false, panStart: { x: 0, y: 0 }, panOrigin: { x: 0, y: 0 }
        };
        this._state = state;

        container.innerHTML = `
            <div class="d-flex align-items-center gap-3 mb-2 flex-wrap" style="font-size:.82rem">
                <label class="d-flex align-items-center gap-1 mb-0">
                    k = <span id="mg-k-val" class="badge bg-primary">3</span>
                    <input type="range" id="mg-k-slider" class="form-range" style="width:110px"
                           min="1" max="8" value="3">
                </label>
                <label class="form-check mb-0">
                    <input type="checkbox" id="mg-labels" class="form-check-input">
                    <span class="form-check-label">Labels</span>
                </label>
                <label class="form-check mb-0">
                    <input type="checkbox" id="mg-cross" class="form-check-input">
                    <span class="form-check-label">Cross-county only</span>
                </label>
                <input type="search" id="mg-search" class="form-control form-control-sm"
                       style="width:160px" placeholder="Search…">
                <span id="mg-stats" class="text-body-secondary ms-auto" style="font-size:.75rem"></span>
            </div>
            <div style="position:relative">
                <svg id="mg-svg" style="width:100%;height:520px;background:#f8f9fa;border-radius:8px;border:1px solid #dee2e6;cursor:grab">
                    <g id="mg-root"><g id="mg-edges"></g><g id="mg-nodes"></g></g>
                </svg>
                <div id="mg-tooltip" style="position:absolute;background:#fff;border:1px solid #dee2e6;border-radius:8px;padding:8px 12px;font-size:.78rem;pointer-events:none;opacity:0;transition:opacity .12s;z-index:20;max-width:250px;box-shadow:0 4px 12px rgba(0,0,0,.12)"></div>
                <div id="mg-legend" style="position:absolute;top:8px;right:8px;background:#ffffffee;border:1px solid #dee2e6;border-radius:8px;padding:8px;font-size:.72rem;max-height:500px;overflow-y:auto;width:155px"></div>
            </div>`;

        const svg = document.getElementById('mg-svg');
        const rootG = document.getElementById('mg-root');
        const edgesG = document.getElementById('mg-edges');
        const nodesG = document.getElementById('mg-nodes');
        this._nodesG = nodesG;
        this._edgesG = edgesG;
        this._nodes = nodes;
        this._counties = counties;
        const tooltip = document.getElementById('mg-tooltip');
        const kSlider = document.getElementById('mg-k-slider');
        const kVal = document.getElementById('mg-k-val');
        const searchBox = document.getElementById('mg-search');
        const labelsChk = document.getElementById('mg-labels');
        const crossChk = document.getElementById('mg-cross');
        const statsEl = document.getElementById('mg-stats');
        const legendEl = document.getElementById('mg-legend');

        function projectAll() {
            const W = svg.clientWidth, H = svg.clientHeight;
            const pad = 30;
            const latMin = 55.2, latMax = 69.2, lonMin = 10.5, lonMax = 25.0;
            const xRange = (lonMax - lonMin) * cosLat;
            const yRange = latMax - latMin;
            const scale = Math.min((W - pad * 2) / xRange, (H - pad * 2) / yRange);
            const ox = pad + ((W - pad * 2) - xRange * scale) / 2;
            const oy = pad + ((H - pad * 2) - yRange * scale) / 2;
            for (const n of nodes) {
                n.x = ox + (n.lon - lonMin) * cosLat * scale;
                n.y = oy + (latMax - n.lat) * scale;
            }
        }

        function geoDist(a, b) {
            const dx = (a.lon - b.lon) * cosLat;
            const dy = a.lat - b.lat;
            return Math.sqrt(dx * dx + dy * dy);
        }

        function computeEdges() {
            const edges = [];
            const seen = new Set();
            for (let i = 0; i < nodes.length; i++) {
                const dists = [];
                for (let j = 0; j < nodes.length; j++) {
                    if (i === j) continue;
                    dists.push({ j, d: geoDist(nodes[i], nodes[j]) });
                }
                dists.sort((a, b) => a.d - b.d);
                for (const nb of dists.slice(0, state.k)) {
                    const key = Math.min(i, nb.j) + '-' + Math.max(i, nb.j);
                    if (!seen.has(key)) {
                        seen.add(key);
                        edges.push({ source: i, target: nb.j, dist: nb.d, cross: nodes[i].county !== nodes[nb.j].county });
                    }
                }
            }
            state.edges = edges;
            state.adjacency = nodes.map(() => new Set());
            for (const e of edges) {
                state.adjacency[e.source].add(e.target);
                state.adjacency[e.target].add(e.source);
            }
        }

        function drawEdges() {
            edgesG.innerHTML = '';
            const crossMode = crossChk.checked;
            for (const e of state.edges) {
                if (crossMode && !e.cross) continue;
                const a = nodes[e.source], b = nodes[e.target];
                const line = document.createElementNS('http://www.w3.org/2000/svg', 'line');
                line.setAttribute('x1', a.x); line.setAttribute('y1', a.y);
                line.setAttribute('x2', b.x); line.setAttribute('y2', b.y);
                line.dataset.source = e.source;
                line.dataset.target = e.target;
                const color = e.cross ? '#ffc107' : counties[a.county]?.color || '#6c757d';
                Object.assign(line.style, {
                    stroke: color, strokeWidth: e.cross ? '1' : '0.8',
                    opacity: e.cross ? '0.5' : '0.35', fill: 'none', pointerEvents: 'none',
                    transition: 'opacity .15s'
                });
                if (e.cross) line.style.strokeDasharray = '4 3';
                edgesG.appendChild(line);
            }
        }

        function drawNodes() {
            nodesG.innerHTML = '';
            const showLabels = labelsChk.checked;
            const search = (searchBox.value || '').toLowerCase();
            for (let i = 0; i < nodes.length; i++) {
                const n = nodes[i];
                const g = document.createElementNS('http://www.w3.org/2000/svg', 'g');
                g.dataset.idx = i;
                g.style.cursor = 'pointer';
                g.style.transition = 'opacity .15s';
                const circle = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
                circle.setAttribute('cx', n.x); circle.setAttribute('cy', n.y);
                circle.setAttribute('r', 3.5);
                circle.setAttribute('fill', counties[n.county]?.color || '#6c757d');
                circle.setAttribute('stroke', '#fff');
                circle.setAttribute('stroke-width', '1');
                circle.style.transition = 'r .15s';
                g.appendChild(circle);
                const text = document.createElementNS('http://www.w3.org/2000/svg', 'text');
                text.setAttribute('x', n.x + 5); text.setAttribute('y', n.y + 3);
                text.textContent = n.name;
                Object.assign(text.style, {
                    fontSize: '7px', fill: '#6c757d', pointerEvents: 'none',
                    userSelect: 'none', opacity: showLabels ? '0.8' : '0', transition: 'opacity .15s'
                });
                g.appendChild(text);
                if (search && n.name.toLowerCase().includes(search)) {
                    circle.setAttribute('r', 7);
                    circle.setAttribute('stroke', '#ffc107');
                    circle.setAttribute('stroke-width', '2.5');
                    circle.style.filter = 'drop-shadow(0 0 4px rgba(255,193,7,.5))';
                    text.style.opacity = '1';
                    text.style.fill = '#856404';
                    text.style.fontWeight = '700';
                    text.style.fontSize = '10px';
                }
                g.addEventListener('mouseenter', e => onHover(i, e));
                g.addEventListener('mouseleave', () => onLeave());
                nodesG.appendChild(g);
            }
        }

        function onHover(idx, evt) {
            state.hoveredIdx = idx;
            const n = nodes[idx];
            const neighbors = state.adjacency[idx];
            nodesG.querySelectorAll('g').forEach(g => {
                const gi = +g.dataset.idx;
                const c = g.querySelector('circle');
                const t = g.querySelector('text');
                if (gi === idx) {
                    c.setAttribute('r', 7); c.setAttribute('stroke-width', '2');
                    t.style.opacity = '1'; t.style.fill = '#212529'; t.style.fontWeight = '600'; t.style.fontSize = '10px';
                } else if (neighbors.has(gi)) {
                    c.setAttribute('r', 5); c.setAttribute('stroke-width', '1.5');
                    t.style.opacity = '1'; t.style.fill = '#495057';
                } else {
                    g.style.opacity = '0.15';
                }
            });
            edgesG.querySelectorAll('line').forEach(line => {
                const s = +line.dataset.source, t = +line.dataset.target;
                if (s === idx || t === idx) {
                    line.style.opacity = '1'; line.style.strokeWidth = '2';
                    line.style.stroke = counties[n.county]?.color || '#6c757d';
                } else {
                    line.style.opacity = '0.05';
                }
            });
            const neighborNames = [...neighbors].map(j => nodes[j].name).sort();
            const crossCount = [...neighbors].filter(j => nodes[j].county !== n.county).length;
            tooltip.innerHTML = `
                <div style="font-weight:700;margin-bottom:2px">${n.name}</div>
                <div style="color:#6c757d;margin-bottom:4px">${counties[n.county]?.name || ''} (${n.code})</div>
                <div style="display:flex;justify-content:space-between;gap:8px;color:#6c757d">
                    <span>Neighbors</span><span style="color:#212529;font-weight:500">${neighbors.size}</span></div>
                <div style="display:flex;justify-content:space-between;gap:8px;color:#6c757d">
                    <span>Cross-county</span><span style="color:#212529;font-weight:500">${crossCount}</span></div>
                <div style="display:flex;justify-content:space-between;gap:8px;color:#6c757d">
                    <span>Lat / Lon</span><span style="color:#212529;font-weight:500">${n.lat.toFixed(2)}° / ${n.lon.toFixed(2)}°</span></div>
                <div style="margin-top:4px;font-size:.68rem;color:#868e96;line-height:1.4">${neighborNames.join(', ')}</div>`;
            tooltip.style.opacity = '1';
            positionTooltip(evt);
        }

        function onLeave() {
            state.hoveredIdx = null;
            nodesG.querySelectorAll('g').forEach(g => {
                g.style.opacity = '1';
                const c = g.querySelector('circle');
                const t = g.querySelector('text');
                c.setAttribute('r', 3.5); c.setAttribute('stroke', '#fff'); c.setAttribute('stroke-width', '1');
                c.style.filter = '';
                t.style.fill = '#6c757d'; t.style.fontWeight = ''; t.style.fontSize = '7px';
                t.style.opacity = labelsChk.checked ? '0.8' : '0';
            });
            edgesG.querySelectorAll('line').forEach(line => {
                const s = +line.dataset.source;
                const isCross = line.style.strokeDasharray;
                line.style.strokeWidth = isCross ? '1' : '0.8';
                line.style.opacity = isCross ? '0.5' : '0.35';
                line.style.stroke = isCross ? '#ffc107' : (counties[nodes[s].county]?.color || '#6c757d');
            });
            tooltip.style.opacity = '0';
            if (searchBox.value) drawNodes();
        }

        function positionTooltip(evt) {
            const rect = svg.getBoundingClientRect();
            let tx = evt.clientX - rect.left + 14;
            let ty = evt.clientY - rect.top - 10;
            if (tx + 260 > rect.width) tx = evt.clientX - rect.left - 265;
            if (ty < 10) ty = 10;
            tooltip.style.left = tx + 'px';
            tooltip.style.top = ty + 'px';
        }

        function updateStats() {
            const cross = state.edges.filter(e => e.cross).length;
            const avg = state.edges.reduce((s, e) => s + e.dist, 0) / (state.edges.length || 1);
            statsEl.textContent = `${nodes.length} municipalities · ${state.edges.length} edges (${cross} cross-county) · avg ~${(avg * 111).toFixed(0)} km`;
        }

        function buildLegend() {
            const counts = {};
            for (const n of nodes) counts[n.county] = (counts[n.county] || 0) + 1;
            let html = '<div style="font-weight:600;margin-bottom:4px;font-size:.78rem">Counties</div>';
            for (const code of Object.keys(counties).sort()) {
                const c = counties[code];
                html += `<div class="mg-legend-item" data-county="${code}" style="display:flex;align-items:center;gap:5px;padding:2px 4px;border-radius:4px;cursor:pointer">
                    <span style="width:10px;height:10px;border-radius:3px;background:${c.color};flex-shrink:0"></span>
                    <span style="white-space:nowrap;overflow:hidden;text-overflow:ellipsis">${c.name.replace(' län', '')}</span>
                    <span style="margin-left:auto;color:#adb5bd;font-size:.65rem">${counts[code] || 0}</span></div>`;
            }
            legendEl.innerHTML = html;
            legendEl.querySelectorAll('.mg-legend-item').forEach(item => {
                item.addEventListener('click', () => {
                    const cc = item.dataset.county;
                    if (state.highlightedCounty === cc) {
                        state.highlightedCounty = null;
                        fullRedraw();
                    } else {
                        state.highlightedCounty = cc;
                        legendEl.querySelectorAll('.mg-legend-item').forEach(el => el.style.background = el.dataset.county === cc ? '#e9ecef' : '');
                        nodesG.querySelectorAll('g').forEach(g => {
                            const n = nodes[+g.dataset.idx];
                            g.style.opacity = n.county === cc ? '1' : '0.12';
                            if (n.county === cc) { const c = g.querySelector('circle'); c.setAttribute('r', 5); }
                        });
                        edgesG.querySelectorAll('line').forEach(l => {
                            const sc = nodes[+l.dataset.source].county, tc = nodes[+l.dataset.target].county;
                            l.style.opacity = (sc === cc || tc === cc) ? '0.7' : '0.03';
                        });
                    }
                });
                item.addEventListener('mouseenter', () => { if (!state.highlightedCounty) item.style.background = '#f1f3f5'; });
                item.addEventListener('mouseleave', () => { if (!state.highlightedCounty) item.style.background = ''; });
            });
        }

        function fullRedraw() {
            projectAll();
            computeEdges();
            updateStats();
            drawEdges();
            drawNodes();
            buildLegend();
        }

        // Zoom / pan
        svg.addEventListener('wheel', e => {
            e.preventDefault();
            const rect = svg.getBoundingClientRect();
            const mx = e.clientX - rect.left, my = e.clientY - rect.top;
            const factor = e.deltaY < 0 ? 1.12 : 1 / 1.12;
            const t = state.transform;
            const newScale = Math.max(0.3, Math.min(15, t.scale * factor));
            const ratio = newScale / t.scale;
            t.x = mx - ratio * (mx - t.x);
            t.y = my - ratio * (my - t.y);
            t.scale = newScale;
            rootG.setAttribute('transform', `translate(${t.x},${t.y}) scale(${t.scale})`);
        }, { passive: false });

        svg.addEventListener('mousedown', e => {
            if (e.button !== 0) return;
            state.isPanning = true;
            state.panStart = { x: e.clientX, y: e.clientY };
            state.panOrigin = { x: state.transform.x, y: state.transform.y };
        });
        window.addEventListener('mousemove', e => {
            if (state.hoveredIdx !== null) positionTooltip(e);
            if (!state.isPanning) return;
            state.transform.x = state.panOrigin.x + (e.clientX - state.panStart.x);
            state.transform.y = state.panOrigin.y + (e.clientY - state.panStart.y);
            rootG.setAttribute('transform', `translate(${state.transform.x},${state.transform.y}) scale(${state.transform.scale})`);
        });
        window.addEventListener('mouseup', () => { state.isPanning = false; });

        // Controls
        kSlider.addEventListener('input', () => {
            state.k = +kSlider.value;
            kVal.textContent = state.k;
            computeEdges();
            updateStats();
            drawEdges();
            drawNodes();
        });
        labelsChk.addEventListener('change', () => {
            nodesG.querySelectorAll('text').forEach(t => { t.style.opacity = labelsChk.checked ? '0.8' : '0'; });
        });
        crossChk.addEventListener('change', () => { drawEdges(); drawNodes(); });
        searchBox.addEventListener('input', () => { state.highlightedCounty = null; drawNodes(); });

        fullRedraw();
    }
};
