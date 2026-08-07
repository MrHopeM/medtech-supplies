// MedTech Supplies — motion engine + interop helpers
window.medtech = {
  _io: null, _parallaxBound: false,

  // Called after each page render; everything is guarded/idempotent.
  boot() {
    this.hideLoader();
    this.initReveal();
    this.initTilt();
    this.initParallax();
    this.initHero();
    this.initRotator();
  },

  // ---- Scroll reveals + counters ----
  initReveal() {
    document.documentElement.classList.add('js');
    if (this._io) this._io.disconnect();
    this._io = new IntersectionObserver((entries, obs) => {
      entries.forEach(e => {
        if (!e.isIntersecting) return;
        e.target.classList.add('in');
        if (e.target.classList.contains('counter')) this.count(e.target);
        obs.unobserve(e.target);
      });
    }, { threshold: 0.12 });
    document.querySelectorAll('.reveal, .counter').forEach(el => this._io.observe(el));
  },

  count(el) {
    const target = parseFloat(el.dataset.target) || 0;
    const suffix = el.dataset.suffix || '';
    const dur = 1400, start = performance.now();
    const step = now => {
      const p = Math.min((now - start) / dur, 1);
      const eased = 1 - Math.pow(1 - p, 3);
      el.textContent = Math.floor(eased * target).toLocaleString('en-ZA') + suffix;
      if (p < 1) requestAnimationFrame(step);
    };
    requestAnimationFrame(step);
  },

  // ---- 3D tilt + glow ----
  initTilt() {
    document.querySelectorAll('.tilt').forEach(el => {
      if (el._tilt) return; el._tilt = true;
      const max = 7;
      el.addEventListener('pointermove', e => {
        if (window.matchMedia('(pointer:coarse)').matches) return;
        const r = el.getBoundingClientRect();
        const px = (e.clientX - r.left) / r.width - 0.5;
        const py = (e.clientY - r.top) / r.height - 0.5;
        el.style.transform = `perspective(900px) rotateX(${(-py * max).toFixed(2)}deg) rotateY(${(px * max).toFixed(2)}deg) translateY(-6px)`;
        el.style.setProperty('--gx', (px * 100 + 50) + '%');
        el.style.setProperty('--gy', (py * 100 + 50) + '%');
        el.classList.add('tilting');
      });
      el.addEventListener('pointerleave', () => {
        el.style.transform = '';
        el.classList.remove('tilting');
      });
    });
  },

  // ---- Parallax ----
  initParallax() {
    if (this._parallaxBound) return; this._parallaxBound = true;
    const apply = () => {
      const y = window.scrollY;
      document.querySelectorAll('[data-parallax]').forEach(el => {
        const s = parseFloat(el.dataset.parallax) || 0.2;
        el.style.transform = `translate3d(0, ${(y * s).toFixed(1)}px, 0)`;
      });
    };
    window.addEventListener('scroll', apply, { passive: true });
    apply();
  },

  // ---- Animated hero particle network ----
  initHero() {
    const c = document.getElementById('heroCanvas');
    if (!c || c._init) return; c._init = true;
    const ctx = c.getContext('2d');
    const DPR = Math.min(window.devicePixelRatio || 1, 2);
    let w, h, pts;
    const resize = () => {
      w = c.clientWidth; h = c.clientHeight;
      c.width = w * DPR; c.height = h * DPR; ctx.setTransform(DPR, 0, 0, DPR, 0, 0);
      const n = Math.max(28, Math.min(80, Math.floor(w / 16)));
      pts = Array.from({ length: n }, () => ({
        x: Math.random() * w, y: Math.random() * h,
        vx: (Math.random() - 0.5) * 0.45, vy: (Math.random() - 0.5) * 0.45
      }));
    };
    resize();
    window.addEventListener('resize', resize);
    const tick = () => {
      ctx.clearRect(0, 0, w, h);
      for (const p of pts) {
        p.x += p.vx; p.y += p.vy;
        if (p.x < 0 || p.x > w) p.vx *= -1;
        if (p.y < 0 || p.y > h) p.vy *= -1;
      }
      for (let i = 0; i < pts.length; i++) {
        for (let j = i + 1; j < pts.length; j++) {
          const a = pts[i], b = pts[j], dx = a.x - b.x, dy = a.y - b.y;
          const d = Math.hypot(dx, dy);
          if (d < 130) {
            ctx.strokeStyle = `rgba(45,212,191,${0.16 * (1 - d / 130)})`;
            ctx.lineWidth = 1;
            ctx.beginPath(); ctx.moveTo(a.x, a.y); ctx.lineTo(b.x, b.y); ctx.stroke();
          }
        }
      }
      for (const p of pts) {
        ctx.fillStyle = 'rgba(125,233,255,.75)';
        ctx.beginPath(); ctx.arc(p.x, p.y, 1.7, 0, 7); ctx.fill();
      }
      requestAnimationFrame(tick);
    };
    tick();
  },

  // ---- Rotating headline word ----
  initRotator() {
    const el = document.getElementById('heroRot');
    if (!el || el._rot) return; el._rot = true;
    const words = (el.dataset.words || '').split('|').filter(Boolean);
    if (!words.length) return;
    let i = 0; el.textContent = words[0];
    setInterval(() => {
      el.classList.add('rot-out');
      setTimeout(() => {
        i = (i + 1) % words.length;
        el.textContent = words[i];
        el.classList.remove('rot-out');
        el.classList.add('rot-in');
        setTimeout(() => el.classList.remove('rot-in'), 450);
      }, 350);
    }, 2600);
  },

  hideLoader() {
    const l = document.getElementById('pageLoader');
    if (!l || l._hiding) return;
    l._hiding = true;
    const minMs = 1300; // keep the spinner on screen long enough to be seen
    const wait = Math.max(0, minMs - performance.now());
    setTimeout(() => {
      l.classList.add('hide');
      setTimeout(() => l.remove(), 700);
    }, wait);
  },

  toast(msg, success) {
    let el = document.querySelector('.toast');
    if (!el) { el = document.createElement('div'); el.className = 'toast'; document.body.appendChild(el); }
    el.textContent = msg;
    el.classList.toggle('success', !!success);
    el.classList.add('show');
    clearTimeout(this._t);
    this._t = setTimeout(() => el.classList.remove('show'), 2600);
  },

  chatScroll() {
    const el = document.getElementById('chatBody');
    if (el) requestAnimationFrame(() => { el.scrollTop = el.scrollHeight; });
  },

  // Back-compat: older pages call initEngage
  initEngage() { this.boot(); }
};
