window.countryGraph = {
    _state: null,

    init(containerId, nodesJson, adjacencyJson, continentColorsJson) {
        const container = document.getElementById(containerId);
        if (!container) return;

        const nodes = JSON.parse(nodesJson);
        const adjacency = JSON.parse(adjacencyJson);
        const continentColors = JSON.parse(continentColorsJson);

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

        const continents = [...new Set(nodes.map(n => n.continent))].sort();

        const state = {
            nodes, edges, adjacency, nodeByCode, continentColors,
            hoveredIdx: null, activeContinent: 'Europe',
            transform: { x: 0, y: 0, scale: 1 },
            isPanning: false, panStart: { x: 0, y: 0 }, panOrigin: { x: 0, y: 0 }
        };
        this._state = state;

        const filterHtml = continents.map(c =>
            `<option value="${c}"${c === 'Europe' ? ' selected' : ''}>${c}</option>`
        ).join('');

        container.innerHTML = `
            <div class="d-flex align-items-center gap-3 mb-2 flex-wrap" style="font-size:.82rem">
                <div class="d-flex align-items-center gap-1">
                    <label class="form-label small mb-0">Continent</label>
                    <select id="ctg-continent" class="form-select form-select-sm" style="width:160px">
                        <option value="">All</option>
                        ${filterHtml}
                    </select>
                </div>
                <label class="form-check mb-0">
                    <input type="checkbox" id="ctg-labels" class="form-check-input" checked>
                    <span class="form-check-label">Labels</span>
                </label>
                <input type="search" id="ctg-search" class="form-control form-control-sm"
                       style="width:160px" placeholder="Search…">
                <span id="ctg-stats" class="text-body-secondary ms-auto" style="font-size:.75rem"></span>
            </div>
            <div style="position:relative">
                <svg id="ctg-svg" style="width:100%;height:560px;background:#f8f9fa;border-radius:8px;border:1px solid #dee2e6;cursor:grab">
                    <g id="ctg-root"><g id="ctg-edges"></g><g id="ctg-nodes"></g></g>
                </svg>
                <div id="ctg-tooltip" style="position:absolute;background:#fff;border:1px solid #dee2e6;border-radius:8px;padding:8px 12px;font-size:.78rem;pointer-events:none;opacity:0;transition:opacity .12s;z-index:20;max-width:320px;box-shadow:0 4px 12px rgba(0,0,0,.12)"></div>
            </div>`;

        const svg = document.getElementById('ctg-svg');
        const rootG = document.getElementById('ctg-root');
        const edgesG = document.getElementById('ctg-edges');
        const nodesG = document.getElementById('ctg-nodes');
        const tooltip = document.getElementById('ctg-tooltip');
        const labelsChk = document.getElementById('ctg-labels');
        const searchBox = document.getElementById('ctg-search');
        const statsEl = document.getElementById('ctg-stats');
        const continentSel = document.getElementById('ctg-continent');

        function getVisibleNodes() {
            const c = state.activeContinent;
            return c ? nodes.filter(n => n.continent === c) : nodes;
        }

        function getVisibleCodes() {
            return new Set(getVisibleNodes().map(n => n.code));
        }

        function projectAll() {
            const visible = getVisibleNodes();
            if (visible.length === 0) return;

            const W = svg.clientWidth, H = svg.clientHeight;
            const pad = 50;

            let latMin = Infinity, latMax = -Infinity, lonMin = Infinity, lonMax = -Infinity;
            for (const n of visible) {
                if (n.lat < latMin) latMin = n.lat;
                if (n.lat > latMax) latMax = n.lat;
                if (n.lon < lonMin) lonMin = n.lon;
                if (n.lon > lonMax) lonMax = n.lon;
            }

            const latMargin = Math.max((latMax - latMin) * 0.08, 2);
            const lonMargin = Math.max((lonMax - lonMin) * 0.08, 2);
            latMin -= latMargin; latMax += latMargin;
            lonMin -= lonMargin; lonMax += lonMargin;

            const midLat = (latMin + latMax) / 2;
            const cosLat = Math.cos(midLat * Math.PI / 180);
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
            const visibleCodes = getVisibleCodes();
            for (const e of edges) {
                if (!visibleCodes.has(e.source) && !visibleCodes.has(e.target)) continue;
                const a = nodeByCode[e.source], b = nodeByCode[e.target];
                if (!a || !b) continue;
                const bothVisible = visibleCodes.has(e.source) && visibleCodes.has(e.target);
                const line = document.createElementNS('http://www.w3.org/2000/svg', 'line');
                line.setAttribute('x1', a.x); line.setAttribute('y1', a.y);
                line.setAttribute('x2', b.x); line.setAttribute('y2', b.y);
                line.dataset.source = e.source;
                line.dataset.target = e.target;
                Object.assign(line.style, {
                    stroke: '#adb5bd', strokeWidth: '1.2',
                    opacity: bothVisible ? '0.4' : '0.12', fill: 'none', pointerEvents: 'none',
                    transition: 'opacity .15s, stroke .15s'
                });
                if (!bothVisible) line.setAttribute('stroke-dasharray', '3,3');
                edgesG.appendChild(line);
            }
        }

        function drawNodes() {
            nodesG.innerHTML = '';
            const showLabels = labelsChk.checked;
            const search = (searchBox.value || '').toLowerCase();
            const visibleCodes = getVisibleCodes();

            for (let i = 0; i < nodes.length; i++) {
                const n = nodes[i];
                const isVisible = visibleCodes.has(n.code);
                if (!isVisible) continue;

                const g = document.createElementNS('http://www.w3.org/2000/svg', 'g');
                g.dataset.idx = i;
                g.dataset.code = n.code;
                g.style.cursor = 'pointer';
                g.style.transition = 'opacity .15s';

                const color = continentColors[n.continent] || '#6c757d';
                const circle = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
                circle.setAttribute('cx', n.x);
                circle.setAttribute('cy', n.y);
                circle.setAttribute('r', 6);
                circle.setAttribute('fill', color);
                circle.setAttribute('stroke', '#fff');
                circle.setAttribute('stroke-width', '1.5');
                circle.style.transition = 'r .15s, stroke .15s';
                g.appendChild(circle);

                const text = document.createElementNS('http://www.w3.org/2000/svg', 'text');
                text.setAttribute('x', n.x);
                text.setAttribute('y', n.y - 10);
                text.setAttribute('text-anchor', 'middle');
                text.textContent = n.name;
                Object.assign(text.style, {
                    fontSize: '8px', fill: '#495057', fontWeight: '500',
                    pointerEvents: 'none', userSelect: 'none',
                    opacity: showLabels ? '1' : '0', transition: 'opacity .15s'
                });
                g.appendChild(text);

                if (search && n.name.toLowerCase().includes(search)) {
                    circle.setAttribute('r', 10);
                    circle.setAttribute('stroke', '#ffc107');
                    circle.setAttribute('stroke-width', '3');
                    circle.style.filter = 'drop-shadow(0 0 6px rgba(255,193,7,.5))';
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
            const adj = adjacency[n.code] || [];
            const adjSet = new Set(adj);
            const visibleCodes = getVisibleCodes();

            nodesG.querySelectorAll('g').forEach(g => {
                const ni = +g.dataset.idx;
                const nc = g.dataset.code;
                const c = g.querySelector('circle');
                const t = g.querySelector('text');
                if (ni === idx) {
                    c.setAttribute('r', 12);
                    c.setAttribute('stroke-width', '3');
                    c.setAttribute('stroke', '#212529');
                    t.style.opacity = '1';
                    t.style.fill = '#212529';
                    t.style.fontWeight = '700';
                    t.style.fontSize = '10px';
                } else if (adjSet.has(nc)) {
                    c.setAttribute('r', 9);
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
                    line.style.strokeWidth = '2.5';
                    line.style.stroke = continentColors[n.continent] || '#212529';
                    line.removeAttribute('stroke-dasharray');
                } else {
                    line.style.opacity = '0.05';
                }
            });

            const adjNames = adj
                .map(c => nodeByCode[c]?.name || c)
                .sort();
            const capitalHtml = n.capital
                ? `<div style="display:flex;justify-content:space-between;gap:8px;color:#6c757d">
                    <span>Capital</span><span style="color:#212529;font-weight:500">${n.capital}${n.capitalNative && n.capitalNative !== n.capital ? ` <span style="color:#868e96">(${n.capitalNative})</span>` : ''}</span></div>`
                : '';

            tooltip.innerHTML = `
                <div style="font-weight:700;margin-bottom:2px">${n.name} <span style="color:#6c757d;font-weight:400">${n.code}</span></div>
                <div style="color:#6c757d;margin-bottom:4px">${n.continent}</div>
                ${capitalHtml}
                <div style="display:flex;justify-content:space-between;gap:8px;color:#6c757d">
                    <span>Lat / Lon</span><span style="color:#212529;font-weight:500">${n.lat.toFixed(1)}° / ${n.lon.toFixed(1)}°</span></div>
                <div style="display:flex;justify-content:space-between;gap:8px;color:#6c757d">
                    <span>Land borders</span><span style="color:#212529;font-weight:500">${adj.length}</span></div>
                ${adjNames.length > 0 ? `<div style="margin-top:4px;font-size:.68rem;color:#868e96;line-height:1.4">Neighbors: ${adjNames.join(', ')}</div>` : '<div style="margin-top:4px;font-size:.68rem;color:#868e96">No land borders (island)</div>'}`;
            tooltip.style.opacity = '1';
            positionTooltip(evt);
        }

        function onLeave() {
            state.hoveredIdx = null;
            nodesG.querySelectorAll('g').forEach(g => {
                g.style.opacity = '1';
                const c = g.querySelector('circle');
                const t = g.querySelector('text');
                c.setAttribute('r', 6);
                c.setAttribute('stroke', '#fff');
                c.setAttribute('stroke-width', '1.5');
                c.style.filter = '';
                t.style.fill = '#495057';
                t.style.fontWeight = '500';
                t.style.fontSize = '8px';
                t.style.opacity = labelsChk.checked ? '1' : '0';
            });
            edgesG.querySelectorAll('line').forEach(line => {
                const sc = line.dataset.source, tc = line.dataset.target;
                const visibleCodes = getVisibleCodes();
                const bothVisible = visibleCodes.has(sc) && visibleCodes.has(tc);
                line.style.strokeWidth = '1.2';
                line.style.opacity = bothVisible ? '0.4' : '0.12';
                line.style.stroke = '#adb5bd';
                if (!bothVisible) line.setAttribute('stroke-dasharray', '3,3');
                else line.removeAttribute('stroke-dasharray');
            });
            tooltip.style.opacity = '0';
            if (searchBox.value) drawNodes();
        }

        function positionTooltip(evt) {
            const rect = svg.getBoundingClientRect();
            let tx = evt.clientX - rect.left + 14;
            let ty = evt.clientY - rect.top - 10;
            if (tx + 330 > rect.width) tx = evt.clientX - rect.left - 335;
            if (ty < 10) ty = 10;
            tooltip.style.left = tx + 'px';
            tooltip.style.top = ty + 'px';
        }

        function updateStats() {
            const visible = getVisibleNodes();
            const visibleCodes = getVisibleCodes();
            const visibleEdges = edges.filter(e => visibleCodes.has(e.source) && visibleCodes.has(e.target));
            const isolated = visible.filter(n => (adjacency[n.code] || []).length === 0).length;
            statsEl.textContent = `${visible.length} countries · ${visibleEdges.length} borders · ${isolated} island`;
        }

        function fullRedraw() {
            state.transform = { x: 0, y: 0, scale: 1 };
            rootG.setAttribute('transform', '');
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

        continentSel.addEventListener('change', () => {
            state.activeContinent = continentSel.value || '';
            fullRedraw();
        });
        labelsChk.addEventListener('change', () => {
            nodesG.querySelectorAll('text').forEach(t => { t.style.opacity = labelsChk.checked ? '1' : '0'; });
        });
        searchBox.addEventListener('input', () => drawNodes());

        fullRedraw();
    }
};
