// Navegación: el hover mueve el indicador flotante, SIN tocar la clase 'hovered' del ítem activo
(function(){
    const items = document.querySelectorAll('.navigation li');
    if (items.length){
        items.forEach(li => {
            li.addEventListener('mouseenter', () => {
                // NO tocamos 'hovered' — Razor ya marcó el ítem activo correcto
                // Solo mostramos el indicador flotante (manejado en layout script)
            });
        });
    }
})();

// Menu toggle (sidebar)
(function(){
    const toggle = document.querySelector('.toggle');
    const navigation = document.querySelector('.navigation');
    const main = document.querySelector('.main');
    if (toggle && navigation && main){
        toggle.onclick = () => {
            navigation.classList.toggle('active');
            main.classList.toggle('active');
        };
    }
})();

// Dark mode toggle
(function(){
    const themeToggle = document.getElementById('themeToggle');
    const body = document.body;
    const moonIcon = document.querySelector('.moon-icon');
    const sunIcon = document.querySelector('.sun-icon');
    const applyTheme = (theme) => {
        if (theme === 'light'){
            body.setAttribute('data-theme','light');
            if (moonIcon) moonIcon.style.display='block';
            if (sunIcon) sunIcon.style.display='none';
        } else {
            body.removeAttribute('data-theme');
            if (moonIcon) moonIcon.style.display='none';
            if (sunIcon) sunIcon.style.display='block';
        }
    };
    const current = localStorage.getItem('theme') || 'dark';
    applyTheme(current);
    if (themeToggle){
        themeToggle.addEventListener('click', ()=>{
            const next = body.getAttribute('data-theme') === 'light' ? 'dark' : 'light';
            localStorage.setItem('theme', next);
            applyTheme(next);
            if (window.updateChartTheme) updateChartTheme();
        });
    }
})();

// Números animados en tarjetas
(function(){
    const cardBox = document.querySelector('.cardBox');
    if (!cardBox) return;
    const animateValue = (obj, start, end, duration, prefix, finalText) => {
        let startTimestamp = null;
        const step = (timestamp) => {
            if (!startTimestamp) startTimestamp = timestamp;
            const progress = Math.min((timestamp - startTimestamp) / duration, 1);
            const ease = 1 - Math.pow(1 - progress, 4);
            // Funciona tanto para subir (start < end) como para bajar (start > end)
            const current = Math.round(start + ease * (end - start));
            obj.innerHTML = prefix + current.toLocaleString('es-MX');
            if (progress < 1) {
                requestAnimationFrame(step);
            } else {
                obj.innerHTML = finalText;
            }
        };
        requestAnimationFrame(step);
    };
    const observer = new IntersectionObserver(entries => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.querySelectorAll('.numbers').forEach(num => {
                    const finalText = num.innerText.trim();
                    const prefix = finalText.includes('$') ? '$' : '';
                    // Valor final (destino de la animación)
                    const dataVal = num.getAttribute('data-value');
                    const numericStr = dataVal !== null && dataVal !== ''
                        ? dataVal
                        : finalText.replace(/[^0-9.]/g, '');
                    const endVal = parseFloat(numericStr) || 0;
                    // Valor inicial: data-from si existe, si no 0
                    const dataFrom = num.getAttribute('data-from');
                    const startVal = dataFrom !== null ? parseFloat(dataFrom) : 0;
                    const targetVal = Math.floor(endVal);
                    if (startVal !== targetVal) {
                        animateValue(num, startVal, targetVal, 1200, prefix, finalText);
                    }
                });
                observer.unobserve(entry.target);
            }
        });
    }, {threshold: 0.1});
    observer.observe(cardBox);
})();

// Charts: cargar solo si existen los canvas
(function(){
    const body=document.body;
    const sales = document.getElementById('salesChart');
    if (sales && window.Chart){
        Chart.defaults.font.family="'Ubuntu', sans-serif";
        window.salesChartInstance = new Chart(sales, {
            type:'line',
            data:{labels:['Lun','Mar','Mié','Jue','Vie','Sáb'], datasets:[{label:'Ventas Semanales ($)', data:[12500,15000,14200,18450,22000,25000], backgroundColor:'rgba(199,110,0,0.2)', borderColor:'#C76E00', borderWidth:3, tension:0.4, fill:true, pointBackgroundColor:'#C76E00', pointBorderColor:'#fff', pointBorderWidth:2, pointRadius:4, pointHoverRadius:6}]},
            options:{responsive:true, maintainAspectRatio:false, plugins:{legend:{display:false}}, scales:{y:{beginAtZero:true, grid:{color:getComputedStyle(body).getPropertyValue('--border').trim()||'#333', drawBorder:false}, ticks:{color:getComputedStyle(body).getPropertyValue('--text-secondary').trim()||'#a3a3a3', callback:v=>'$'+v.toLocaleString() }}, x:{grid:{display:false, drawBorder:false}, ticks:{color:getComputedStyle(body).getPropertyValue('--text-secondary').trim()||'#a3a3a3'}}}, interaction:{intersect:false, mode:'index'}}
        });
    }
    const stock = document.getElementById('stockChart');
    if (stock && window.Chart){
        const isLight=body.getAttribute('data-theme')==='light';
        const textColor=isLight?'#64748b':'#a3a3a3';
        window.stockChartInstance = new Chart(stock, {
            type:'doughnut',
            data:{labels:['Tornillería','Material Pesado','Eléctrico','Herramientas','Pinturas'], datasets:[{data:[35,20,15,25,5], backgroundColor:['#f97316','#3b82f6','#10b981','#f59e0b','#ef4444'], borderWidth:0, hoverOffset:4}]},
            options:{responsive:true, maintainAspectRatio:false, cutout:'70%', plugins:{legend:{position:'right', labels:{color:textColor, font:{size:12, family:"'Ubuntu', sans-serif"}, padding:20}}, tooltip:{backgroundColor:'rgba(0,0,0,0.8)', displayColors:true}}}
        });
    }
})();

function updateChartTheme(){
    const body=document.body;
    const isLight=body.getAttribute('data-theme')==='light';
    const textColor=isLight?'#64748b':'#a3a3a3';
    const gridColor=isLight?'#e2e8f0':'#333333';
    if(window.salesChartInstance){
        salesChartInstance.options.scales.x.ticks.color=textColor;
        salesChartInstance.options.scales.y.ticks.color=textColor;
        salesChartInstance.options.scales.y.grid.color=gridColor;
        salesChartInstance.update();
    }
    if(window.stockChartInstance){
        stockChartInstance.options.plugins.legend.labels.color=textColor;
        stockChartInstance.update();
    }
}
