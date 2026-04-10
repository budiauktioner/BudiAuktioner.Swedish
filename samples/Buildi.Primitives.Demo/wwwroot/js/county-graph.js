window.countyGraph = {
    _state: null,

    init(containerId, nodesJson, adjacencyJson) {
        const container = document.getElementById(containerId);
        if (!container) return;

        const nodes = JSON.parse(nodesJson);
        const adjacency = JSON.parse(adjacencyJson);
        const cosLat = Math.cos(62 * Math.PI / 180);

        const edges = [];
        const seen = new Set();
        for (const n of nodes) {
            const adj = adjacency[n.code] || [];
            for (const neighborCode of adj) {
                const key = [n.code, neighborCode].sort().join('-');
                if (!seen.has(key)) {
                    seen.add(key);
                    edges.push({ source: n.code, target: neighborCode });
                }
            }
        }

        const nodeByCode = {};
        nodes.forEach((n, i) => { n.idx = i; nodeByCode[n.code] = n; });

        const state = {
            nodes, edges, adjacency, nodeByCode,
            hoveredIdx: null,
            transform: { x: 0, y: 0, scale: 1 },
            isPanning: false, panStart: { x: 0, y: 0 }, panOrigin: { x: 0, y: 0 }
        };
        this._state = state;

        container.innerHTML = `
            <div class="d-flex align-items-center gap-3 mb-2 flex-wrap" style="font-size:.82rem">
                <label class="form-check mb-0">
                    <input type="checkbox" id="cg-labels" class="form-check-input" checked>
                    <span class="form-check-label">Labels</span>
                </label>
                <input type="search" id="cg-search" class="form-control form-control-sm"
                       style="width:160px" placeholder="Search…">
                <span id="cg-stats" class="text-body-secondary ms-auto" style="font-size:.75rem"></span>
            </div>
            <div style="position:relative">
                <svg id="cg-svg" style="width:100%;height:520px;background:#f8f9fa;border-radius:8px;border:1px solid #dee2e6;cursor:grab">
                    <g id="cg-root"><g id="cg-edges"></g><g id="cg-nodes"></g></g>
                </svg>
                <div id="cg-tooltip" style="position:absolute;background:#fff;border:1px solid #dee2e6;border-radius:8px;padding:8px 12px;font-size:.78rem;pointer-events:none;opacity:0;transition:opacity .12s;z-index:20;max-width:280px;box-shadow:0 4px 12px rgba(0,0,0,.12)"></div>
            </div>`;

        const svg = document.getElementById('cg-svg');
        const rootG = document.getElementById('cg-root');
        const edgesG = document.getElementById('cg-edges');
        const nodesG = document.getElementById('cg-nodes');
        const tooltip = document.getElementById('cg-tooltip');
        const labelsChk = document.getElementById('cg-labels');
        const searchBox = document.getElementById('cg-search');
        const statsEl = document.getElementById('cg-stats');

        function projectAll() {
            const W = svg.clientWidth, H = svg.clientHeight;
            const pad = 50;
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

        function drawEdges() {
            edgesG.innerHTML = '';
            for (const e of edges) {
                const a = nodeByCode[e.source], b = nodeByCode[e.target];
                if (!a || !b) continue;
                const line = document.createElementNS('http://www.w3.org/2000/svg', 'line');
                line.setAttribute('x1', a.x); line.setAttribute('y1', a.y);
                line.setAttribute('x2', b.x); line.setAttribute('y2', b.y);
                line.dataset.source = e.source;
                line.dataset.target = e.target;
                Object.assign(line.style, {
                    stroke: '#adb5bd', strokeWidth: '1.5',
                    opacity: '0.5', fill: 'none', pointerEvents: 'none',
                    transition: 'opacity .15s, stroke .15s'
                });
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
                g.dataset.code = n.code;
                g.style.cursor = 'pointer';
                g.style.transition = 'opacity .15s';

                const circle = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
                circle.setAttribute('cx', n.x);
                circle.setAttribute('cy', n.y);
                circle.setAttribute('r', 8);
                circle.setAttribute('fill', n.color);
                circle.setAttribute('stroke', '#fff');
                circle.setAttribute('stroke-width', '2');
                circle.style.transition = 'r .15s, stroke .15s';
                g.appendChild(circle);

                const text = document.createElementNS('http://www.w3.org/2000/svg', 'text');
                text.setAttribute('x', n.x);
                text.setAttribute('y', n.y - 13);
                text.setAttribute('text-anchor', 'middle');
                text.textContent = n.name.replace(' län', '');
                Object.assign(text.style, {
                    fontSize: '9px', fill: '#495057', fontWeight: '500',
                    pointerEvents: 'none', userSelect: 'none',
                    opacity: showLabels ? '1' : '0', transition: 'opacity .15s'
                });
                g.appendChild(text);

                if (search && n.name.toLowerCase().includes(search)) {
                    circle.setAttribute('r', 12);
                    circle.setAttribute('stroke', '#ffc107');
                    circle.setAttribute('stroke-width', '3');
                    circle.style.filter = 'drop-shadow(0 0 6px rgba(255,193,7,.5))';
                    text.style.opacity = '1';
                    text.style.fill = '#856404';
                    text.style.fontWeight = '700';
                    text.style.fontSize = '11px';
                }

                g.addEventListener('mouseenter', e => onHover(i, e));
                g.addEventListener('mouseleave', () => onLeave());
                nodesG.appendChild(g);
            }
        }

        function onHover(idx, evt) {
            state.hoveredIdx = idx;
            const n = nodes[idx];
            const adj = adjacency[n.code] || [];
            const adjSet = new Set(adj);

            nodesG.querySelectorAll('g').forEach(g => {
                const ni = +g.dataset.idx;
                const nc = g.dataset.code;
                const c = g.querySelector('circle');
                const t = g.querySelector('text');
                if (ni === idx) {
                    c.setAttribute('r', 14);
                    c.setAttribute('stroke-width', '3');
                    c.setAttribute('stroke', '#212529');
                    t.style.opacity = '1';
                    t.style.fill = '#212529';
                    t.style.fontWeight = '700';
                    t.style.fontSize = '11px';
                } else if (adjSet.has(nc)) {
                    c.setAttribute('r', 10);
                    c.setAttribute('stroke-width', '2');
                    c.setAttribute('stroke', '#198754');
                    t.style.opacity = '1';
                    t.style.fill = '#198754';
                    t.style.fontWeight = '600';
                } else {
                    g.style.opacity = '0.15';
                }
            });

            edgesG.querySelectorAll('line').forEach(line => {
                const sc = line.dataset.source, tc = line.dataset.target;
                if (sc === n.code || tc === n.code) {
                    line.style.opacity = '1';
                    line.style.strokeWidth = '3';
                    line.style.stroke = n.color;
                } else {
                    line.style.opacity = '0.05';
                }
            });

            const adjNames = adj.map(c => nodeByCode[c]?.name?.replace(' län', '') || c).sort();
            tooltip.innerHTML = `
                <div style="font-weight:700;margin-bottom:2px">${n.name}</div>
                <div style="color:#6c757d;margin-bottom:4px">Code: ${n.code} · ${n.muniCount} municipalities</div>
                <div style="display:flex;justify-content:space-between;gap:8px;color:#6c757d">
                    <span>Adjacent counties</span><span style="color:#212529;font-weight:500">${adj.length}</span></div>
                <div style="display:flex;justify-content:space-between;gap:8px;color:#6c757d">
                    <span>Lat / Lon</span><span style="color:#212529;font-weight:500">${n.lat.toFixed(2)}° / ${n.lon.toFixed(2)}°</span></div>
                ${adjNames.length > 0 ? `<div style="margin-top:4px;font-size:.68rem;color:#868e96;line-height:1.4">Neighbors: ${adjNames.join(', ')}</div>` : '<div style="margin-top:4px;font-size:.68rem;color:#868e96">No land borders (island)</div>'}
                ${n.municipalities ? `<div style="margin-top:4px;font-size:.68rem;color:#868e96;line-height:1.4">Municipalities: ${n.municipalities}</div>` : ''}`;
            tooltip.style.opacity = '1';
            positionTooltip(evt);
        }

        function onLeave() {
            state.hoveredIdx = null;
            nodesG.querySelectorAll('g').forEach(g => {
                g.style.opacity = '1';
                const c = g.querySelector('circle');
                const t = g.querySelector('text');
                c.setAttribute('r', 8);
                c.setAttribute('stroke', '#fff');
                c.setAttribute('stroke-width', '2');
                c.style.filter = '';
                t.style.fill = '#495057';
                t.style.fontWeight = '500';
                t.style.fontSize = '9px';
                t.style.opacity = labelsChk.checked ? '1' : '0';
            });
            edgesG.querySelectorAll('line').forEach(line => {
                line.style.strokeWidth = '1.5';
                line.style.opacity = '0.5';
                line.style.stroke = '#adb5bd';
            });
            tooltip.style.opacity = '0';
            if (searchBox.value) drawNodes();
        }

        function positionTooltip(evt) {
            const rect = svg.getBoundingClientRect();
            let tx = evt.clientX - rect.left + 14;
            let ty = evt.clientY - rect.top - 10;
            if (tx + 290 > rect.width) tx = evt.clientX - rect.left - 295;
            if (ty < 10) ty = 10;
            tooltip.style.left = tx + 'px';
            tooltip.style.top = ty + 'px';
        }

        function updateStats() {
            const isolated = nodes.filter(n => (adjacency[n.code] || []).length === 0).length;
            statsEl.textContent = `${nodes.length} counties · ${edges.length} borders · ${isolated} island`;
        }

        function fullRedraw() {
            projectAll();
            updateStats();
            drawEdges();
            drawNodes();
        }

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

        labelsChk.addEventListener('change', () => {
            nodesG.querySelectorAll('text').forEach(t => { t.style.opacity = labelsChk.checked ? '1' : '0'; });
        });
        searchBox.addEventListener('input', () => drawNodes());

        fullRedraw();
    }
};
