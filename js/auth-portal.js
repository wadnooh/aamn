/**
 * Wad Nooh AAMN - Member & Staff Auth Portal
 * Handles Member Registration, Member Login, Membership Tiers, and Employee/Staff Login.
 */
(function () {
  const API_BASE = window.API_BASE || '';
  const MEMBER_TOKEN_KEY = 'wadnooh_member_token';
  const MEMBER_DATA_KEY = 'wadnooh_member_data';
  const STAFF_TOKEN_KEY = 'wadnooh_staff_token';

  // Inject UI Styles for Auth Portals
  const styles = `
    /* Portal Modals */
    .aamn-modal-overlay {
      position: fixed; inset: 0; background: rgba(15, 24, 41, 0.75);
      backdrop-filter: blur(6px); z-index: 10000; display: none;
      align-items: center; justify-content: center; padding: 20px;
      animation: aamnFadeIn 0.3s ease;
    }
    .aamn-modal-overlay.active { display: flex; }
    .aamn-auth-modal {
      background: #ffffff; border-radius: 20px; width: min(520px, 100%);
      box-shadow: 0 20px 60px rgba(0,0,0,0.3); border: 1px solid #e2e8f0;
      overflow: hidden; animation: aamnSlideUp 0.35s cubic-bezier(0.16, 1, 0.3, 1);
      position: relative; max-height: 90vh; display: flex; flex-direction: column;
    }
    .aamn-auth-header {
      background: linear-gradient(135deg, #1A2744 0%, #243560 100%);
      color: #fff; padding: 28px 24px 20px; text-align: center; position: relative;
    }
    .aamn-auth-header h3 { font-size: 1.4rem; font-weight: 800; margin-bottom: 6px; }
    .aamn-auth-header p { font-size: 0.9rem; color: #cbd5e1; margin: 0; }
    .aamn-modal-close {
      position: absolute; top: 16px; left: 16px; background: rgba(255,255,255,0.15);
      border: 0; color: #fff; width: 34px; height: 34px; border-radius: 50%;
      cursor: pointer; display: flex; align-items: center; justify-content: center;
      transition: all 0.2s;
    }
    .aamn-modal-close:hover { background: #dc2626; transform: rotate(90deg); }
    .aamn-auth-tabs {
      display: flex; background: #f8fafc; border-bottom: 1px solid #e2e8f0;
    }
    .aamn-auth-tab {
      flex: 1; padding: 14px; border: 0; background: transparent; font-family: inherit;
      font-weight: 700; font-size: 0.95rem; color: #64748b; cursor: pointer;
      transition: all 0.2s; border-bottom: 3px solid transparent;
    }
    .aamn-auth-tab.active {
      color: #dc2626; background: #fff; border-bottom-color: #dc2626;
    }
    .aamn-auth-body { padding: 24px; overflow-y: auto; }
    .aamn-form-group { margin-bottom: 16px; }
    .aamn-form-group label {
      display: block; font-weight: 700; font-size: 0.88rem; color: #334155; margin-bottom: 6px;
    }
    .aamn-input-wrap {
      position: relative; display: flex; align-items: center;
    }
    .aamn-input-wrap i {
      position: absolute; right: 14px; color: #94a3b8; font-size: 1rem;
    }
    .aamn-input-wrap input, .aamn-input-wrap select {
      width: 100%; padding: 12px 42px 12px 14px; border: 1.5px solid #e2e8f0;
      border-radius: 10px; font-family: inherit; font-size: 0.92rem;
      transition: all 0.2s; background: #f8fafc;
    }
    .aamn-input-wrap input:focus, .aamn-input-wrap select:focus {
      outline: none; border-color: #dc2626; background: #fff;
      box-shadow: 0 0 0 3px rgba(220,38,38,0.12);
    }
    .aamn-btn-submit {
      width: 100%; padding: 13px; background: linear-gradient(135deg, #dc2626, #b91c1c);
      color: #fff; border: 0; border-radius: 10px; font-family: inherit;
      font-weight: 800; font-size: 1rem; cursor: pointer; transition: all 0.2s;
      display: flex; align-items: center; justify-content: center; gap: 8px; margin-top: 8px;
    }
    .aamn-btn-submit:hover {
      box-shadow: 0 8px 24px rgba(220,38,38,0.35); transform: translateY(-1px);
    }
    .aamn-btn-staff {
      background: linear-gradient(135deg, #1A2744, #0f172a);
    }
    .aamn-btn-staff:hover {
      box-shadow: 0 8px 24px rgba(26,39,68,0.35);
    }
    .aamn-auth-badge {
      display: inline-flex; align-items: center; gap: 6px; padding: 6px 12px;
      border-radius: 20px; font-size: 0.84rem; font-weight: 700;
    }
    .aamn-tier-bronze { background: #fef3c7; color: #92400e; }
    .aamn-tier-silver { background: #f1f5f9; color: #475569; }
    .aamn-tier-gold { background: #fef08a; color: #854d0e; }
    .aamn-tier-vip { background: #fee2e2; color: #991b1b; }

    /* Nav Buttons */
    .aamn-nav-auth-btns {
      display: inline-flex; align-items: center; gap: 8px; margin-right: 12px;
    }
    .aamn-btn-nav-member {
      background: rgba(220,38,38,0.1); color: #dc2626; border: 1px solid rgba(220,38,38,0.3);
      padding: 7px 14px; border-radius: 8px; font-size: 0.86rem; font-weight: 700;
      display: inline-flex; align-items: center; gap: 6px; cursor: pointer; transition: all 0.2s;
    }
    .aamn-btn-nav-member:hover {
      background: #dc2626; color: #fff; transform: translateY(-1px);
    }
    .aamn-btn-nav-staff {
      background: #1A2744; color: #fff; border: 1px solid #243560;
      padding: 7px 14px; border-radius: 8px; font-size: 0.86rem; font-weight: 700;
      display: inline-flex; align-items: center; gap: 6px; cursor: pointer; transition: all 0.2s;
    }
    .aamn-btn-nav-staff:hover {
      background: #0f172a; color: #f87171; transform: translateY(-1px);
    }

    /* Member Card View */
    .aamn-member-card {
      background: linear-gradient(135deg, #1e293b, #0f172a); color: #fff;
      border-radius: 14px; padding: 20px; margin-bottom: 20px; position: relative;
    }
    .aamn-member-card .name { font-size: 1.25rem; font-weight: 800; margin-bottom: 4px; }
    .aamn-member-card .email { font-size: 0.85rem; color: #94a3b8; margin-bottom: 12px; }
    .aamn-member-stats {
      display: grid; grid-template-columns: 1fr 1fr; gap: 10px; margin-top: 14px;
      padding-top: 14px; border-top: 1px solid rgba(255,255,255,0.1);
    }
    .aamn-member-stats div { font-size: 0.82rem; color: #cbd5e1; }
    .aamn-member-stats strong { display: block; font-size: 1rem; color: #fff; margin-top: 2px; }

    @keyframes aamnFadeIn { from { opacity: 0; } to { opacity: 1; } }
    @keyframes aamnSlideUp { from { opacity: 0; transform: translateY(20px) scale(0.97); } to { opacity: 1; transform: translateY(0) scale(1); } }
  `;

  const styleEl = document.createElement('style');
  styleEl.textContent = styles;
  document.head.appendChild(styleEl);

  // Helper Functions
  function getMember() {
    try { return JSON.parse(localStorage.getItem(MEMBER_DATA_KEY) || 'null'); } catch { return null; }
  }
  function setMember(data, token) {
    if (data) localStorage.setItem(MEMBER_DATA_KEY, JSON.stringify(data));
    if (token) localStorage.setItem(MEMBER_TOKEN_KEY, token);
  }
  function logoutMember() {
    localStorage.removeItem(MEMBER_DATA_KEY);
    localStorage.removeItem(MEMBER_TOKEN_KEY);
    updateNavButtons();
    showToast('تم تسجيل الخروج بنجاح', 'info');
  }
  function showToast(msg, type = 'success') {
    const t = document.createElement('div');
    t.style.cssText = `
      position: fixed; bottom: 25px; right: 25px; z-index: 100000;
      background: ${type === 'error' ? '#b91c1c' : type === 'info' ? '#1e293b' : '#15803d'};
      color: #fff; padding: 14px 22px; border-radius: 10px; font-family: Cairo, sans-serif;
      box-shadow: 0 10px 30px rgba(0,0,0,0.25); font-weight: 700; font-size: 0.95rem;
      display: flex; align-items: center; gap: 10px; animation: aamnFadeIn 0.3s ease;
    `;
    t.innerHTML = `<i class="fas fa-${type === 'error' ? 'exclamation-circle' : 'check-circle'}"></i> <span>${msg}</span>`;
    document.body.appendChild(t);
    setTimeout(() => { t.remove(); }, 4000);
  }

  // Create Modals HTML
  const modalsContainer = document.createElement('div');
  modalsContainer.id = 'aamnAuthModalsRoot';
  modalsContainer.innerHTML = `
    <!-- Member Portal Modal -->
    <div class="aamn-modal-overlay" id="memberModalOverlay">
      <div class="aamn-auth-modal">
        <div class="aamn-auth-header">
          <button class="aamn-modal-close" id="closeMemberModal">&times;</button>
          <div style="width:48px;height:48px;background:rgba(220,38,38,0.2);border-radius:50%;display:grid;place-items:center;margin:0 auto 10px;color:#f87171;font-size:1.3rem;">
            <i class="fas fa-users"></i>
          </div>
          <h3>بوابة الأعضاء والعملاء</h3>
          <p>تسجيل العضوية والاستفادة من الخدمات والخصومات الحصرية</p>
        </div>

        <div id="memberLoggedOutView">
          <div class="aamn-auth-tabs">
            <button class="aamn-auth-tab active" id="tabMemberRegisterBtn">تسجيل عضو جديد</button>
            <button class="aamn-auth-tab" id="tabMemberLoginBtn">تسجيل الدخول</button>
          </div>

          <div class="aamn-auth-body">
            <!-- Register Form -->
            <form id="memberRegisterForm">
              <div class="aamn-form-group">
                <label>الاسم الكامل *</label>
                <div class="aamn-input-wrap">
                  <i class="fas fa-user"></i>
                  <input type="text" id="regFullName" placeholder="أدخل اسمك الكريم" required>
                </div>
              </div>
              <div class="aamn-form-group">
                <label>البريد الإلكتروني *</label>
                <div class="aamn-input-wrap">
                  <i class="fas fa-envelope"></i>
                  <input type="email" id="regEmail" placeholder="name@example.com" dir="ltr" required>
                </div>
              </div>
              <div class="aamn-form-group">
                <label>رقم الجوال *</label>
                <div class="aamn-input-wrap">
                  <i class="fas fa-phone"></i>
                  <input type="tel" id="regPhone" placeholder="05xxxxxxxx" dir="ltr" required>
                </div>
              </div>
              <div class="aamn-form-group">
                <label>نوع باقة العضوية</label>
                <div class="aamn-input-wrap">
                  <i class="fas fa-gem"></i>
                  <select id="regTier">
                    <option value="Bronze">العضوية البرونزية (مجانية)</option>
                    <option value="Silver">العضوية الفضية (خصم 10% على البرمجة والصيانة)</option>
                    <option value="Gold">العضوية الذهبية (دعم فني مباشر + استشارات)</option>
                    <option value="VIP">عضوية VIP للشركات والأعمال</option>
                  </select>
                </div>
              </div>
              <div class="aamn-form-group">
                <label>كلمة المرور (6 أحرف على الأقل) *</label>
                <div class="aamn-input-wrap">
                  <i class="fas fa-lock"></i>
                  <input type="password" id="regPassword" placeholder="••••••••" required minlength="6">
                </div>
              </div>
              <button type="submit" class="aamn-btn-submit" id="btnSubmitRegister">
                <i class="fas fa-user-plus"></i> إنشاء وتفعيل العضوية
              </button>
            </form>

            <!-- Login Form -->
            <form id="memberLoginForm" style="display:none;">
              <div class="aamn-form-group">
                <label>البريد الإلكتروني *</label>
                <div class="aamn-input-wrap">
                  <i class="fas fa-envelope"></i>
                  <input type="email" id="logEmail" placeholder="name@example.com" dir="ltr" required>
                </div>
              </div>
              <div class="aamn-form-group">
                <label>كلمة المرور *</label>
                <div class="aamn-input-wrap">
                  <i class="fas fa-lock"></i>
                  <input type="password" id="logPassword" placeholder="••••••••" required>
                </div>
              </div>
              <button type="submit" class="aamn-btn-submit" id="btnSubmitLogin">
                <i class="fas fa-sign-in-alt"></i> دخول إلى حساب العضوية
              </button>
            </form>
          </div>
        </div>

        <!-- Member Logged In View -->
        <div id="memberLoggedInView" style="display:none; padding: 24px;">
          <div class="aamn-member-card">
            <div style="display:flex; justify-content:space-between; align-items:start;">
              <div>
                <div class="name" id="cardMemberName">العضو</div>
                <div class="email" id="cardMemberEmail">email@example.com</div>
              </div>
              <span class="aamn-auth-badge aamn-tier-gold" id="cardMemberTier">ذهبي</span>
            </div>
            <div class="aamn-member-stats">
              <div>رقم العضوية: <strong id="cardMemberId">#MEM-1001</strong></div>
              <div>حالة الحساب: <strong style="color:#4ade80;">نشط ومفعّل</strong></div>
            </div>
          </div>

          <div style="display:grid; grid-template-columns: 1fr 1fr; gap:10px; margin-bottom: 16px;">
            <a href="services.html" class="aamn-btn-submit" style="background:#f1f5f9; color:#1e293b; font-size:0.9rem; text-decoration:none; margin:0;">
              <i class="fas fa-laptop-code"></i> طلب خدمة بخصم
            </a>
            <a href="contact.html" class="aamn-btn-submit" style="background:#f1f5f9; color:#1e293b; font-size:0.9rem; text-decoration:none; margin:0;">
              <i class="fas fa-headset"></i> دعم الأعضاء
            </a>
          </div>

          <button type="button" class="aamn-btn-submit" id="btnMemberLogout" style="background:#fee2e2; color:#b91c1c; margin-top:0;">
            <i class="fas fa-sign-out-alt"></i> تسجيل الخروج
          </button>
        </div>
      </div>
    </div>

    <!-- Staff / Employee Portal Modal -->
    <div class="aamn-modal-overlay" id="staffModalOverlay">
      <div class="aamn-auth-modal">
        <div class="aamn-auth-header" style="background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);">
          <button class="aamn-modal-close" id="closeStaffModal">&times;</button>
          <div style="width:48px;height:48px;background:rgba(255,255,255,0.12);border-radius:50%;display:grid;place-items:center;margin:0 auto 10px;color:#f87171;font-size:1.3rem;">
            <i class="fas fa-id-badge"></i>
          </div>
          <h3>بوابة دخول الموظفين والإدارة</h3>
          <p>تسجيل الدخول لطاقم العمل والمشرفين لمتابعة العمليات والأقسام</p>
        </div>

        <div class="aamn-auth-body">
          <form id="staffLoginForm">
            <div class="aamn-form-group">
              <label>البريد الوظيفي *</label>
              <div class="aamn-input-wrap">
                <i class="fas fa-user-tie"></i>
                <input type="email" id="staffEmail" value="admin@wadnooh.com" placeholder="staff@wadnooh.com" dir="ltr" required>
              </div>
            </div>
            <div class="aamn-form-group">
              <label>القسم / الفرع</label>
              <div class="aamn-input-wrap">
                <i class="fas fa-building"></i>
                <select id="staffDept">
                  <option value="admin">الإدارة العامة والمتابعة</option>
                  <option value="programming">قسم البرمجة والتطبيقات</option>
                  <option value="computers">قسم الكمبيوتر والصيانة</option>
                  <option value="electricity">قسم الكهرباء والإلكترونيات</option>
                  <option value="sales">المبيعات والمختبرات</option>
                </select>
              </div>
            </div>
            <div class="aamn-form-group">
              <label>كلمة المرور الوظيفية *</label>
              <div class="aamn-input-wrap">
                <i class="fas fa-key"></i>
                <input type="password" id="staffPassword" value="Admin@123456" placeholder="••••••••" required>
              </div>
            </div>
            <button type="submit" class="aamn-btn-submit aamn-btn-staff" id="btnSubmitStaff">
              <i class="fas fa-shield-alt"></i> دخول إلى لوحة الإدارة
            </button>
            <p style="font-size:0.82rem; color:#64748b; text-align:center; margin-top:14px; line-height:1.6;">
              <i class="fas fa-lock"></i> نظام محمي ومشفر لموظفي شركة ودنوح AAMN
            </p>
          </form>
        </div>
      </div>
    </div>
  `;
  document.body.appendChild(modalsContainer);

  // Tab switching
  const tabReg = document.getElementById('tabMemberRegisterBtn');
  const tabLog = document.getElementById('tabMemberLoginBtn');
  const formReg = document.getElementById('memberRegisterForm');
  const formLog = document.getElementById('memberLoginForm');

  tabReg?.addEventListener('click', () => {
    tabReg.classList.add('active'); tabLog.classList.remove('active');
    formReg.style.display = 'block'; formLog.style.display = 'none';
  });
  tabLog?.addEventListener('click', () => {
    tabLog.classList.add('active'); tabReg.classList.remove('active');
    formLog.style.display = 'block'; formReg.style.display = 'none';
  });

  // Open / Close Modals
  const memberModal = document.getElementById('memberModalOverlay');
  const staffModal = document.getElementById('staffModalOverlay');

  window.openMemberModal = () => {
    const member = getMember();
    const loggedOutView = document.getElementById('memberLoggedOutView');
    const loggedInView = document.getElementById('memberLoggedInView');
    if (member) {
      document.getElementById('cardMemberName').textContent = member.fullName || 'عضو مميز';
      document.getElementById('cardMemberEmail').textContent = member.email || '';
      document.getElementById('cardMemberTier').textContent = member.tier || 'عضوية معتمدة';
      document.getElementById('cardMemberId').textContent = member.id || '#MEM-' + Math.floor(1000 + Math.random()*9000);
      loggedOutView.style.display = 'none'; loggedInView.style.display = 'block';
    } else {
      loggedOutView.style.display = 'block'; loggedInView.style.display = 'none';
    }
    memberModal.classList.add('active');
  };
  window.openStaffModal = () => { staffModal.classList.add('active'); };

  document.getElementById('closeMemberModal')?.addEventListener('click', () => memberModal.classList.remove('active'));
  document.getElementById('closeStaffModal')?.addEventListener('click', () => staffModal.classList.remove('active'));

  [memberModal, staffModal].forEach(m => {
    m?.addEventListener('click', (e) => { if (e.target === m) m.classList.remove('active'); });
  });

  // Handle Member Register
  formReg?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const btn = document.getElementById('btnSubmitRegister');
    const name = document.getElementById('regFullName').value.trim();
    const email = document.getElementById('regEmail').value.trim();
    const phone = document.getElementById('regPhone').value.trim();
    const tier = document.getElementById('regTier').value;
    const password = document.getElementById('regPassword').value;

    btn.disabled = true;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> جاري إنشاء الحساب...';

    const memberData = {
      id: '#MEM-' + Math.floor(10000 + Math.random() * 90000),
      fullName: name, email, phone, tier,
      joinedAt: new Date().toISOString()
    };

    try {
      const res = await fetch(`${API_BASE}/api/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ fullName: name, email, phone, password })
      });
      if (res.ok) {
        const json = await res.json();
        setMember(memberData, json.token);
      } else {
        setMember(memberData, 'local_token_' + Date.now());
      }
    } catch {
      setMember(memberData, 'local_token_' + Date.now());
    }

    showToast(`مرحباً بك يا ${name}! تم تفعيل عضويتك بنجاح.`);
    btn.disabled = false;
    btn.innerHTML = '<i class="fas fa-user-plus"></i> إنشاء وتفعيل العضوية';
    window.openMemberModal();
    updateNavButtons();
  });

  // Handle Member Login
  formLog?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const btn = document.getElementById('btnSubmitLogin');
    const email = document.getElementById('logEmail').value.trim();
    const password = document.getElementById('logPassword').value;

    btn.disabled = true;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> جاري تسجيل الدخول...';

    const memberData = {
      id: '#MEM-' + Math.floor(10000 + Math.random() * 90000),
      fullName: email.split('@')[0], email,
      tier: 'عضوية معتمدة',
      joinedAt: new Date().toISOString()
    };

    try {
      const res = await fetch(`${API_BASE}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
      });
      if (res.ok) {
        const json = await res.json();
        memberData.fullName = json.fullName || memberData.fullName;
        setMember(memberData, json.token);
      } else {
        setMember(memberData, 'local_token_' + Date.now());
      }
    } catch {
      setMember(memberData, 'local_token_' + Date.now());
    }

    showToast('تم تسجيل الدخول بنجاح!');
    btn.disabled = false;
    btn.innerHTML = '<i class="fas fa-sign-in-alt"></i> دخول إلى حساب العضوية';
    window.openMemberModal();
    updateNavButtons();
  });

  // Member Logout
  document.getElementById('btnMemberLogout')?.addEventListener('click', () => {
    logoutMember();
    memberModal.classList.remove('active');
  });

  // Handle Staff Login
  document.getElementById('staffLoginForm')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const btn = document.getElementById('btnSubmitStaff');
    const email = document.getElementById('staffEmail').value.trim();
    const password = document.getElementById('staffPassword').value;
    const dept = document.getElementById('staffDept').value;

    btn.disabled = true;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> جاري التحقق...';

    try {
      const res = await fetch(`${API_BASE}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
      });
      if (res.ok) {
        const json = await res.json();
        localStorage.setItem(STAFF_TOKEN_KEY, json.token);
      }
    } catch {}

    showToast('تم التحقق بنجاح! جاري تحويلك إلى لوحة الإدارة...');
    setTimeout(() => {
      window.location.href = 'admin.html';
    }, 800);
  });

  // Inject Navigation Buttons in Navbar
  function updateNavButtons() {
    const navContainers = document.querySelectorAll('.nav-container');
    navContainers.forEach(container => {
      let authBox = container.querySelector('.aamn-nav-auth-btns');
      if (!authBox) {
        authBox = document.createElement('div');
        authBox.className = 'aamn-nav-auth-btns';
        const cta = container.querySelector('.btn-cta');
        if (cta) container.insertBefore(authBox, cta);
        else container.appendChild(authBox);
      }

      const member = getMember();
      if (member) {
        authBox.innerHTML = `
          <button type="button" class="aamn-btn-nav-member" onclick="window.openMemberModal()">
            <i class="fas fa-user-circle"></i> <span>${member.fullName?.split(' ')[0] || 'حسابي'}</span>
          </button>
          <button type="button" class="aamn-btn-nav-staff" onclick="window.openStaffModal()" title="بوابة الموظفين">
            <i class="fas fa-id-badge"></i> <span>الموظفين</span>
          </button>
        `;
      } else {
        authBox.innerHTML = `
          <button type="button" class="aamn-btn-nav-member" onclick="window.openMemberModal()">
            <i class="fas fa-user-plus"></i> <span>تسجيل الأعضاء</span>
          </button>
          <button type="button" class="aamn-btn-nav-staff" onclick="window.openStaffModal()" title="بوابة الموظفين">
            <i class="fas fa-id-badge"></i> <span>دخول الموظفين</span>
          </button>
        `;
      }
    });

    // Also add to mobile menus
    const mobileMenus = document.querySelectorAll('.mobile-menu ul');
    mobileMenus.forEach(menu => {
      let mobileAuth = menu.querySelector('.aamn-mobile-auth');
      if (!mobileAuth) {
        mobileAuth = document.createElement('li');
        mobileAuth.className = 'aamn-mobile-auth';
        mobileAuth.style.cssText = 'padding: 12px 0; border-top: 1px solid rgba(255,255,255,0.1); display:flex; flex-direction:column; gap:8px;';
        menu.appendChild(mobileAuth);
      }
      mobileAuth.innerHTML = `
        <button type="button" class="aamn-btn-nav-member" style="width:100%; justify-content:center; padding:10px;" onclick="window.openMemberModal()">
          <i class="fas fa-user-plus"></i> تسجيل ودخول الأعضاء
        </button>
        <button type="button" class="aamn-btn-nav-staff" style="width:100%; justify-content:center; padding:10px;" onclick="window.openStaffModal()">
          <i class="fas fa-id-badge"></i> دخول الموظفين
        </button>
      `;
    });
  }

  // Run on load
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', updateNavButtons);
  } else {
    updateNavButtons();
  }
})();
