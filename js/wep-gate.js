/**
 * Shared auth gate for AAMN Software & Computer (satellite pages).
 * Token key matches wadnooh-eng.js: localStorage 'wadnooh_token'
 */
(function (global) {
  const API_BASE = '/api';
  const TOKEN_KEY = 'wadnooh_token';

  function lang() {
    return localStorage.getItem('wadnooh_lang') === 'en' ? 'en' : 'ar';
  }

  function token() {
    return localStorage.getItem(TOKEN_KEY) || '';
  }

  function isLoggedIn() {
    return !!token();
  }

  function copy() {
    const ar = lang() === 'ar';
    return {
      lockTitle: ar ? 'سجّل الدخول للمتابعة' : 'Sign in to continue',
      lockBody: ar
        ? 'حساب مجاني للمهندسين والطلاب: دورات، مكتبة، مشاريع، مركز OSH، ومساعد هندسي — بلا رسوم للبدء.'
        : 'Free account for engineers & students: courses, library, projects, OSH center, and AI assistant — no fee to start.',
      register: ar ? 'إنشاء حساب مجاني' : 'Create free account',
      login: ar ? 'دخول' : 'Login',
      logout: ar ? 'خروج' : 'Logout',
      password: ar ? 'كلمة المرور' : 'Password',
      fullName: ar ? 'الاسم الكامل' : 'Full name',
      phone: ar ? 'الهاتف' : 'Phone',
      cancel: ar ? 'إلغاء' : 'Cancel',
      preview: ar ? 'معاينة بعد التسجيل' : 'Preview after signup',
      guestBanner: ar
        ? 'انضم مجاناً وافتح كامل محتوى البوابة التقنية.'
        : 'Join free and unlock the full engineering portal.'
    };
  }

  function ensureStyles() {
    if (document.getElementById('wep-gate-css')) return;
    const css = document.createElement('style');
    css.id = 'wep-gate-css';
    css.textContent = `
      :root {
        --wep-space-1: 4px; --wep-space-2: 8px; --wep-space-3: 12px; --wep-space-4: 16px;
        --wep-space-5: 24px; --wep-space-6: 32px; --wep-space-7: 48px;
        --wep-section-y: clamp(24px, 4vw, 44px); --wep-section-x: clamp(14px, 3.5vw, 36px);
      }
      .wep-auth-banner {
        display: flex; flex-wrap: wrap; gap: var(--wep-space-3); align-items: center;
        justify-content: space-between; padding: 12px 16px; margin-bottom: var(--wep-space-4);
        background: rgba(5,61,71,.06); border: 1px solid var(--line, #cfc7b8);
        border-inline-start: 3px solid var(--kush, #c4a35a);
      }
      .wep-auth-banner p { margin: 0; color: var(--muted, #5c6a70); font-size: .92rem; flex: 1; min-width: 180px; }
      .wep-auth-banner .actions { display: flex; gap: 8px; flex-wrap: wrap; }
      .wep-lock-shell { position: relative; min-height: 160px; }
      .wep-lock-shell.is-locked .wep-lock-body { filter: blur(5px); opacity: .45; pointer-events: none; user-select: none; }
      .wep-lock-overlay {
        position: absolute; inset: 0; z-index: 5; display: grid; place-items: center;
        padding: 16px; background: linear-gradient(180deg, rgba(247,244,238,.55), rgba(247,244,238,.92));
      }
      .wep-lock-shell:not(.is-locked) .wep-lock-overlay { display: none; }
      .wep-lock-card {
        max-width: 420px; text-align: center; background: #fff;
        border: 1px solid var(--line, #cfc7b8); border-top: 3px solid var(--kush, #c4a35a);
        padding: 20px 18px;
      }
      .wep-lock-card h3 {
        font-family: var(--font-display, inherit); color: var(--nile-deep, #053d47);
        margin: 0 0 8px; font-size: 1.15rem;
      }
      .wep-lock-card p { color: var(--muted, #5c6a70); font-size: .9rem; margin: 0 0 14px; }
      .wep-lock-actions { display: flex; gap: 8px; justify-content: center; flex-wrap: wrap; }
      .wep-preview-blur {
        background: #fff; border: 1px solid var(--line, #cfc7b8); padding: 14px; margin-bottom: 10px;
        border-inline-start: 3px solid var(--kush, #c4a35a);
      }
      .wep-preview-blur h4 { color: var(--nile-deep, #053d47); margin-bottom: 6px; }
      .wep-preview-blur p { color: var(--muted, #5c6a70); font-size: .88rem; margin: 0; }
      .wep-modal-overlay {
        display: none; position: fixed; inset: 0; z-index: 200; place-items: center; padding: 16px;
        background: rgba(5,30,36,.55);
      }
      .wep-modal-overlay.open { display: grid; }
      .wep-modal {
        width: min(420px, 100%); background: var(--surface, #f7f4ee);
        border-top: 4px solid var(--kush, #c4a35a); padding: 20px;
      }
      .wep-modal h3 { font-family: var(--font-display, inherit); color: var(--nile-deep, #053d47); margin-bottom: 12px; }
      .wep-modal .fg { margin-bottom: 10px; }
      .wep-modal label { display: block; font-size: .85rem; color: var(--muted, #5c6a70); margin-bottom: 4px; font-weight: 600; }
      .wep-modal input {
        width: 100%; padding: 10px 12px; border: 1px solid var(--line, #cfc7b8); font: inherit; box-sizing: border-box;
      }
      .wep-modal .row { display: flex; gap: 8px; justify-content: flex-end; margin-top: 14px; flex-wrap: wrap; }
      .wep-btn {
        display: inline-flex; align-items: center; justify-content: center; padding: 9px 14px;
        border: none; cursor: pointer; font: inherit; font-weight: 600; text-decoration: none;
        background: var(--nile-deep, #053d47); color: #f5f0e6;
      }
      .wep-btn.secondary { background: var(--kush-deep, #9a7a35); color: #fff; }
      .wep-btn.outline { background: transparent; color: var(--nile-deep, #053d47); border: 1px solid var(--nile-deep, #053d47); }
      .wep-msg { font-size: .88rem; margin-top: 8px; min-height: 1.2em; }
      .wep-msg.ok { color: var(--palm, #2f6b4f); }
      .wep-msg.err { color: #8b2e2e; }
    `;
    document.head.appendChild(css);
  }

  function ensureModals() {
    if (document.getElementById('wepLoginModal')) return;
    const c = copy();
    const wrap = document.createElement('div');
    wrap.innerHTML = `
      <div id="wepLoginModal" class="wep-modal-overlay" role="dialog" aria-modal="true">
        <div class="wep-modal">
          <h3>${c.login}</h3>
          <div class="fg"><label>Email</label><input id="wepLoginEmail" type="email" autocomplete="username"></div>
          <div class="fg"><label>${c.password}</label><input id="wepLoginPassword" type="password" autocomplete="current-password"></div>
          <div id="wepLoginMsg" class="wep-msg"></div>
          <div class="row">
            <button type="button" class="wep-btn outline" data-wep-close="wepLoginModal">${c.cancel}</button>
            <button type="button" class="wep-btn secondary" id="wepDoLogin">${c.login}</button>
          </div>
        </div>
      </div>
      <div id="wepRegisterModal" class="wep-modal-overlay" role="dialog" aria-modal="true">
        <div class="wep-modal">
          <h3>${c.register}</h3>
          <div class="fg"><label>${c.fullName}</label><input id="wepRegName" type="text" autocomplete="name"></div>
          <div class="fg"><label>Email</label><input id="wepRegEmail" type="email" autocomplete="email"></div>
          <div class="fg"><label>${c.phone}</label><input id="wepRegPhone" type="tel" autocomplete="tel"></div>
          <div class="fg"><label>${c.password}</label><input id="wepRegPassword" type="password" autocomplete="new-password"></div>
          <div id="wepRegMsg" class="wep-msg"></div>
          <div class="row">
            <button type="button" class="wep-btn outline" data-wep-close="wepRegisterModal">${c.cancel}</button>
            <button type="button" class="wep-btn secondary" id="wepDoRegister">${c.register}</button>
          </div>
        </div>
      </div>`;
    document.body.appendChild(wrap);
    wrap.querySelectorAll('[data-wep-close]').forEach((btn) => {
      btn.addEventListener('click', () => closeModal(btn.getAttribute('data-wep-close')));
    });
    wrap.querySelectorAll('.wep-modal-overlay').forEach((ov) => {
      ov.addEventListener('click', (e) => { if (e.target === ov) ov.classList.remove('open'); });
    });
    document.getElementById('wepDoLogin')?.addEventListener('click', doLogin);
    document.getElementById('wepDoRegister')?.addEventListener('click', doRegister);
  }

  function openModal(id) {
    ensureModals();
    document.getElementById(id)?.classList.add('open');
  }

  function closeModal(id) {
    document.getElementById(id)?.classList.remove('open');
  }

  function openLogin() { openModal('wepLoginModal'); }
  function openRegister() { openModal('wepRegisterModal'); }

  async function api(path, options) {
    const headers = { 'Content-Type': 'application/json', ...(options?.headers || {}) };
    const tok = token();
    if (tok) headers.Authorization = `Bearer ${tok}`;
    const res = await fetch(`${API_BASE}${path}`, { ...options, headers });
    let data = null;
    const text = await res.text();
    if (text) { try { data = JSON.parse(text); } catch { data = text; } }
    if (!res.ok) {
      const message = typeof data === 'string' ? data : (data?.message || data?.title || data?.detail || 'Error');
      throw new Error(message);
    }
    return data;
  }

  async function doLogin() {
    const msg = document.getElementById('wepLoginMsg');
    try {
      const result = await api('/auth/login', {
        method: 'POST',
        body: JSON.stringify({
          email: document.getElementById('wepLoginEmail').value.trim(),
          password: document.getElementById('wepLoginPassword').value
        })
      });
      localStorage.setItem(TOKEN_KEY, result.token);
      if (msg) { msg.className = 'wep-msg ok'; msg.textContent = lang() === 'en' ? 'Welcome' : 'مرحباً'; }
      setTimeout(() => {
        closeModal('wepLoginModal');
        global.dispatchEvent(new CustomEvent('wep:auth', { detail: { user: result.user } }));
      }, 400);
    } catch (err) {
      if (msg) { msg.className = 'wep-msg err'; msg.textContent = err.message; }
    }
  }

  async function doRegister() {
    const msg = document.getElementById('wepRegMsg');
    try {
      const result = await api('/auth/register', {
        method: 'POST',
        body: JSON.stringify({
          fullName: document.getElementById('wepRegName').value.trim(),
          email: document.getElementById('wepRegEmail').value.trim(),
          phone: document.getElementById('wepRegPhone').value.trim(),
          password: document.getElementById('wepRegPassword').value
        })
      });
      localStorage.setItem(TOKEN_KEY, result.token);
      if (msg) { msg.className = 'wep-msg ok'; msg.textContent = lang() === 'en' ? 'Registered' : 'تم التسجيل'; }
      setTimeout(() => {
        closeModal('wepRegisterModal');
        global.dispatchEvent(new CustomEvent('wep:auth', { detail: { user: result.user } }));
      }, 400);
    } catch (err) {
      if (msg) { msg.className = 'wep-msg err'; msg.textContent = err.message; }
    }
  }

  function logout() {
    localStorage.removeItem(TOKEN_KEY);
    global.dispatchEvent(new CustomEvent('wep:auth', { detail: { user: null } }));
  }

  function lockHtml() {
    const c = copy();
    return `<div class="wep-lock-overlay">
      <div class="wep-lock-card">
        <h3>${c.lockTitle}</h3>
        <p>${c.lockBody}</p>
        <div class="wep-lock-actions">
          <button type="button" class="wep-btn secondary" data-wep-reg>${c.register}</button>
          <button type="button" class="wep-btn outline" data-wep-login>${c.login}</button>
        </div>
      </div>
    </div>`;
  }

  function previewCards(n) {
    const c = copy();
    return Array.from({ length: n || 2 }, (_, i) =>
      `<div class="wep-preview-blur" aria-hidden="true"><h4>${c.preview} ${i + 1}</h4><p>${c.lockBody}</p></div>`
    ).join('');
  }

  function mountBanner(el) {
    if (!el) return;
    const c = copy();
    if (isLoggedIn()) {
      el.innerHTML = `<div class="wep-auth-banner"><p>${lang() === 'en' ? 'Signed in — content unlocked.' : 'أنت مسجّل — المحتوى مفتوح.'}</p>
        <div class="actions"><button type="button" class="wep-btn outline" data-wep-logout>${c.logout}</button>
        <a class="wep-btn" href="/">${lang() === 'en' ? 'Portal home' : 'الرئيسية'}</a></div></div>`;
      el.querySelector('[data-wep-logout]')?.addEventListener('click', logout);
    } else {
      el.innerHTML = `<div class="wep-auth-banner"><p>${c.guestBanner}</p>
        <div class="actions">
          <button type="button" class="wep-btn secondary" data-wep-reg>${c.register}</button>
          <button type="button" class="wep-btn outline" data-wep-login>${c.login}</button>
        </div></div>`;
      el.querySelector('[data-wep-reg]')?.addEventListener('click', openRegister);
      el.querySelector('[data-wep-login]')?.addEventListener('click', openLogin);
    }
  }

  function applyLocks(root) {
    ensureStyles();
    ensureModals();
    const locked = !isLoggedIn();
    (root || document).querySelectorAll('[data-wep-lock]').forEach((shell) => {
      shell.classList.add('wep-lock-shell');
      shell.classList.toggle('is-locked', locked);
      let body = shell.querySelector('.wep-lock-body');
      if (!body) {
        body = document.createElement('div');
        body.className = 'wep-lock-body';
        while (shell.firstChild) body.appendChild(shell.firstChild);
        shell.appendChild(body);
      }
      let ov = shell.querySelector('.wep-lock-overlay');
      if (!ov) {
        shell.insertAdjacentHTML('beforeend', lockHtml());
        ov = shell.querySelector('.wep-lock-overlay');
      }
      if (locked && body && !body.dataset.preview) {
        const n = parseInt(shell.getAttribute('data-wep-lock') || '2', 10);
        body.innerHTML = previewCards(n);
        body.dataset.preview = '1';
      }
      if (!locked) delete body.dataset.preview;
    });
    document.querySelectorAll('[data-wep-reg]').forEach((b) => {
      b.onclick = openRegister;
    });
    document.querySelectorAll('[data-wep-login]').forEach((b) => {
      b.onclick = openLogin;
    });
  }

  function init() {
    ensureStyles();
    ensureModals();
  }

  global.WepGate = {
    API_BASE,
    isLoggedIn,
    token,
    lang,
    openLogin,
    openRegister,
    logout,
    applyLocks,
    mountBanner,
    lockHtml,
    previewCards,
    init,
    api
  };
})(window);
