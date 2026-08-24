/**
 * AAMN Software & Computer Platform (AAMN) — Phase 1 client
 * Institutional Arabic EdTech: education + research + training + career path.
 * Roles scaffold (full server RBAC → Phase 2):
 */
const ROLES = Object.freeze({
  SystemAdmin: 'SystemAdmin',
  Supervisor: 'Supervisor',
  Author: 'Author',
  Teacher: 'Teacher',
  TA: 'TA',
  Student: 'Student',
  Guest: 'Guest'
});

/** Permission map: role → UI affordance keys */
const ROLE_PERMISSIONS = Object.freeze({
  [ROLES.SystemAdmin]: ['admin', 'users', 'departments', 'courses', 'articles', 'projects', 'publish', 'teach', 'grade', 'enroll', 'library', 'ai', 'dashboard'],
  [ROLES.Supervisor]: ['admin', 'users', 'departments', 'courses', 'articles', 'projects', 'publish', 'teach', 'grade', 'enroll', 'library', 'ai', 'dashboard'],
  [ROLES.Author]: ['courses', 'articles', 'projects', 'publish', 'library', 'ai', 'enroll', 'dashboard'],
  [ROLES.Teacher]: ['courses', 'teach', 'grade', 'articles', 'library', 'ai', 'enroll', 'dashboard'],
  [ROLES.TA]: ['teach', 'grade', 'library', 'ai', 'enroll', 'dashboard'],
  [ROLES.Student]: ['enroll', 'library', 'ai', 'dashboard'],
  [ROLES.Guest]: ['library', 'ai', 'browse']
});

function resolveUiRole(user) {
  if (!user) return ROLES.Guest;
  const roles = user.roles || [];
  if (roles.includes('Admin')) return ROLES.SystemAdmin;
  if (roles.includes('Teacher')) return ROLES.Teacher;
  return ROLES.Student;
}

function can(role, perm) {
  return (ROLE_PERMISSIONS[role] || []).includes(perm);
}

const API_BASE = '/api';
const PROGRESS_KEY_OLD = 'wadnooh_academy_progress';
const PROGRESS_KEY = 'wadnooh_eng_progress';
const NOTES_KEY = 'wadnooh_eng_notes';
const SAVED_KEY = 'wadnooh_eng_saved';
const CAL_KEY = 'wadnooh_eng_calendar';
const NEWSLETTER_KEY = 'wadnooh_eng_newsletter';
const ROLE_KEY = 'wadnooh_eng_role_override';
const LECTURES_LS_PREFIX = 'wadnooh_eng_lectures_';

let authToken = localStorage.getItem('wadnooh_token') || '';
let currentUser = null;
let pendingDemoPaymentId = null;
let activeDepartment = 'all';
let openCourseId = null;
let activeLesson = null;
let universitiesData = [];
let universitiesLoaded = false;
let DEPARTMENTS = [];
let COURSES = [];
let PATHS = [];
let ARTICLES = [];
let NEWS = [];
let PROJECTS = [];
let PROJECT_CATS = [];
let LIBRARY = [];
let FAQS = [];
let SUCCESS = [];
let PARTNERS = [];
let ROADMAP = [];
let EVENTS = [];
let QBANKS = [];
let ACCREDITATIONS = [];
let catalogReady = false;
let projectCatFilter = 'all';
let contentUnlocked = false;
let mineSubTab = 'progress';
let memberLectures = [];
let lecturesLoaded = false;
let lectureSearchQ = '';
let lectureTagFilter = '';
let lecturesApiOk = true;

/** Public specialty name teasers (no catalog fetch while logged out). */
const GUEST_DEPT_TEASERS = [
  { id: 'electronics', icon: '⚡', name: { ar: 'إلكترونيات', en: 'Electronics' } },
  { id: 'electrical', icon: '🔌', name: { ar: 'كهربائية', en: 'Electrical' } },
  { id: 'civil', icon: '🏗', name: { ar: 'مدنية', en: 'Civil' } },
  { id: 'mechanical', icon: '⚙', name: { ar: 'ميكانيكية', en: 'Mechanical' } },
  { id: 'computer', icon: '💻', name: { ar: 'حاسوب', en: 'Computer' } },
  { id: 'osh', icon: '🦺', name: { ar: 'سلامة مهنية OSH', en: 'OSH / HSE' } },
  { id: 'chemical', icon: '🧪', name: { ar: 'كيميائية', en: 'Chemical' } },
  { id: 'architecture', icon: '🏛', name: { ar: 'عمارة', en: 'Architecture' } }
];

const GUEST_FAQ_TEASERS = [
  {
    q: { ar: 'هل التسجيل مجاني؟', en: 'Is registration free?' },
    a: { ar: 'نعم — أنشئ حساباً مجانياً وافتح الدورات والمكتبة والمساعد التقني.', en: 'Yes — create a free account to unlock courses, library, and the AI assistant.' }
  },
  {
    q: { ar: 'لمن هذه البوابة؟', en: 'Who is this portal for?' },
    a: { ar: 'طلاب تقنية، خريجون، مهندسون ميدانيون، وكليات تبحث عن منظومة عربية.', en: 'Engineering students, graduates, field engineers, and colleges seeking an Arabic ecosystem.' }
  },
  {
    q: { ar: 'ماذا أحصل بعد التسجيل؟', en: 'What do I get after signup?' },
    a: { ar: 'كتالوج الدورات، مشاريع، مكتبات، مركز OSH، جامعات، ولوحة تقدّم شخصية.', en: 'Course catalog, projects, libraries, OSH center, universities, and a personal progress board.' }
  }
];

function isAuthed() {
  return !!(authToken && (currentUser || contentUnlocked));
}

function isEmailVerified() {
  if (!currentUser) return false;
  if (currentUser.isAdmin || (currentUser.roles || []).some(r => String(r).toLowerCase() === 'admin'))
    return true;
  return !!currentUser.emailConfirmed;
}

function canUnlockCatalog() {
  return !!(authToken && isEmailVerified());
}

function ensureVerifyBannerEl() {
  let el = document.getElementById('emailVerifyBanner');
  if (el) return el;
  const header = document.querySelector('header.site-header, header');
  el = document.createElement('div');
  el.id = 'emailVerifyBanner';
  el.hidden = true;
  el.setAttribute('role', 'status');
  if (header && header.parentNode) header.insertAdjacentElement('afterend', el);
  else document.body.prepend(el);
  return el;
}

function applyVerifyBanner() {
  const el = ensureVerifyBannerEl();
  const show = !!(authToken && currentUser && !isEmailVerified());
  el.hidden = !show;
  if (!show) {
    el.innerHTML = '';
    return;
  }
  el.innerHTML = `<div class="email-verify-banner">
    <p>${escapeHtml(t('verifyBanner'))}</p>
    <div class="actions">
      <button type="button" class="btn btn-secondary btn-sm" id="btnResendVerify">${escapeHtml(t('verifyResend'))}</button>
      <a class="btn btn-outline btn-sm" href="/verify.html">${escapeHtml(t('verifyPage'))}</a>
    </div>
    <div id="verifyBannerMsg" class="email-verify-msg"></div>
  </div>`;
  document.getElementById('btnResendVerify')?.addEventListener('click', resendVerification);
}

async function resendVerification() {
  const msg = document.getElementById('verifyBannerMsg') || document.getElementById('registerMsg');
  try {
    const r = await api('/auth/resend-verification', {
      method: 'POST',
      body: JSON.stringify({ email: currentUser?.email || '' })
    });
    let text = r.message || t('verifyCheckInbox');
    if (r.devVerifyUrl) text += ` — ${r.devVerifyUrl}`;
    if (r.devVerifyCode) text += ` · ${r.devVerifyCode}`;
    if (msg && msg.id === 'verifyBannerMsg') msg.textContent = text;
    else setMsg('registerMsg', text, 'success');
  } catch (err) {
    if (msg && msg.id === 'verifyBannerMsg') msg.textContent = err.message;
    else setMsg('registerMsg', err.message, 'error');
  }
}

async function handleVerifyQuery() {
  const params = new URLSearchParams(location.search);
  const token = params.get('verify') || params.get('token');
  if (!token || !location.pathname.endsWith('index.html') && location.pathname !== '/' && !location.pathname.endsWith('/'))
    return;
  // Prefer dedicated page for cleaner UX when landing with ?verify=
  if (params.get('verify') || params.get('token')) {
    try {
      await api(`/auth/confirm-email?token=${encodeURIComponent(token)}`, { method: 'POST', body: '{}' });
      if (authToken) await refreshAuth();
      alert(t('verifyDone'));
      const url = new URL(location.href);
      url.searchParams.delete('verify');
      url.searchParams.delete('token');
      history.replaceState({}, '', url.pathname + url.search + url.hash);
    } catch (err) {
      location.href = `/verify.html?token=${encodeURIComponent(token)}`;
    }
  }
}

function gateCardHtml() {
  return `<div class="auth-lock-overlay">
    <div class="auth-lock-card">
      <h3>${escapeHtml(t('gateTitle'))}</h3>
      <p>${escapeHtml(t('gateBody'))}</p>
      <div class="auth-lock-actions">
        <button type="button" class="btn btn-secondary" onclick="openModal('registerModal')">${escapeHtml(t('gateRegister'))}</button>
        <button type="button" class="btn btn-outline" onclick="openModal('loginModal')">${escapeHtml(t('gateLogin'))}</button>
      </div>
    </div>
  </div>`;
}

function fakePreviewHtml(n) {
  const count = n || 2;
  return Array.from({ length: count }, (_, i) => `
    <div class="card auth-preview-blur" aria-hidden="true">
      <h4>${escapeHtml(t('gatePreview'))} ${i + 1}</h4>
      <p>${escapeHtml(t('gateBody'))}</p>
    </div>`).join('');
}

function applySectionGates() {
  document.querySelectorAll('[data-auth-gate]').forEach((sec) => {
    const locked = !contentUnlocked;
    sec.classList.toggle('is-auth-locked', locked);
    let ov = sec.querySelector(':scope > .auth-lock-overlay');
    if (locked && !ov) {
      sec.insertAdjacentHTML('beforeend', gateCardHtml());
    } else if (locked && ov) {
      ov.outerHTML = gateCardHtml();
    } else if (!locked && ov) {
      ov.remove();
    }
    const body = sec.querySelector('[data-auth-body]');
    if (body && locked && !catalogReady) {
      const n = parseInt(sec.getAttribute('data-auth-gate') || '2', 10);
      if (!body.dataset.hasPreview) {
        body.innerHTML = `<div class="grid">${fakePreviewHtml(n)}</div>`;
        body.dataset.hasPreview = '1';
      }
    }
    if (body && !locked) delete body.dataset.hasPreview;
  });
  const band = document.getElementById('signupBand');
  if (band) band.hidden = contentUnlocked;
  const searchInput = document.getElementById('smartSearch');
  if (searchInput) {
    searchInput.placeholder = contentUnlocked
      ? t('searchSmartPh')
      : (currentLang === 'en' ? 'Sign in to search the catalog…' : 'سجّل الدخول للبحث في الكتالوج…');
  }
}

function clearCatalogState() {
  DEPARTMENTS = []; COURSES = []; PATHS = []; ARTICLES = []; NEWS = [];
  PROJECTS = []; PROJECT_CATS = []; LIBRARY = []; FAQS = []; SUCCESS = [];
  PARTNERS = []; ROADMAP = []; EVENTS = []; QBANKS = []; ACCREDITATIONS = [];
  universitiesData = []; universitiesLoaded = false; catalogReady = false;
}

function renderGuestMarketing() {
  const depts = document.getElementById('landingDepts');
  if (depts) {
    depts.innerHTML = GUEST_DEPT_TEASERS.map(d => `
      <a class="dept-chip dept-chip-teaser" href="/specialty.html?dept=${encodeURIComponent(d.id)}" style="text-decoration:none;color:inherit;display:block">
        <span class="dept-ico" aria-hidden="true">${d.icon || '◆'}</span>
        <strong>${escapeHtml(loc(d.name))}</strong>
        <span>${escapeHtml(t('guestDeptHint'))}</span>
      </a>`).join('');
  }
  const faq = document.getElementById('landingFaq');
  if (faq) {
    faq.innerHTML = GUEST_FAQ_TEASERS.map(f => `
      <details class="faq-item">
        <summary>${escapeHtml(loc(f.q))}</summary>
        <p>${escapeHtml(loc(f.a))}</p>
      </details>`).join('') +
      `<p class="gate-faq-more">${escapeHtml(t('faqTeaserMore'))} —
        <button type="button" class="btn btn-secondary btn-sm" onclick="openModal('registerModal')">${escapeHtml(t('gateRegister'))}</button>
      </p>`;
  }
  ['landingCourses', 'landingArticles', 'landingProjects', 'landingPaths', 'landingQbank', 'landingEvents',
    'landingNews', 'coursesResults',
    'pathsResults', 'libraryResults', 'projectsResults', 'universitiesResults', 'mineResults',
    'portfolioResults', 'aiBooks', 'aiSources'].forEach((id) => {
    const el = document.getElementById(id);
    if (el && !contentUnlocked) {
      el.innerHTML = el.classList.contains('grid') || el.classList.contains('grid-3') || id.startsWith('landing')
        ? fakePreviewHtml(2)
        : gateCardHtml();
    }
  });
  const success = document.getElementById('landingSuccess');
  if (success && !contentUnlocked) success.innerHTML = '';
  const partners = document.getElementById('landingPartners');
  if (partners && !contentUnlocked) partners.innerHTML = '';
  const acc = document.getElementById('landingAccreditations');
  if (acc && !contentUnlocked) acc.innerHTML = '';
  const reply = document.getElementById('aiReply');
  if (reply && !contentUnlocked) reply.innerHTML = gateCardHtml();
  updateStats();
  applySectionGates();
}

async function unlockAndLoad() {
  if (!canUnlockCatalog()) {
    contentUnlocked = false;
    applySectionGates();
    applyVerifyBanner();
    renderGuestMarketing();
    return;
  }
  contentUnlocked = true;
  applySectionGates();
  applyVerifyBanner();
  await loadCatalog();
  await loadUniversities();
}

function lockContent() {
  contentUnlocked = false;
  clearCatalogState();
  renderGuestMarketing();
  applyAuthUi();
}

let currentLang = (() => {
  const saved = localStorage.getItem('wadnooh_lang');
  if (saved === 'en' || saved === 'ar') return saved;
  const q = new URLSearchParams(location.search).get('lang');
  if (q === 'en' || q === 'ar') return q;
  return (navigator.language || '').toLowerCase().startsWith('en') ? 'en' : 'ar';
})();

const I18N = {
  ar: {
        brand: 'ودنوح AAMN للبرمجيات والكمبيوتر',
        brandEnSmall: 'AAMN Software & Computer',
        heroHeadline: 'Code • Repair • Connect • Support',
        heroLead: 'منظومة رقمية عربية للتقنية: تعليم + بحث + ابتكار + وظائف — للطلاب والكليات والمهندسين والجهات الصناعية. ليست مجرد دورات.',
        ctaCourses: 'استكشف الدورات',
        ctaDepts: 'استكشف التخصصات',
        ctaSearch: 'ابحث في البوابة',
        ctaOsh: 'مركز السلامة OSH',
        ctaOshOpen: 'افتح مركز OSH',
        ctaOshPortal: 'بوابة التخصص',
        ctaRegisterFree: 'إنشاء حساب مجاني',
        gateTitle: 'سجّل الدخول للمتابعة',
        gateBody: 'حساب مجاني للمهندسين والطلاب: دورات أكاديمية، مكتبة، مشاريع، مركز OSH، مساعد هندسي، ولوحة تقدّم — ابدأ بلا رسوم.',
        gateRegister: 'إنشاء حساب مجاني',
        gateLogin: 'دخول',
        gatePreview: 'معاينة بعد التسجيل',
        gateSignupBand: 'انضم مجاناً وافتح كامل محتوى البوابة — للطلاب والمهندسين.',
        guestDeptHint: 'أسماء التخصصات للتعرّف — التفاصيل بعد التسجيل',
        faqTeaserMore: 'المزيد من الإجابات بعد إنشاء حساب مجاني',
        verifyBanner: 'أكد بريدك لفتح الكتالوج الكامل.',
        verifyResend: 'إعادة إرسال رابط التأكيد',
        verifyPage: 'صفحة التأكيد',
        verifyCheckInbox: 'تحقق من بريدك — أرسلنا رابط التأكيد.',
        verifyDone: 'تم تأكيد البريد',
        verifyNeedConfirm: 'سجّل الدخول ثم أكّد بريدك لفتح المحتوى.',
        navHome: 'الرئيسية',
        navAcademy: 'الشركة',
        navOsh: 'السلامة OSH',
        navAi: 'المساعد التقني',
        navCerts: 'الشهادات',
        navNews: 'الأخبار',
        navEvents: 'الفعاليات',
        navContact: 'تواصل',
        navMore: 'المزيد',
        secOsh: 'السلامة والصحة المهنية (OSH / HSE)',
        secOshSub: 'مركز متخصص: إدارة سلامة، تقييم مخاطر، PPE، تصاريح عمل، تحقيق حوادث، قوالب، ومراجع ILO/OSHA/ISO.',
        secOshAside: 'يشمل دورات OSHA Awareness وISO 45001 وFire وLOTO والعمل على الارتفاع والأماكن المحصورة وHAZOP.',
        soonResearch: 'مركز بحث',
        soonStore: 'متجر هندسي',
        navCourses: 'الدورات',
        navDepts: 'التخصصات',
        navProjects: 'المشاريع',
        navLibrary: 'المكتبة',
        navUnis: 'الجامعات',
        navPortals: 'بوابات التخصصات',
        openPortals: 'فتح بوابات التخصصات',
        guide: 'عن البوابة',
        adminPanel: 'لوحة التحكم',
        login: 'دخول', register: 'تسجيل', logout: 'خروج',
        password: 'كلمة المرور', fullName: 'الاسم الكامل', phone: 'الهاتف',
        cancel: 'إلغاء', close: 'إغلاق',
        searchSmart: 'بحث موحّد',
        searchSmartPh: 'دورات، كتب، مشاريع، بنوك أسئلة، مقالات، جامعات، سلامة…',
        searchSmartHint: 'بحث دلالي عبر كتالوج البوابة',
        secDepts: 'بوابات التخصصات التقنية',
        secDeptsSub: 'كل تخصص بوابة: خارطة · دورات · مختبرات · كتب · بحث · مشاريع · تحميلات · وظائف · نقاش',
        secCourses: 'أحدث الدورات والموصى بها',
        secCoursesSub: 'نموذج أكاديمي: مخرجات · خطة أسبوعية · محاضرات · مختبرات',
        secArticles: 'أحدث المقالات',
        secArticlesSub: 'محتوى هندسي للطلاب والباحثين والمهندسين',
        secProjects: 'مشاريع مميزة',
        secProjectsSub: 'معرض عملي: كود · تقرير · BOM · خطوات',
        secPaths: 'مسارات كفاءات تقنية',
        secPathsSub: 'تسلسل دورات مبني على كفاءات قابلة للتتبع',
        secVideo: 'تعرّف على البوابة',
        secVideoSub: 'إطار جاهز لتضمين يوتيوب',
        secStats: 'البوابة بالأرقام',
        secSuccess: 'شهادات وتزكيات',
        secSuccessSub: 'قصص متعلمين + اعتمادات البوابة (مخدمة ١ رمزية)',
        secPartners: 'شركاء أكاديميون وصناعيون',
        secPartnersSub: 'مراجع ومعايير — تكامل الجامعات في المخدمة ٣',
        secNews: 'أخبار البوابة',
        secFaq: 'الأسئلة الشائعة',
        secNewsletter: 'النشرة التقنية',
        secNewsletterSub: 'أدخل بريدك للتحديثات (محلي أو عبر الخادم)',
        secQbank: 'بنوك أسئلة (معاينة)',
        secQbankSub: 'محرك الامتحانات الكامل في المخدمة ٢',
        secEvents: 'فعاليات ومؤتمرات',
        secEventsSub: 'ندوات وورش وملتقيات مهنية',
        newsletterBtn: 'اشترك',
        newsletterOk: 'تم تسجيل بريدك',
        newsletterPh: 'you@example.com',
        workspaceTitle: 'مساحة الشركة (LMS خفيف)',
        workspaceSub: 'دورات · مسارات · مكتبة · مشاريع · جامعات · مساعد هندسي · لوحتي · محفظة',
        tabCourses: 'الدورات', tabPaths: 'المسارات', tabLibrary: 'المكتبة',
        tabProjects: 'المشاريع', tabUnis: 'الجامعات', tabMine: 'لوحتي',
        tabPortfolio: 'محفظة تقنية',
        tabAi: 'مساعد هندسي', tabMembership: 'العضوية',
        soonLabs: 'مختبرات',
        soonForum: 'منتدى',
        soonJobs: 'وظائف',
        soonLabel: 'قريباً',
        coursesTitle: 'كتالوج الدورات الشركة',
        pathsTitle: 'مسارات الكفاءات',
        pathsHint: 'اختر مساراً وابدأ الدورات المرتبطة.',
        libraryTitle: 'المكتبة العلمية',
        libraryHint: 'كتب · أوراق · أطروحات · كود/CAD/MATLAB — روابط خارجية',
        projectsTitle: 'مركز المشاريع',
        projectsHint: 'صفّ حسب الفئة. تفاصيل BOM والخطوات في البطاقة.',
        mineTitle: 'لوحة الطالب',
        mineHint: 'دوراتي · محاضراتي · تقدّم · شهادات · ملفات · ملاحظات · تقويم.',
        mineTabProgress: 'تقدّمي',
        mineTabLectures: 'محاضراتي',
        lecTitle: 'محاضراتي',
        lecHint: 'احفظ محاضراتك وملاحظاتك وروابط الملفات — تُزامن مع حسابك.',
        lecAdd: 'إضافة محاضرة',
        lecEdit: 'تعديل',
        lecDelete: 'حذف',
        lecSearchPh: 'بحث في المحاضرات…',
        lecTagPh: 'تصفية بوسم',
        lecEmptyTitle: 'لا محاضرات محفوظة بعد',
        lecEmptyBody: 'أضف أول محاضرة أو احفظ درساً من مقرر عبر «احفظ من مقرر».',
        lecFormAdd: 'إضافة محاضرة',
        lecFormEdit: 'تعديل محاضرة',
        lecTitleAr: 'العنوان (عربي)',
        lecTitleEn: 'العنوان (إنجليزي)',
        lecSubject: 'المادة / المقرر',
        lecDuration: 'المدة (دقائق)',
        lecDate: 'التاريخ',
        lecTags: 'وسوم (مفصولة بفاصلة)',
        lecNotes: 'ملخص / ملاحظات',
        lecAttachUrl: 'رابط مرفق (PDF / فيديو)',
        lecAttachName: 'اسم الملف (اختياري)',
        lecSave: 'حفظ',
        lecSaveFromCourse: 'احفظ من مقرر',
        lecSavedOk: 'تم حفظ المحاضرة في محاضراتي',
        lecDeletedOk: 'تم الحذف',
        lecLocalFallback: 'حُفظت محلياً (الخادم غير متاح مؤقتاً)',
        lecConfirmDelete: 'حذف هذه المحاضرة؟',
        lecOpenAttach: 'فتح المرفق',
        lecCount: 'محاضرة',
        portfolioTitle: 'المحفظة التقنية',
        portfolioHint: 'مشاريع ومهارات وشهادات — أساس ملف مهني',
        unisTitle: 'دليل الجامعات والشركاء',
        unisHint: 'موارد عالمية بروابط رسمية.',
        aiTitle: 'المساعد التقني — بوابة ودنوح',
        aiHint: 'معادلات · كود · دوائر · تحليل مخاطر · مراجع · تلخيص · مشروع تخرج · اختبارات قصيرة.',
        aiPh: 'مثال: اشرح مصفوفة مخاطر أو راجع كود Arduino أو لخّص ISO 45001',
        ask: 'اسأل', findBooks: 'كتب',
        booksTitle: 'كتب مقترحة', openBook: 'فتح', readBook: 'قراءة',
        sourcesTitle: 'مصدر المعرفة',
        aiLoading: 'جاري جلب المعلومات…',
        aiFailed: 'تعذر الاتصال بالمساعد — تأكد أن الخادم يعمل',
        membershipTitle: 'عضوية ودنوح AAMN للبرمجيات والكمبيوتر',
        membershipHint: 'سجّل مجاناً وافتح المحتوى — ثم رقِّ اختيارياً إن رغبت.',
        subscribe: 'اشترك الآن',
        searchLabel: 'بحث', searchPh: 'اسم الدورة أو الكلمة',
        levelLabel: 'المستوى',
        levelAll: 'الكل', levelBeginner: 'مبتدئ', levelIntermediate: 'متوسط', levelAdvanced: 'متقدم',
        catAll: 'الكل',
        enroll: 'الالتحاق', enrolled: 'ملتحق',
        openCourse: 'فتح الدورة', continueLearning: 'متابعة',
        markDone: 'إتمام الدرس', viewPath: 'عرض الدورات',
        noCourses: 'لا توجد نتائج مطابقة',
        noEnrolled: 'لم تلتحق بأي دورة بعد — اختر من الكتالوج.',
        lessons: 'محاضرات', duration: 'المدة', level: 'المستوى',
        progress: 'التقدّم', completed: 'مكتمل',
        objectives: 'الأهداف', prerequisites: 'المتطلبات',
        outcomes: 'مخرجات التعلم', weeklyPlan: 'الخطة الأسبوعية',
        slides: 'شرائح', videos: 'فيديو', labs: 'مختبرات',
        assignmentsLabel: 'واجبات', examsLabel: 'امتحانات',
        sourcesLabel: 'مصادر', refsLabel: 'مراجع',
        discussions: 'نقاشات',
        media: 'الوسائط', hasProject: 'مشروع', hasCert: 'شهادة',
        demoPayTitle: 'إتمام الدفع التجريبي', payNow: 'ادفع الآن',
        loginRequired: 'سجّل الدخول أولاً',
        footerTag: 'تقنية بثقة وجودة',
        footerRoadmap: 'خارطة طريق البوابة',
        statCourses: 'دورات', statDepts: 'تخصصات', statProjects: 'مشاريع',
        statUnis: 'جامعة', statEnrolled: 'ملتحق بها',
        hours: 'ساعات',
        uniSearchLabel: 'بحث', uniSearchPh: 'اسم الجامعة أو الدولة أو التخصص',
        uniRegionLabel: 'المنطقة', uniRegionAll: 'كل المناطق',
        uniFieldLabel: 'المجال', uniFieldAll: 'كل المجالات',
        uniVisit: 'الموقع الرسمي', uniAskAi: 'اسأل المساعد',
        uniLoading: 'جاري تحميل دليل الجامعات…', uniFailed: 'تعذر تحميل الدليل',
        uniNone: 'لا توجد جامعات مطابقة', uniShowing: 'عرض', uniOf: 'من',
        regAfrica: 'أفريقيا', regMiddleEast: 'الشرق الأوسط', regEurope: 'أوروبا',
        regNorthAmerica: 'أمريكا الشمالية', regAsia: 'آسيا', regOceania: 'أوقيانوسيا',
        regLatinAmerica: 'أمريكا اللاتينية',
        fieldGeneral: 'عام', fieldEngineering: 'تقنية', fieldComputer: 'حوسبة',
        fieldMedicine: 'طب', fieldBusiness: 'أعمال', fieldArts: 'فنون وإنسانيات', fieldOpen: 'تعليم مفتوح',
        certsTitle: 'شهادات (رمزية)',
        certsEmpty: 'أكمل دورة بنسبة ١٠٠٪ لظهور شهادة رمزية.',
        savedTitle: 'ملفات محفوظة',
        savedHint: 'احفظ روابط PDF/كود هنا (محلياً).',
        savedAdd: 'إضافة رابط',
        notesTitle: 'ملاحظات سريعة',
        notesSave: 'حفظ الملاحظات',
        calTitle: 'تقويم بسيط',
        calAdd: 'إضافة موعد',
        viewAll: 'عرض الكل',
        openLink: 'فتح',
        codeLink: 'الكود', reportLink: 'التقرير',
        videoPlaceholder: 'ضع رابط يوتيوب لاحقاً — الإطار جاهز للتضمين',
        roleLabel: 'الدور (واجهة)',
        comingSoonHint: 'متاح في المخدمة ٢ / ٣',
        bomLabel: 'قائمة المكونات',
        stepsLabel: 'الخطوات',
        skillsLabel: 'مهارات',
        analyticsProgress: 'متوسط التقدّم',
        analyticsCourses: 'دورات ملتحق بها',
        analyticsCerts: 'شهادات',
        week: 'أسبوع'
  },
  en: {
        brand: 'AAMN Software & Computer',
        brandEnSmall: 'ودنوح AAMN للبرمجيات والكمبيوتر',
        heroHeadline: 'Code • Repair • Connect • Support',
        heroLead: 'An Arabic engineering digital ecosystem: education + research + innovation + jobs — for students, colleges, engineers, and industry. Not just courses.',
        ctaCourses: 'Explore courses',
        ctaDepts: 'Explore specialties',
        ctaSearch: 'Search the portal',
        ctaOsh: 'OSH Safety Center',
        ctaOshOpen: 'Open OSH Center',
        ctaOshPortal: 'Specialty gateway',
        ctaRegisterFree: 'Create free account',
        gateTitle: 'Sign in to continue',
        gateBody: 'Free account for engineers & students: academic courses, library, projects, OSH center, AI assistant, and progress dashboard — no fee to start.',
        gateRegister: 'Create free account',
        gateLogin: 'Login',
        gatePreview: 'Preview after signup',
        gateSignupBand: 'Join free and unlock the full portal — for students and engineers.',
        guestDeptHint: 'Specialty names to explore — details after signup',
        faqTeaserMore: 'More answers after creating a free account',
        verifyBanner: 'Confirm your email to unlock the full catalog.',
        verifyResend: 'Resend verification link',
        verifyPage: 'Verification page',
        verifyCheckInbox: 'Check your inbox — we sent a verification link.',
        verifyDone: 'Email confirmed',
        verifyNeedConfirm: 'Sign in, then confirm your email to unlock content.',
        navHome: 'Home',
        navAcademy: 'Academy',
        navOsh: 'OSH / HSE',
        navAi: 'Engineering AI',
        navCerts: 'Certificates',
        navNews: 'News',
        navEvents: 'Events',
        navContact: 'Contact',
        navMore: 'More',
        secOsh: 'Occupational Safety & Health (OSH / HSE)',
        secOshSub: 'Dedicated center: safety management, risk, PPE, PTW, incidents, templates, and ILO/OSHA/ISO refs.',
        secOshAside: 'Includes OSHA Awareness, ISO 45001, Fire, LOTO, Working at Height, Confined Space, and HAZOP courses.',
        soonResearch: 'Research center',
        soonStore: 'Engineering store',
        navCourses: 'Courses',
        navDepts: 'Specialties',
        navProjects: 'Projects',
        navLibrary: 'Library',
        navUnis: 'Universities',
        navPortals: 'Specialty portals',
        openPortals: 'Open specialty portals',
        guide: 'About',
        adminPanel: 'Admin',
        login: 'Login', register: 'Register', logout: 'Logout',
        password: 'Password', fullName: 'Full name', phone: 'Phone',
        cancel: 'Cancel', close: 'Close',
        searchSmart: 'Unified search',
        searchSmartPh: 'Courses, books, projects, Q-banks, articles, universities…',
        searchSmartHint: 'Semantic search across the platform catalog',
        secDepts: 'Engineering specialty portals',
        secDeptsSub: 'Each specialty gateway: roadmap · courses · labs · books · research · projects · downloads · jobs · discussion',
        secCourses: 'Latest & recommended courses',
        secCoursesSub: 'Academic model: outcomes · weekly plan · lectures · labs',
        secArticles: 'Latest articles',
        secArticlesSub: 'Engineering content for students, researchers, and engineers',
        secProjects: 'Featured projects',
        secProjectsSub: 'Practical hub: code · report · BOM · steps',
        secPaths: 'Competency learning paths',
        secPathsSub: 'Course sequences built on trackable competencies',
        secVideo: 'Meet the Portal',
        secVideoSub: 'YouTube-ready embed frame',
        secStats: 'Portal at a glance',
        secSuccess: 'Testimonials & credentials',
        secSuccessSub: 'Learner stories + Phase-1 placeholder credentials',
        secPartners: 'Academic & industry partners',
        secPartnersSub: 'Standards & references — university integrations in Phase 3',
        secNews: 'Portal news',
        secFaq: 'FAQ',
        secNewsletter: 'Engineering newsletter',
        secNewsletterSub: 'Email for updates (local or API)',
        secQbank: 'Q-banks (teasers)',
        secQbankSub: 'Full exam engine in Phase 2',
        secEvents: 'Events & conferences',
        secEventsSub: 'Webinars, workshops, career meetups',
        newsletterBtn: 'Subscribe',
        newsletterOk: 'Email saved',
        newsletterPh: 'you@example.com',
        workspaceTitle: 'Learning workspace (LMS-lite)',
        workspaceSub: 'Courses · Paths · Library · Projects · Universities · AI tutor · Dashboard · Portfolio',
        tabCourses: 'Courses', tabPaths: 'Paths', tabLibrary: 'Library',
        tabProjects: 'Projects', tabUnis: 'Universities', tabMine: 'My dashboard',
        tabPortfolio: 'Engineering portfolio',
        tabAi: 'Engineering AI', tabMembership: 'Membership',
        soonLabs: 'Labs',
        soonForum: 'Forum',
        soonJobs: 'Jobs',
        soonLabel: 'Soon',
        coursesTitle: 'Academic course catalog',
        pathsTitle: 'Competency paths',
        pathsHint: 'Pick a path and start linked courses.',
        libraryTitle: 'Scientific library',
        libraryHint: 'Books · papers · theses · code/CAD/MATLAB — external links',
        projectsTitle: 'Projects hub',
        projectsHint: 'Filter by category. BOM and steps in the card.',
        mineTitle: 'Student dashboard',
        mineHint: 'Courses · My lectures · Progress · Certificates · Files · Notes · Calendar.',
        mineTabProgress: 'Progress',
        mineTabLectures: 'My lectures',
        lecTitle: 'My lectures',
        lecHint: 'Save lectures, notes, and file links — synced to your account.',
        lecAdd: 'Add lecture',
        lecEdit: 'Edit',
        lecDelete: 'Delete',
        lecSearchPh: 'Search lectures…',
        lecTagPh: 'Filter by tag',
        lecEmptyTitle: 'No saved lectures yet',
        lecEmptyBody: 'Add your first lecture, or save a course lesson with “Save from course”.',
        lecFormAdd: 'Add lecture',
        lecFormEdit: 'Edit lecture',
        lecTitleAr: 'Title (Arabic)',
        lecTitleEn: 'Title (English)',
        lecSubject: 'Subject / course',
        lecDuration: 'Duration (minutes)',
        lecDate: 'Date',
        lecTags: 'Tags (comma-separated)',
        lecNotes: 'Summary / notes',
        lecAttachUrl: 'Attachment URL (PDF / video)',
        lecAttachName: 'Filename (optional)',
        lecSave: 'Save',
        lecSaveFromCourse: 'Save from course',
        lecSavedOk: 'Lecture saved to My lectures',
        lecDeletedOk: 'Deleted',
        lecLocalFallback: 'Saved locally (server temporarily unavailable)',
        lecConfirmDelete: 'Delete this lecture?',
        lecOpenAttach: 'Open attachment',
        lecCount: 'lectures',
        portfolioTitle: 'Engineering portfolio',
        portfolioHint: 'Projects, skills, certificates — career profile foundation',
        unisTitle: 'Universities & partners',
        unisHint: 'Global resources with official links.',
        aiTitle: 'Engineering Copilot — AAMN Portal',
        aiHint: 'Explain · equations · code review · refs · summarize — plus Wikipedia & open books.',
        aiPh: 'e.g. explain Ohm’s law, review Arduino code, or summarize a paper',
        ask: 'Ask', findBooks: 'Books',
        booksTitle: 'Suggested books', openBook: 'Open', readBook: 'Read',
        sourcesTitle: 'Knowledge source',
        aiLoading: 'Fetching information…',
        aiFailed: 'Could not reach the assistant — check that the server is running',
        membershipTitle: 'AAMN Software & Computer membership',
        membershipHint: 'Register free to unlock content — optional upgrades later.',
        subscribe: 'Subscribe',
        searchLabel: 'Search', searchPh: 'Course name or keyword',
        levelLabel: 'Level',
        levelAll: 'All', levelBeginner: 'Beginner', levelIntermediate: 'Intermediate', levelAdvanced: 'Advanced',
        catAll: 'All',
        enroll: 'Enroll', enrolled: 'Enrolled',
        openCourse: 'Open course', continueLearning: 'Continue',
        markDone: 'Mark complete', viewPath: 'View courses',
        noCourses: 'No matching results',
        noEnrolled: 'No enrolled courses yet — pick from the catalog.',
        lessons: 'lectures', duration: 'Duration', level: 'Level',
        progress: 'Progress', completed: 'Completed',
        objectives: 'Objectives', prerequisites: 'Prerequisites',
        outcomes: 'Learning outcomes', weeklyPlan: 'Weekly plan',
        slides: 'Slides', videos: 'Videos', labs: 'Labs',
        assignmentsLabel: 'Assignments', examsLabel: 'Exams',
        sourcesLabel: 'Sources', refsLabel: 'References',
        discussions: 'Discussions',
        media: 'Media', hasProject: 'Project', hasCert: 'Certificate',
        demoPayTitle: 'Complete demo payment', payNow: 'Pay now',
        loginRequired: 'Please log in first',
        footerTag: 'Engineering with the spirit of the Nile',
        footerRoadmap: 'Portal roadmap',
        statCourses: 'Courses', statDepts: 'Specialties', statProjects: 'Projects',
        statUnis: 'Universities', statEnrolled: 'Enrolled',
        hours: 'hours',
        uniSearchLabel: 'Search', uniSearchPh: 'University, country, or field',
        uniRegionLabel: 'Region', uniRegionAll: 'All regions',
        uniFieldLabel: 'Field', uniFieldAll: 'All fields',
        uniVisit: 'Official site', uniAskAi: 'Ask assistant',
        uniLoading: 'Loading university directory…', uniFailed: 'Could not load directory',
        uniNone: 'No matching universities', uniShowing: 'Showing', uniOf: 'of',
        regAfrica: 'Africa', regMiddleEast: 'Middle East', regEurope: 'Europe',
        regNorthAmerica: 'North America', regAsia: 'Asia', regOceania: 'Oceania',
        regLatinAmerica: 'Latin America',
        fieldGeneral: 'General', fieldEngineering: 'Engineering', fieldComputer: 'Computing',
        fieldMedicine: 'Medicine', fieldBusiness: 'Business', fieldArts: 'Arts & humanities', fieldOpen: 'Open learning',
        certsTitle: 'Certificates (placeholder)',
        certsEmpty: 'Complete a course at 100% to show a placeholder certificate.',
        savedTitle: 'Saved files',
        savedHint: 'Save PDF/code links here (local).',
        savedAdd: 'Add link',
        notesTitle: 'Quick notes',
        notesSave: 'Save notes',
        calTitle: 'Simple calendar',
        calAdd: 'Add event',
        viewAll: 'View all',
        openLink: 'Open',
        codeLink: 'Code', reportLink: 'Report',
        videoPlaceholder: 'Drop a YouTube URL later — embed frame is ready',
        roleLabel: 'UI role',
        comingSoonHint: 'Available in Phase 2 / 3',
        bomLabel: 'Bill of materials',
        stepsLabel: 'Steps',
        skillsLabel: 'Skills',
        analyticsProgress: 'Avg. progress',
        analyticsCourses: 'Enrolled courses',
        analyticsCerts: 'Certificates',
        week: 'Week'
  }
};

function t(key) {
  return (I18N[currentLang] && I18N[currentLang][key]) || I18N.ar[key] || key;
}
function loc(obj) {
  if (!obj) return '';
  if (typeof obj === 'string') return obj;
  return obj[currentLang] || obj.ar || obj.en || '';
}
function locList(obj) {
  if (!obj) return [];
  if (Array.isArray(obj)) return obj;
  return obj[currentLang] || obj.ar || obj.en || [];
}
function levelLabel(level) {
  if (level === 'beginner') return t('levelBeginner');
  if (level === 'intermediate') return t('levelIntermediate');
  if (level === 'advanced') return t('levelAdvanced');
  return level;
}
function escapeHtml(str) {
  return String(str || '')
    .replace(/&/g, '&amp;').replace(/</g, '&lt;')
    .replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}
function deptName(id) {
  const d = DEPARTMENTS.find(x => x.id === id);
  return d ? loc(d.name) : id;
}

/* —— Progress migration —— */
function migrateProgress() {
  try {
    const neu = localStorage.getItem(PROGRESS_KEY);
    if (neu) return;
    const old = localStorage.getItem(PROGRESS_KEY_OLD);
    if (old) {
      localStorage.setItem(PROGRESS_KEY, old);
    }
  } catch { /* ignore */ }
}
function getProgress() {
  migrateProgress();
  try { return JSON.parse(localStorage.getItem(PROGRESS_KEY) || '{}') || {}; }
  catch { return {}; }
}
function saveProgress(data) {
  localStorage.setItem(PROGRESS_KEY, JSON.stringify(data));
  // keep legacy key in sync for older bookmarks
  try { localStorage.setItem(PROGRESS_KEY_OLD, JSON.stringify(data)); } catch { /* */ }
  updateStats();
}
function ensureCourseProgress(courseId) {
  const data = getProgress();
  if (!data[courseId]) {
    data[courseId] = { enrolled: true, completed: [] };
    saveProgress(data);
  } else if (!data[courseId].enrolled) {
    data[courseId].enrolled = true;
    saveProgress(data);
  }
  return data[courseId];
}
function courseProgressPercent(course) {
  const p = getProgress()[course.id];
  if (!p?.enrolled || !course.lessons?.length) return 0;
  return Math.round(((p.completed || []).length / course.lessons.length) * 100);
}
function isEnrolled(courseId) {
  return !!getProgress()[courseId]?.enrolled;
}

async function loadCatalog() {
  if (!authToken) {
    catalogReady = false;
    renderGuestMarketing();
    return;
  }
  const fetchJson = async (path) => {
    try {
      const res = await fetch(path, { cache: 'no-cache' });
      if (!res.ok) throw new Error(path);
      return await res.json();
    } catch {
      try {
        const headers = {};
        if (authToken) headers.Authorization = `Bearer ${authToken}`;
        const res = await fetch(`${API_BASE}/catalog/${path.split('/').pop().replace('.json', '')}`, { cache: 'no-cache', headers });
        if (!res.ok) throw new Error('api');
        return await res.json();
      } catch { return null; }
    }
  };
  const [deps, courses, articles, projects, library, faq, events] = await Promise.all([
    fetchJson('/data/departments.json'),
    fetchJson('/data/courses.json'),
    fetchJson('/data/articles.json'),
    fetchJson('/data/projects.json'),
    fetchJson('/data/library.json'),
    fetchJson('/data/faq.json'),
    fetchJson('/data/events.json')
  ]);
  DEPARTMENTS = deps?.departments || [];
  COURSES = courses?.courses || [];
  PATHS = courses?.paths || [];
  ARTICLES = articles?.articles || [];
  NEWS = articles?.news || [];
  PROJECTS = projects?.projects || [];
  PROJECT_CATS = projects?.categories || [];
  LIBRARY = library?.items || [];
  FAQS = faq?.faqs || [];
  SUCCESS = faq?.successStories || [];
  PARTNERS = faq?.partners || [];
  ROADMAP = faq?.roadmap || [];
  EVENTS = events?.events || [];
  QBANKS = events?.qbankTeasers || [];
  ACCREDITATIONS = events?.accreditations || [];
  catalogReady = true;
  contentUnlocked = true;
  renderAllLanding();
  renderDepartmentChips();
  renderCourses();
  renderPaths();
  renderLibrary();
  renderProjectsTab();
  renderMine();
  renderPortfolio();
  updateStats();
  applySectionGates();
}

function setLang(lang) {
  currentLang = lang === 'en' ? 'en' : 'ar';
  localStorage.setItem('wadnooh_lang', currentLang);
  document.documentElement.lang = currentLang;
  document.documentElement.dir = currentLang === 'ar' ? 'rtl' : 'ltr';
  document.getElementById('langAr')?.classList.toggle('active', currentLang === 'ar');
  document.getElementById('langEn')?.classList.toggle('active', currentLang === 'en');
  document.querySelectorAll('[data-i18n]').forEach(el => {
    const key = el.getAttribute('data-i18n');
    if (key && I18N[currentLang][key]) el.textContent = I18N[currentLang][key];
  });
  document.querySelectorAll('[data-i18n-placeholder]').forEach(el => {
    const key = el.getAttribute('data-i18n-placeholder');
    if (key && I18N[currentLang][key]) el.setAttribute('placeholder', I18N[currentLang][key]);
  });
  document.title = currentLang === 'en'
    ? 'AAMN Software & Computer | ودنوح AAMN للبرمجيات والكمبيوتر'
    : 'ودنوح AAMN للبرمجيات والكمبيوتر | AAMN Software & Computer';
  if (catalogReady) {
    renderAllLanding();
    renderDepartmentChips();
    renderCourses();
    renderPaths();
    renderLibrary();
    renderProjectsTab();
    renderMine();
    renderPortfolio();
    renderUniversities();
    renderAiSuggestions();
    loadMembershipPlans();
    applyRoleUi();
    applySectionGates();
  } else {
    renderGuestMarketing();
  }
}

function renderAllLanding() {
  renderLandingDepts();
  renderLandingCourses();
  renderLandingArticles();
  renderLandingProjects();
  renderLandingPaths();
  renderSuccess();
  renderPartners();
  renderNews();
  renderFaq();
  renderQbank();
  renderEvents();
  renderAccreditations();
  renderRoadmapFooter();
  updateStats();
}

function renderLandingDepts() {
  const box = document.getElementById('landingDepts');
  if (!box) return;
  box.innerHTML = DEPARTMENTS.map(d => `
    <a class="dept-chip" href="/specialty.html?dept=${encodeURIComponent(d.id)}" style="text-decoration:none;color:inherit;display:block">
      <span class="dept-ico" aria-hidden="true">${d.icon || '◆'}</span>
      <strong>${escapeHtml(loc(d.name))}</strong>
      <span>${escapeHtml(loc(d.desc))}</span>
    </a>`).join('');
}

function renderQbank() {
  const box = document.getElementById('landingQbank');
  if (!box) return;
  box.innerHTML = QBANKS.map(q => `
    <article class="teaser is-soon" title="${escapeHtml(t('comingSoonHint'))}">
      <h4>${escapeHtml(loc(q.title))} <span class="soon-badge">${t('soonLabel')}</span></h4>
      <p>${escapeHtml(loc(q.summary))}</p>
      <span class="meta-tag">${escapeHtml(deptName(q.department))}</span>
      <span class="meta-tag">${q.count || 0} Q</span>
    </article>`).join('');
}

function renderEvents() {
  const box = document.getElementById('landingEvents');
  if (!box) return;
  box.innerHTML = EVENTS.map(e => `
    <div class="event-row ${e.status === 'soon' ? 'is-soon' : ''}">
      <div class="date">${escapeHtml(e.date || '')}</div>
      <div>
        <h4 style="font-family:var(--font-display);color:var(--nile-deep);margin-bottom:4px">${escapeHtml(loc(e.title))}</h4>
        <p style="color:var(--muted);font-size:.9rem">${escapeHtml(loc(e.summary))}</p>
        <span class="meta-tag">${escapeHtml(e.type || '')}</span>
        ${e.status === 'soon' ? `<span class="soon-badge">${t('soonLabel')}</span>` : ''}
      </div>
    </div>`).join('');
}

function renderAccreditations() {
  const box = document.getElementById('landingAccreditations');
  if (!box) return;
  box.innerHTML = ACCREDITATIONS.map(a => `
    <article class="teaser">
      <h4>${escapeHtml(loc(a.name))}</h4>
      <p>${escapeHtml(loc(a.note))}</p>
    </article>`).join('');
}

function renderLandingCourses() {
  const box = document.getElementById('landingCourses');
  if (!box) return;
  const list = COURSES.slice(0, 6);
  box.innerHTML = list.map(c => courseCardHtml(c)).join('');
}

function renderLandingArticles() {
  const box = document.getElementById('landingArticles');
  if (!box) return;
  box.innerHTML = ARTICLES.slice(0, 8).map(a => `
    <article class="teaser">
      <time>${escapeHtml(a.date || '')}</time>
      <h4>${escapeHtml(loc(a.title))}</h4>
      <p>${escapeHtml(loc(a.summary))}</p>
      <span class="meta-tag">${escapeHtml(deptName(a.department))}</span>
    </article>`).join('');
}

function renderLandingProjects() {
  const box = document.getElementById('landingProjects');
  if (!box) return;
  box.innerHTML = PROJECTS.slice(0, 6).map(p => projectCardHtml(p)).join('');
}

function renderLandingPaths() {
  const box = document.getElementById('landingPaths');
  if (!box) return;
  box.innerHTML = PATHS.map(path => {
    const names = path.courseIds.map(id => COURSES.find(c => c.id === id)).filter(Boolean)
      .map(c => `<li>${escapeHtml(loc(c.title))}</li>`).join('');
    return `<div class="path-card">
      <h4>${escapeHtml(loc(path.title))}</h4>
      <p>${escapeHtml(loc(path.desc))}</p>
      <ul>${names}</ul>
      <button class="btn btn-secondary btn-sm" type="button" onclick="openPath('${path.id}')">${t('viewPath')}</button>
    </div>`;
  }).join('');
}

function renderSuccess() {
  const box = document.getElementById('landingSuccess');
  if (!box) return;
  box.innerHTML = SUCCESS.map(s => `
    <blockquote class="story">
      <p>«${escapeHtml(loc(s.quote))}»</p>
      <footer><strong>${escapeHtml(loc(s.name))}</strong> — ${escapeHtml(loc(s.role))}</footer>
    </blockquote>`).join('');
}

function renderPartners() {
  const box = document.getElementById('landingPartners');
  if (!box) return;
  box.innerHTML = PARTNERS.map(p =>
    `<a class="partner-mark" href="${escapeHtml(p.url)}" target="_blank" rel="noopener">${escapeHtml(p.name)}</a>`
  ).join('');
}

function renderNews() {
  const box = document.getElementById('landingNews');
  if (!box) return;
  box.innerHTML = NEWS.map(n => `
    <article class="teaser">
      <time>${escapeHtml(n.date || '')}</time>
      <h4>${escapeHtml(loc(n.title))}</h4>
      <p>${escapeHtml(loc(n.summary))}</p>
    </article>`).join('');
}

function renderFaq() {
  const box = document.getElementById('landingFaq');
  if (!box) return;
  box.innerHTML = FAQS.map(f => `
    <details class="faq-item">
      <summary>${escapeHtml(loc(f.q))}</summary>
      <p>${escapeHtml(loc(f.a))}</p>
    </details>`).join('');
}

function renderRoadmapFooter() {
  const box = document.getElementById('roadmapStrip');
  if (!box) return;
  box.innerHTML = ROADMAP.map(r => {
    const items = locList(r.items).map(i => `<li>${escapeHtml(i)}</li>`).join('');
    const statusLabel = r.status === 'shipped'
      ? (currentLang === 'en' ? 'Shipped' : 'مُسلَّم')
      : r.status === 'soon'
        ? (currentLang === 'en' ? 'Soon' : 'قريباً')
        : (currentLang === 'en' ? 'Planned' : 'مخطط');
    const title = r.label ? loc(r.label) : `Phase ${r.version}`;
    return `<div class="roadmap-col">
      <h4>${escapeHtml(title)} <span class="meta-tag">${statusLabel}</span></h4>
      <ul>${items}</ul>
    </div>`;
  }).join('');
}

function projectCardHtml(p) {
  const cat = PROJECT_CATS.find(c => c.id === p.category);
  return `<article class="card project-card">
    <div class="project-thumb" style="background-image:url('${escapeHtml(p.image || '/images/wad-nooh-icon.png')}')"></div>
    <h4>${escapeHtml(loc(p.title))}</h4>
    <p>${escapeHtml(loc(p.description))}</p>
    <div class="meta-row">
      <span class="meta-tag">${escapeHtml(cat ? loc(cat.name) : p.category)}</span>
      <span class="meta-tag">${escapeHtml(deptName(p.department))}</span>
    </div>
    <div class="card-actions">
      <button class="btn btn-secondary btn-sm" type="button" onclick="openProject('${p.id}')">${t('openLink')}</button>
      ${p.codeUrl ? `<a class="btn btn-outline btn-sm" href="${escapeHtml(p.codeUrl)}" target="_blank" rel="noopener">${t('codeLink')}</a>` : ''}
      ${p.reportUrl ? `<a class="btn btn-outline btn-sm" href="${escapeHtml(p.reportUrl)}" target="_blank" rel="noopener">${t('reportLink')}</a>` : ''}
    </div>
  </article>`;
}

function openProject(id) {
  const p = PROJECTS.find(x => x.id === id);
  if (!p) return;
  const bom = (p.bom || []).map(b => `<li>${escapeHtml(loc(b.item))} × ${b.qty || 1}</li>`).join('');
  const steps = locList(p.steps).map(s => `<li>${escapeHtml(s)}</li>`).join('');
  const faq = locList(p.faq).map(f => `<details class="faq-item"><summary>${escapeHtml(f.q)}</summary><p>${escapeHtml(f.a)}</p></details>`).join('');
  document.getElementById('projectModalTitle').textContent = loc(p.title);
  document.getElementById('projectModalBody').innerHTML = `
    <p>${escapeHtml(loc(p.description))}</p>
    <div class="meta-row"><span class="meta-tag">${escapeHtml(deptName(p.department))}</span></div>
    ${bom ? `<div class="academic-block"><h4>${t('bomLabel')}</h4><ul>${bom}</ul></div>` : ''}
    ${steps ? `<div class="academic-block"><h4>${t('stepsLabel')}</h4><ol>${steps}</ol></div>` : ''}
    ${faq ? `<div class="academic-block">${faq}</div>` : ''}
    <div class="card-actions" style="margin-top:12px">
      ${p.codeUrl ? `<a class="btn btn-outline btn-sm" href="${escapeHtml(p.codeUrl)}" target="_blank" rel="noopener">${t('codeLink')}</a>` : ''}
      ${p.reportUrl ? `<a class="btn btn-outline btn-sm" href="${escapeHtml(p.reportUrl)}" target="_blank" rel="noopener">${t('reportLink')}</a>` : ''}
      ${p.videoUrl ? `<a class="btn btn-outline btn-sm" href="${escapeHtml(p.videoUrl)}" target="_blank" rel="noopener">${t('videos')}</a>` : ''}
      ${p.circuitUrl ? `<a class="btn btn-outline btn-sm" href="${escapeHtml(p.circuitUrl)}" target="_blank" rel="noopener">Circuit</a>` : ''}
    </div>`;
  openModal('projectModal');
}

function courseCardHtml(course) {
  const pct = courseProgressPercent(course);
  const enrolled = isEnrolled(course.id);
  const media = course.media || {};
  const mediaBits = [
    media.video ? 'Video' : '',
    media.pdf ? 'PDF' : '',
    media.ppt ? 'PPT' : '',
    media.code ? 'Code' : ''
  ].filter(Boolean).join(' · ');
  return `
    <div class="card">
      <h4>${escapeHtml(loc(course.title))}</h4>
      <p>${escapeHtml(loc(course.description || course.desc))}</p>
      <div class="meta-row">
        <span class="meta-tag">${escapeHtml(deptName(course.department || course.category))}</span>
        <span class="meta-tag">${levelLabel(course.level)}</span>
        <span class="meta-tag">${course.hours || ''} ${t('hours')}</span>
        <span class="meta-tag">${(course.lessons || []).length} ${t('lessons')}</span>
        ${course.certificate ? `<span class="meta-tag">${t('hasCert')}</span>` : ''}
        ${course.project ? `<span class="meta-tag">${t('hasProject')}</span>` : ''}
        ${enrolled ? `<span class="meta-tag">${t('enrolled')}</span>` : ''}
      </div>
      ${mediaBits ? `<div class="media-flags">${t('media')}: ${mediaBits}</div>` : ''}
      ${enrolled ? `
        <div class="progress-bar" aria-hidden="true"><span style="width:${pct}%"></span></div>
        <div class="progress-label">${t('progress')}: ${pct}%</div>` : ''}
      <div class="card-actions">
        <button class="btn btn-outline btn-sm" type="button" onclick="openCourse('${course.id}')">${t('openCourse')}</button>
        ${enrolled
          ? `<button class="btn btn-secondary btn-sm" type="button" onclick="openCourse('${course.id}')">${t('continueLearning')}</button>`
          : `<button class="btn btn-secondary btn-sm" type="button" onclick="enrollCourse('${course.id}')">${t('enroll')}</button>`}
      </div>
    </div>`;
}

function filterByDept(id) {
  activeDepartment = id;
  openTab('courses');
  renderDepartmentChips();
  renderCourses();
}

function renderDepartmentChips() {
  const box = document.getElementById('categoryChips');
  if (!box) return;
  const chips = [{ id: 'all', name: t('catAll') }].concat(
    DEPARTMENTS.map(d => ({ id: d.id, name: loc(d.name) }))
  );
  box.innerHTML = chips.map(c =>
    `<button type="button" class="${activeDepartment === c.id ? 'active' : ''}" onclick="setDepartment('${c.id}')">${escapeHtml(c.name)}</button>`
  ).join('');
}

function setDepartment(id) {
  activeDepartment = id;
  renderDepartmentChips();
  renderCourses();
}

function filteredCourses() {
  const q = (document.getElementById('courseSearch')?.value || '').trim().toLowerCase();
  const level = document.getElementById('levelFilter')?.value || '';
  return COURSES.filter(c => {
    if (activeDepartment !== 'all' && c.department !== activeDepartment) return false;
    if (level && c.level !== level) return false;
    if (!q) return true;
    const hay = `${loc(c.title)} ${loc(c.description)} ${deptName(c.department)}`.toLowerCase();
    return hay.includes(q);
  });
}

function renderCourses() {
  if (!contentUnlocked) {
    const box = document.getElementById('coursesResults');
    if (box) box.innerHTML = gateCardHtml();
    return;
  }
  const box = document.getElementById('coursesResults');
  if (!box) return;
  const list = filteredCourses();
  box.innerHTML = list.length ? list.map(c => courseCardHtml(c)).join('') : `<div class="empty">${t('noCourses')}</div>`;
}

function renderPaths() {
  if (!contentUnlocked) {
    const box = document.getElementById('pathsResults');
    if (box) box.innerHTML = gateCardHtml();
    return;
  }
  const box = document.getElementById('pathsResults');
  if (!box) return;
  box.innerHTML = PATHS.map(path => {
    const names = path.courseIds.map(id => COURSES.find(c => c.id === id)).filter(Boolean)
      .map(c => `<li>${escapeHtml(loc(c.title))}</li>`).join('');
    return `<div class="path-card">
      <h4>${escapeHtml(loc(path.title))}</h4>
      <p>${escapeHtml(loc(path.desc))}</p>
      <ul>${names}</ul>
      <button class="btn btn-secondary btn-sm" type="button" onclick="openPath('${path.id}')">${t('viewPath')}</button>
    </div>`;
  }).join('');
}

function openPath(pathId) {
  const path = PATHS.find(p => p.id === pathId);
  if (!path) return;
  openTab('courses');
  activeDepartment = 'all';
  const search = document.getElementById('courseSearch');
  if (search) search.value = '';
  const level = document.getElementById('levelFilter');
  if (level) level.value = '';
  renderDepartmentChips();
  const box = document.getElementById('coursesResults');
  const list = path.courseIds.map(id => COURSES.find(c => c.id === id)).filter(Boolean);
  if (box) box.innerHTML = list.map(c => courseCardHtml(c)).join('');
  document.getElementById('learn')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

function renderLibrary() {
  if (!contentUnlocked) {
    const box = document.getElementById('libraryResults');
    if (box) box.innerHTML = gateCardHtml();
    return;
  }
  const box = document.getElementById('libraryResults');
  if (!box) return;
  const q = (document.getElementById('librarySearch')?.value || '').trim().toLowerCase();
  const list = LIBRARY.filter(item => {
    if (!q) return true;
    const hay = `${loc(item.title)} ${item.authors || ''} ${item.org || ''} ${(item.topics || []).join(' ')}`.toLowerCase();
    return hay.includes(q);
  });
  box.innerHTML = list.map(item => `
    <article class="uni-row">
      <div>
        <h4>${escapeHtml(loc(item.title))}</h4>
        <p class="uni-focus">${escapeHtml(item.authors || '')}${item.year ? ` · ${item.year}` : ''}${item.org ? ` · ${item.org}` : ''}</p>
        <p style="color:var(--muted);font-size:0.9rem;">${escapeHtml(loc(item.note))}</p>
        <div class="meta-row">
          <span class="meta-tag">${escapeHtml(item.type || '')}</span>
          ${(item.lang || []).map(l => `<span class="meta-tag">${l.toUpperCase()}</span>`).join('')}
          ${(item.topics || []).slice(0, 3).map(tp => `<span class="meta-tag">${escapeHtml(tp)}</span>`).join('')}
        </div>
      </div>
      <div class="uni-actions">
        <a class="btn btn-secondary btn-sm" href="${escapeHtml(item.url)}" target="_blank" rel="noopener">${t('openLink')}</a>
      </div>
    </article>`).join('') || `<div class="empty">${t('noCourses')}</div>`;
}

function renderProjectsTab() {
  if (!contentUnlocked) {
    const box = document.getElementById('projectsResults');
    if (box) box.innerHTML = gateCardHtml();
    return;
  }
  const chips = document.getElementById('projectChips');
  if (chips) {
    const all = [{ id: 'all', name: t('catAll') }].concat(PROJECT_CATS.map(c => ({ id: c.id, name: loc(c.name) })));
    chips.innerHTML = all.map(c =>
      `<button type="button" class="${projectCatFilter === c.id ? 'active' : ''}" onclick="setProjectCat('${c.id}')">${escapeHtml(c.name)}</button>`
    ).join('');
  }
  const box = document.getElementById('projectsResults');
  if (!box) return;
  const list = PROJECTS.filter(p => projectCatFilter === 'all' || p.category === projectCatFilter);
  box.innerHTML = list.map(p => projectCardHtml(p)).join('') || `<div class="empty">${t('noCourses')}</div>`;
}

function setProjectCat(id) {
  projectCatFilter = id;
  renderProjectsTab();
}

function renderMine() {
  if (!contentUnlocked) {
    const box = document.getElementById('mineResults');
    if (box) box.innerHTML = gateCardHtml();
    const analytics = document.getElementById('mineAnalytics');
    if (analytics) analytics.innerHTML = '';
    return;
  }
  document.querySelectorAll('[data-mine-tab]').forEach(btn => {
    btn.classList.toggle('active', btn.getAttribute('data-mine-tab') === mineSubTab);
  });
  if (mineSubTab === 'lectures') {
    const analytics = document.getElementById('mineAnalytics');
    if (analytics) {
      analytics.innerHTML = `
        <div class="w"><strong>${memberLectures.length}</strong><span>${t('lecCount')}</span></div>
        <div class="w"><strong>${lecturesApiOk ? 'API' : 'Local'}</strong><span>${t('lecTitle')}</span></div>`;
    }
    renderMyLecturesPanel();
    return;
  }
  renderMineAnalytics();
  const box = document.getElementById('mineResults');
  if (!box) return;
  const progress = getProgress();
  const enrolled = COURSES.filter(c => progress[c.id]?.enrolled);
  const certs = enrolled.filter(c => courseProgressPercent(c) === 100 && c.certificate);
  const notes = localStorage.getItem(NOTES_KEY) || '';
  let saved = [];
  try { saved = JSON.parse(localStorage.getItem(SAVED_KEY) || '[]'); } catch { saved = []; }
  let cal = [];
  try { cal = JSON.parse(localStorage.getItem(CAL_KEY) || '[]'); } catch { cal = []; }

  const coursesHtml = enrolled.length
    ? enrolled.map(c => {
        const p = progress[c.id];
        const done = (p.completed || []).length;
        const pct = courseProgressPercent(c);
        const lessons = (c.lessons || []).map(l => {
          const isDone = (p.completed || []).includes(l.id);
          return `<li class="${isDone ? 'done' : ''}">
            <span>${isDone ? '✓ ' : ''}${escapeHtml(loc(l.title))}</span>
            <span style="display:flex;gap:6px;flex-wrap:wrap">
              <button type="button" onclick="quickSaveCourseLesson('${c.id}','${l.id}')">${t('lecSaveFromCourse')}</button>
              <button type="button" onclick="openLesson('${c.id}','${l.id}')">${t('continueLearning')}</button>
            </span>
          </li>`;
        }).join('');
        return `<div class="card">
          <h4>${escapeHtml(loc(c.title))}</h4>
          <div class="progress-bar"><span style="width:${pct}%"></span></div>
          <div class="progress-label">${t('progress')}: ${pct}% · ${done}/${(c.lessons || []).length} ${t('lessons')}${pct === 100 ? ` · ${t('completed')}` : ''}</div>
          <ul class="lesson-list">${lessons}</ul>
        </div>`;
      }).join('')
    : `<div class="empty">${t('noEnrolled')}</div>`;

  const certsHtml = certs.length
    ? certs.map(c => `<div class="cert-card"><strong>${escapeHtml(loc(c.title))}</strong><span>${t('hasCert')} · MVP</span></div>`).join('')
    : `<div class="empty">${t('certsEmpty')}</div>`;

  const savedHtml = `
    <div class="dash-block">
      <h4>${t('savedTitle')}</h4>
      <p style="color:var(--muted);font-size:0.9rem;">${t('savedHint')}</p>
      <div class="form-row">
        <input id="savedUrlInput" type="url" placeholder="https://…">
        <button class="btn btn-secondary btn-sm" type="button" onclick="addSavedFile()">${t('savedAdd')}</button>
      </div>
      <ul class="simple-list">${saved.map((s, i) =>
        `<li><a href="${escapeHtml(s)}" target="_blank" rel="noopener">${escapeHtml(s)}</a>
         <button type="button" class="btn btn-ghost btn-sm" onclick="removeSavedFile(${i})">×</button></li>`
      ).join('')}</ul>
    </div>`;

  const notesHtml = `
    <div class="dash-block">
      <h4>${t('notesTitle')}</h4>
      <textarea id="notesArea" rows="4">${escapeHtml(notes)}</textarea>
      <button class="btn btn-secondary btn-sm" type="button" style="margin-top:8px" onclick="saveNotes()">${t('notesSave')}</button>
    </div>`;

  const calHtml = `
    <div class="dash-block">
      <h4>${t('calTitle')}</h4>
      <div class="form-row">
        <input id="calDate" type="date">
        <input id="calText" type="text" placeholder="…">
        <button class="btn btn-secondary btn-sm" type="button" onclick="addCalEvent()">${t('calAdd')}</button>
      </div>
      <ul class="simple-list">${cal.map((e, i) =>
        `<li><strong>${escapeHtml(e.date)}</strong> — ${escapeHtml(e.text)}
         <button type="button" class="btn btn-ghost btn-sm" onclick="removeCalEvent(${i})">×</button></li>`
      ).join('')}</ul>
    </div>`;

  const lecturesTeaser = `
    <div class="dash-block">
      <h4>${t('lecTitle')}</h4>
      <p style="color:var(--muted);font-size:0.9rem;">${t('lecHint')}</p>
      <p style="margin:8px 0;font-family:var(--font-display);color:var(--kush);">${memberLectures.length} ${t('lecCount')}</p>
      <button class="btn btn-secondary btn-sm" type="button" onclick="setMineSubTab('lectures')">${t('mineTabLectures')}</button>
    </div>`;

  box.innerHTML = `
    <div class="dash-grid">
      <div class="dash-main">${coursesHtml}</div>
      <aside class="dash-side">
        ${lecturesTeaser}
        <div class="dash-block"><h4>${t('certsTitle')}</h4>${certsHtml}</div>
        ${savedHtml}${notesHtml}${calHtml}
      </aside>
    </div>`;
}

function setMineSubTab(tab) {
  mineSubTab = tab === 'lectures' ? 'lectures' : 'progress';
  if (mineSubTab === 'lectures') loadMemberLectures().finally(() => renderMine());
  else renderMine();
}

function lecturesLocalKey() {
  const uid = currentUser?.id || 'anon';
  return LECTURES_LS_PREFIX + uid;
}

function readLocalLectures() {
  try {
    const raw = JSON.parse(localStorage.getItem(lecturesLocalKey()) || '[]');
    return Array.isArray(raw) ? raw : [];
  } catch {
    return [];
  }
}

function writeLocalLectures(list) {
  localStorage.setItem(lecturesLocalKey(), JSON.stringify(list));
}

function lectureDisplayTitle(lec) {
  if (currentLang === 'en' && lec.titleEn) return lec.titleEn;
  return lec.titleAr || lec.titleEn || '—';
}

function parseLectureDateInput(iso) {
  if (!iso) return '';
  try {
    return String(iso).slice(0, 10);
  } catch {
    return '';
  }
}

async function loadMemberLectures() {
  if (!authToken || !contentUnlocked) {
    memberLectures = [];
    lecturesLoaded = true;
    return;
  }
  try {
    const q = lectureSearchQ ? `?q=${encodeURIComponent(lectureSearchQ)}` : '';
    const tag = lectureTagFilter
      ? `${q ? '&' : '?'}tag=${encodeURIComponent(lectureTagFilter)}`
      : '';
    const data = await api(`/me/lectures${q}${tag}`);
    memberLectures = Array.isArray(data) ? data : [];
    lecturesApiOk = true;
    writeLocalLectures(memberLectures);
  } catch {
    lecturesApiOk = false;
    let local = readLocalLectures();
    const q = (lectureSearchQ || '').trim().toLowerCase();
    const tag = (lectureTagFilter || '').trim().toLowerCase();
    if (q) {
      local = local.filter(l =>
        [l.titleAr, l.titleEn, l.subject, l.notes, ...(l.tags || [])]
          .filter(Boolean)
          .join(' ')
          .toLowerCase()
          .includes(q)
      );
    }
    if (tag) {
      local = local.filter(l => (l.tags || []).some(x => String(x).toLowerCase().includes(tag)));
    }
    memberLectures = local;
  }
  lecturesLoaded = true;
}

function renderMyLecturesPanel() {
  const box = document.getElementById('mineResults');
  if (!box) return;
  if (!lecturesLoaded) {
    box.innerHTML = `<div class="empty">…</div>`;
    loadMemberLectures().then(() => renderMine());
    return;
  }

  const listHtml = memberLectures.length
    ? memberLectures.map(lec => {
        const tags = (lec.tags || []).map(tg => `<span class="meta-tag">${escapeHtml(tg)}</span>`).join('');
        const attach = (lec.attachments || [])[0];
        const attachHtml = attach?.url
          ? `<a class="btn btn-outline btn-sm" href="${escapeHtml(attach.url)}" target="_blank" rel="noopener">${t('lecOpenAttach')}</a>`
          : '';
        const metaBits = [
          lec.subject,
          lec.lectureDate ? parseLectureDateInput(lec.lectureDate) : '',
          lec.durationMinutes ? `${lec.durationMinutes} ${t('duration')}` : ''
        ].filter(Boolean).join(' · ');
        return `<article class="lecture-card" data-lec-id="${lec.id}">
          <h4>${escapeHtml(lectureDisplayTitle(lec))}</h4>
          <div class="lec-meta">${escapeHtml(metaBits || '—')}</div>
          ${lec.notes ? `<div class="lec-notes">${escapeHtml(lec.notes)}</div>` : ''}
          <div class="meta-row" style="margin-bottom:10px">${tags}</div>
          <div class="lec-actions">
            ${attachHtml}
            <button type="button" class="btn btn-outline btn-sm" onclick="openLectureEditor(${Number(lec.id)})">${t('lecEdit')}</button>
            <button type="button" class="btn btn-ghost btn-sm" onclick="deleteMemberLecture(${Number(lec.id)})">${t('lecDelete')}</button>
          </div>
        </article>`;
      }).join('')
    : `<div class="lecture-empty">
        <strong>${t('lecEmptyTitle')}</strong>
        <p style="color:var(--muted);margin-bottom:14px">${t('lecEmptyBody')}</p>
        <button type="button" class="btn btn-secondary" onclick="openLectureEditor()">${t('lecAdd')}</button>
      </div>`;

  box.innerHTML = `
    <div class="dash-block">
      <h4>${t('lecTitle')}</h4>
      <p style="color:var(--muted);font-size:0.9rem;margin-bottom:12px;">${t('lecHint')}</p>
      <div class="lecture-toolbar">
        <input id="lecSearchInput" type="search" value="${escapeHtml(lectureSearchQ)}" placeholder="${escapeHtml(t('lecSearchPh'))}" onkeydown="if(event.key==='Enter')filterMemberLectures()">
        <input id="lecTagFilterInput" type="text" value="${escapeHtml(lectureTagFilter)}" placeholder="${escapeHtml(t('lecTagPh'))}" onkeydown="if(event.key==='Enter')filterMemberLectures()">
        <button class="btn btn-outline btn-sm" type="button" onclick="filterMemberLectures()">${t('searchLabel')}</button>
        <button class="btn btn-secondary btn-sm" type="button" onclick="openLectureEditor()">${t('lecAdd')}</button>
      </div>
      ${listHtml}
    </div>`;
}

function filterMemberLectures() {
  lectureSearchQ = document.getElementById('lecSearchInput')?.value?.trim() || '';
  lectureTagFilter = document.getElementById('lecTagFilterInput')?.value?.trim() || '';
  loadMemberLectures().then(() => renderMine());
}

function openLectureEditor(id) {
  if (!contentUnlocked || !authToken) {
    openModal('loginModal');
    return;
  }
  const lec = id != null ? memberLectures.find(x => Number(x.id) === Number(id)) : null;
  document.getElementById('lecEditId').value = lec ? String(lec.id) : '';
  document.getElementById('lectureModalTitle').textContent = lec ? t('lecFormEdit') : t('lecFormAdd');
  document.getElementById('lecTitleAr').value = lec?.titleAr || '';
  document.getElementById('lecTitleEn').value = lec?.titleEn || '';
  document.getElementById('lecSubject').value = lec?.subject || '';
  document.getElementById('lecDuration').value = lec?.durationMinutes || '';
  document.getElementById('lecDate').value = parseLectureDateInput(lec?.lectureDate);
  document.getElementById('lecTags').value = (lec?.tags || []).join(', ');
  document.getElementById('lecNotes').value = lec?.notes || '';
  const att = (lec?.attachments || [])[0];
  document.getElementById('lecAttachUrl').value = att?.url || '';
  document.getElementById('lecAttachName').value = att?.filename || '';
  document.getElementById('lecCourseId').value = lec?.courseId || '';
  document.getElementById('lecLessonId').value = lec?.lessonId || '';
  document.getElementById('lecSpecialtyId').value = lec?.specialtyId || '';
  setMsg('lectureMsg', '', '');
  openModal('lectureModal');
}

function collectLecturePayload() {
  const titleAr = (document.getElementById('lecTitleAr')?.value || '').trim();
  const titleEn = (document.getElementById('lecTitleEn')?.value || '').trim();
  const subject = (document.getElementById('lecSubject')?.value || '').trim();
  const durationRaw = document.getElementById('lecDuration')?.value;
  const durationMinutes = durationRaw ? Number(durationRaw) : null;
  const dateVal = document.getElementById('lecDate')?.value || '';
  const tags = (document.getElementById('lecTags')?.value || '')
    .split(/[,،]/)
    .map(s => s.trim())
    .filter(Boolean);
  const notes = (document.getElementById('lecNotes')?.value || '').trim();
  const attachUrl = (document.getElementById('lecAttachUrl')?.value || '').trim();
  const attachName = (document.getElementById('lecAttachName')?.value || '').trim();
  const attachments = [];
  if (attachUrl || attachName) {
    attachments.push({
      url: attachUrl || null,
      filename: attachName || null,
      type: attachUrl.includes('youtu') || attachUrl.includes('.mp4') ? 'video' : 'file'
    });
  }
  return {
    titleAr: titleAr || titleEn,
    titleEn: titleEn || null,
    subject: subject || null,
    specialtyId: (document.getElementById('lecSpecialtyId')?.value || '').trim() || null,
    courseId: (document.getElementById('lecCourseId')?.value || '').trim() || null,
    lessonId: (document.getElementById('lecLessonId')?.value || '').trim() || null,
    notes: notes || null,
    lectureDate: dateVal ? new Date(dateVal + 'T12:00:00Z').toISOString() : null,
    durationMinutes: Number.isFinite(durationMinutes) && durationMinutes > 0 ? durationMinutes : null,
    attachments,
    tags
  };
}

async function submitLectureForm() {
  const payload = collectLecturePayload();
  if (!payload.titleAr) {
    setMsg('lectureMsg', currentLang === 'en' ? 'Title is required' : 'العنوان مطلوب', 'error');
    return;
  }
  const editId = (document.getElementById('lecEditId')?.value || '').trim();
  try {
    if (editId) await api(`/me/lectures/${editId}`, { method: 'PUT', body: JSON.stringify(payload) });
    else await api('/me/lectures', { method: 'POST', body: JSON.stringify(payload) });
    lecturesApiOk = true;
    setMsg('lectureMsg', t('lecSavedOk'), 'success');
    closeModal('lectureModal');
    mineSubTab = 'lectures';
    await loadMemberLectures();
    renderMine();
  } catch (err) {
    // Local fallback when API is down
    const local = readLocalLectures();
    const now = new Date().toISOString();
    if (editId) {
      const idx = local.findIndex(x => String(x.id) === String(editId));
      if (idx >= 0) {
        local[idx] = { ...local[idx], ...payload, id: local[idx].id, updatedAtUtc: now };
      }
    } else {
      local.unshift({
        ...payload,
        id: `local-${Date.now()}`,
        createdAtUtc: now,
        updatedAtUtc: now
      });
    }
    writeLocalLectures(local);
    lecturesApiOk = false;
    memberLectures = local;
    setMsg('lectureMsg', t('lecLocalFallback'), 'success');
    closeModal('lectureModal');
    mineSubTab = 'lectures';
    renderMine();
  }
}

async function deleteMemberLecture(id) {
  if (!confirm(t('lecConfirmDelete'))) return;
  try {
    await api(`/me/lectures/${id}`, { method: 'DELETE' });
    lecturesApiOk = true;
  } catch {
    lecturesApiOk = false;
    writeLocalLectures(readLocalLectures().filter(x => String(x.id) !== String(id)));
  }
  await loadMemberLectures();
  renderMine();
}

function saveLessonToMyLectures() {
  if (!activeLesson) return;
  quickSaveCourseLesson(activeLesson.courseId, activeLesson.lessonId);
}

function quickSaveCourseLesson(courseId, lessonId) {
  if (!contentUnlocked || !authToken) {
    openModal('loginModal');
    return;
  }
  const course = COURSES.find(c => c.id === courseId);
  const lesson = course?.lessons?.find(l => l.id === lessonId);
  if (!course || !lesson) return;
  const titleObj = lesson.title || {};
  document.getElementById('lecEditId').value = '';
  document.getElementById('lectureModalTitle').textContent = t('lecFormAdd');
  document.getElementById('lecTitleAr').value = titleObj.ar || loc(lesson.title);
  document.getElementById('lecTitleEn').value = titleObj.en || '';
  document.getElementById('lecSubject').value = loc(course.title);
  document.getElementById('lecDuration').value = '';
  document.getElementById('lecDate').value = new Date().toISOString().slice(0, 10);
  document.getElementById('lecTags').value = deptName(course.department || course.category) || '';
  document.getElementById('lecNotes').value = loc(lesson.body) || '';
  document.getElementById('lecAttachUrl').value = '';
  document.getElementById('lecAttachName').value = '';
  document.getElementById('lecCourseId').value = course.id;
  document.getElementById('lecLessonId').value = lesson.id;
  document.getElementById('lecSpecialtyId').value = course.department || course.category || '';
  setMsg('lectureMsg', '', '');
  closeModal('lessonModal');
  openModal('lectureModal');
}

function renderPortfolio() {
  if (!contentUnlocked) {
    const box = document.getElementById('portfolioResults');
    if (box) box.innerHTML = gateCardHtml();
    return;
  }
  const box = document.getElementById('portfolioResults');
  if (!box) return;
  const progress = getProgress();
  const enrolled = COURSES.filter(c => progress[c.id]?.enrolled);
  const certs = enrolled.filter(c => courseProgressPercent(c) === 100 && c.certificate);
  const skills = new Set();
  enrolled.forEach(c => { if (c.department) skills.add(deptName(c.department)); });
  PROJECTS.slice(0, 4).forEach(p => (p.skills || []).forEach(s => skills.add(s)));
  box.innerHTML = `
    <div class="dash-block">
      <h4>${t('skillsLabel')}</h4>
      <div class="meta-row">${[...skills].map(s => `<span class="meta-tag">${escapeHtml(s)}</span>`).join('') || '—'}</div>
    </div>
    <div class="dash-block">
      <h4>${t('certsTitle')}</h4>
      ${certs.length ? certs.map(c => `<div class="cert-card"><strong>${escapeHtml(loc(c.title))}</strong><span>AAMN · Phase 1</span></div>`).join('') : `<div class="empty">${t('certsEmpty')}</div>`}
    </div>
    <div class="dash-block">
      <h4>${t('secProjects')}</h4>
      <div class="grid">${PROJECTS.slice(0, 3).map(p => projectCardHtml(p)).join('')}</div>
    </div>`;
}

function renderMineAnalytics() {
  const box = document.getElementById('mineAnalytics');
  if (!box) return;
  const progress = getProgress();
  const enrolled = COURSES.filter(c => progress[c.id]?.enrolled);
  const avg = enrolled.length
    ? Math.round(enrolled.reduce((s, c) => s + courseProgressPercent(c), 0) / enrolled.length)
    : 0;
  const certs = enrolled.filter(c => courseProgressPercent(c) === 100 && c.certificate).length;
  box.innerHTML = `
    <div class="w"><strong>${enrolled.length}</strong><span>${t('analyticsCourses')}</span></div>
    <div class="w"><strong>${avg}%</strong><span>${t('analyticsProgress')}</span></div>
    <div class="w"><strong>${certs}</strong><span>${t('analyticsCerts')}</span></div>
    <div class="w"><strong>${DEPARTMENTS.length}</strong><span>${t('statDepts')}</span></div>`;
}

function addSavedFile() {
  const input = document.getElementById('savedUrlInput');
  const url = (input?.value || '').trim();
  if (!url) return;
  let saved = [];
  try { saved = JSON.parse(localStorage.getItem(SAVED_KEY) || '[]'); } catch { saved = []; }
  saved.push(url);
  localStorage.setItem(SAVED_KEY, JSON.stringify(saved));
  renderMine();
}
function removeSavedFile(i) {
  let saved = [];
  try { saved = JSON.parse(localStorage.getItem(SAVED_KEY) || '[]'); } catch { saved = []; }
  saved.splice(i, 1);
  localStorage.setItem(SAVED_KEY, JSON.stringify(saved));
  renderMine();
}
function saveNotes() {
  const v = document.getElementById('notesArea')?.value || '';
  localStorage.setItem(NOTES_KEY, v);
}
function addCalEvent() {
  const date = document.getElementById('calDate')?.value;
  const text = (document.getElementById('calText')?.value || '').trim();
  if (!date || !text) return;
  let cal = [];
  try { cal = JSON.parse(localStorage.getItem(CAL_KEY) || '[]'); } catch { cal = []; }
  cal.push({ date, text });
  localStorage.setItem(CAL_KEY, JSON.stringify(cal));
  renderMine();
}
function removeCalEvent(i) {
  let cal = [];
  try { cal = JSON.parse(localStorage.getItem(CAL_KEY) || '[]'); } catch { cal = []; }
  cal.splice(i, 1);
  localStorage.setItem(CAL_KEY, JSON.stringify(cal));
  renderMine();
}

function updateStats() {
  const set = (id, v) => { const el = document.getElementById(id); if (el) el.textContent = String(v); };
  if (!contentUnlocked) {
    set('statCourses', '—');
    set('statDepts', String(GUEST_DEPT_TEASERS.length) + '+');
    set('statProjects', '—');
    set('statUnis', '—');
    set('statEnrolled', '—');
    set('heroStatCourses', '—');
    set('heroStatDepts', String(GUEST_DEPT_TEASERS.length) + '+');
    return;
  }
  set('statCourses', COURSES.length || '—');
  set('statDepts', DEPARTMENTS.length || '—');
  set('statProjects', PROJECTS.length || '—');
  set('statUnis', universitiesData.length || '—');
  set('statEnrolled', Object.values(getProgress()).filter(p => p.enrolled).length);
  set('heroStatCourses', COURSES.length || '—');
  set('heroStatDepts', DEPARTMENTS.length || '—');
}

/* —— Unified smart search —— */
function runSmartSearch() {
  const q = (document.getElementById('smartSearch')?.value || '').trim().toLowerCase();
  const box = document.getElementById('smartSearchResults');
  if (!box) return;
  if (!contentUnlocked) {
    box.hidden = false;
    box.innerHTML = gateCardHtml();
    return;
  }
  if (!q) { box.innerHTML = ''; box.hidden = true; return; }
  const hits = [];
  DEPARTMENTS.forEach(d => {
    if (`${loc(d.name)} ${loc(d.desc)}`.toLowerCase().includes(q))
      hits.push({ type: currentLang === 'en' ? 'Department' : 'قسم', title: loc(d.name), action: `filterByDept('${d.id}')` });
  });
  COURSES.forEach(c => {
    if (`${loc(c.title)} ${loc(c.description)}`.toLowerCase().includes(q))
      hits.push({ type: currentLang === 'en' ? 'Course' : 'دورة', title: loc(c.title), action: `openCourse('${c.id}')` });
  });
  LIBRARY.forEach(item => {
    if (`${loc(item.title)} ${item.authors || ''}`.toLowerCase().includes(q))
      hits.push({ type: currentLang === 'en' ? 'Library' : 'مكتبة', title: loc(item.title), action: `window.open(${JSON.stringify(item.url)},'_blank')` });
  });
  PROJECTS.forEach(p => {
    if (`${loc(p.title)} ${loc(p.description)}`.toLowerCase().includes(q))
      hits.push({ type: currentLang === 'en' ? 'Project' : 'مشروع', title: loc(p.title), action: `openTab('projects')` });
  });
  QBANKS.forEach(item => {
    if (`${loc(item.title)} ${loc(item.summary)}`.toLowerCase().includes(q))
      hits.push({ type: currentLang === 'en' ? 'Q-Bank' : 'بنك أسئلة', title: loc(item.title), action: `document.getElementById('qbank-sec')?.scrollIntoView({behavior:'smooth'})` });
  });
  ARTICLES.forEach(a => {
    if (`${loc(a.title)} ${loc(a.summary)}`.toLowerCase().includes(q))
      hits.push({ type: currentLang === 'en' ? 'Article' : 'مقالة', title: loc(a.title), action: `document.getElementById('articles-sec')?.scrollIntoView({behavior:'smooth'})` });
  });
  universitiesData.forEach(u => {
    const hay = `${u.nameAr} ${u.nameEn} ${u.countryAr} ${u.countryEn}`.toLowerCase();
    if (hay.includes(q))
      hits.push({ type: currentLang === 'en' ? 'University' : 'جامعة', title: currentLang === 'en' ? u.nameEn : u.nameAr, action: `askAboutUniversity(${JSON.stringify(currentLang === 'en' ? 'Tell me about ' + u.nameEn : 'أخبرني عن ' + u.nameAr)})` });
  });
  box.hidden = false;
  box.innerHTML = hits.slice(0, 12).map(h =>
    `<button type="button" class="search-hit" onclick="${h.action}"><span class="meta-tag">${escapeHtml(h.type)}</span> ${escapeHtml(h.title)}</button>`
  ).join('') || `<div class="empty">${t('noCourses')}</div>`;
}

async function loadUniversities() {
  if (!authToken || !contentUnlocked) {
    const box = document.getElementById('universitiesResults');
    if (box) box.innerHTML = gateCardHtml();
    return;
  }
  if (universitiesLoaded) { renderUniversities(); return; }
  const box = document.getElementById('universitiesResults');
  if (box) box.innerHTML = `<div class="loading">${t('uniLoading')}</div>`;
  try {
    const res = await fetch('/data/universities.json', { cache: 'no-cache' });
    if (!res.ok) throw new Error('load failed');
    const data = await res.json();
    universitiesData = Array.isArray(data.universities) ? data.universities : [];
    universitiesLoaded = true;
    updateStats();
    renderUniversities();
  } catch {
    if (box) box.innerHTML = `<div class="error">${t('uniFailed')}</div>`;
  }
}

function filteredUniversities() {
  const q = (document.getElementById('uniSearch')?.value || '').trim().toLowerCase();
  const region = document.getElementById('uniRegion')?.value || '';
  const field = document.getElementById('uniField')?.value || '';
  return universitiesData.filter(u => {
    if (region && u.region !== region) return false;
    if (field && !(u.fields || []).includes(field)) return false;
    if (!q) return true;
    const hay = [u.nameAr, u.nameEn, u.countryAr, u.countryEn, u.focusAr, u.focusEn, ...(u.fields || [])].join(' ').toLowerCase();
    return hay.includes(q);
  });
}

function regionLabel(region) {
  const map = {
    africa: 'regAfrica', middle_east: 'regMiddleEast', europe: 'regEurope',
    north_america: 'regNorthAmerica', asia: 'regAsia', oceania: 'regOceania',
    latin_america: 'regLatinAmerica'
  };
  return map[region] ? t(map[region]) : region;
}

function renderUniversities() {
  const box = document.getElementById('universitiesResults');
  const countEl = document.getElementById('uniCount');
  if (!box) return;
  if (!universitiesLoaded) {
    box.innerHTML = `<div class="loading">${t('uniLoading')}</div>`;
    return;
  }
  const list = filteredUniversities();
  if (countEl) countEl.textContent = `${t('uniShowing')} ${list.length} ${t('uniOf')} ${universitiesData.length}`;
  if (!list.length) { box.innerHTML = `<div class="empty">${t('uniNone')}</div>`; return; }
  box.innerHTML = list.map(u => {
    const namePrimary = currentLang === 'en' ? u.nameEn : u.nameAr;
    const nameSecondary = currentLang === 'en' ? u.nameAr : u.nameEn;
    const country = currentLang === 'en' ? u.countryEn : u.countryAr;
    const focus = currentLang === 'en' ? u.focusEn : u.focusAr;
    const fieldTags = (u.fields || []).slice(0, 4).map(f => {
      const key = 'field' + f.charAt(0).toUpperCase() + f.slice(1).replace(/_([a-z])/g, (_, c) => c.toUpperCase());
      return `<span class="meta-tag">${escapeHtml(I18N[currentLang][key] || f)}</span>`;
    }).join('');
    const askQ = currentLang === 'en' ? `Tell me about ${u.nameEn}` : `أخبرني عن ${u.nameAr}`;
    return `<article class="uni-row">
      <div>
        <h4>${escapeHtml(namePrimary)}</h4>
        <div class="uni-en">${escapeHtml(nameSecondary)}</div>
        <p class="uni-focus">${escapeHtml(focus)}</p>
        <div class="meta-row">
          <span class="meta-tag">${escapeHtml(country)}</span>
          <span class="meta-tag">${escapeHtml(regionLabel(u.region))}</span>
          ${fieldTags}
        </div>
      </div>
      <div class="uni-actions">
        <a class="btn btn-secondary btn-sm" href="${escapeHtml(u.website)}" target="_blank" rel="noopener noreferrer">${t('uniVisit')}</a>
        <button class="btn btn-outline btn-sm" type="button" onclick="askAboutUniversity(${JSON.stringify(askQ)})">${t('uniAskAi')}</button>
      </div>
    </article>`;
  }).join('');
}

function askAboutUniversity(question) {
  openTab('ai');
  const input = document.getElementById('aiMessage');
  if (input) input.value = question;
  askStudyAi();
}

function openTab(name) {
  const tab = document.querySelector(`.tab[data-tab="${name}"]`);
  if (tab) tab.click();
  else {
    document.querySelectorAll('.tab').forEach(x => x.classList.toggle('active', x.dataset.tab === name));
    document.querySelectorAll('.workspace .content').forEach(c => c.classList.toggle('active', c.id === name));
  }
  document.getElementById('learn')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

function enrollCourse(courseId) {
  ensureCourseProgress(courseId);
  renderCourses();
  renderLandingCourses();
  renderMine();
  openCourse(courseId);
}
function enrollFromModal() {
  if (!openCourseId) return;
  enrollCourse(openCourseId);
}

function openCourse(courseId) {
  const course = COURSES.find(c => c.id === courseId);
  if (!course) return;
  openCourseId = courseId;
  const enrolled = isEnrolled(courseId);
  const progress = getProgress()[courseId] || { completed: [] };
  const objs = locList(course.objectives || course.outcomes).map(o => `<li>${escapeHtml(o)}</li>`).join('');
  const outcomes = locList(course.outcomes).map(o => `<li>${escapeHtml(o)}</li>`).join('');
  const prereq = locList(course.prerequisites).map(o => `<li>${escapeHtml(o)}</li>`).join('');
  const weeks = locList(course.weeklyPlan).map(w => `<li>${t('week')} ${w.week}: ${escapeHtml(w.topic)} (${w.hours || ''} ${t('hours')})</li>`).join('');
  const slides = (course.slides || []).map(s => `<li><a href="${escapeHtml(s.url || '#')}" target="_blank" rel="noopener">${escapeHtml(loc(s.title))}</a></li>`).join('');
  const videos = (course.videos || []).map(v => `<li>${escapeHtml(loc(v.title))}${v.placeholder ? ` <span class="soon-badge">${t('soonLabel')}</span>` : ''} ${v.url && v.url !== '#' ? `<a href="${escapeHtml(v.url)}" target="_blank" rel="noopener">↗</a>` : ''}</li>`).join('');
  const labs = (course.labs || []).map(l => `<li>${escapeHtml(loc(l.title))}${l.soon ? ` <span class="soon-badge">${t('soonLabel')}</span>` : ''}</li>`).join('');
  const assigns = (course.assignmentsList || []).map(a => `<li>${escapeHtml(loc(a.title))} · W${a.dueWeek || ''}</li>`).join('');
  const exams = (course.exams || []).map(e => `<li>${escapeHtml(loc(e.title))}${e.soon ? ` <span class="soon-badge">${t('soonLabel')}</span>` : ''}</li>`).join('');
  const sources = (course.sources || []).map(s => `<li><a href="${escapeHtml(s.url || '#')}" target="_blank" rel="noopener">${escapeHtml(s.title)}</a></li>`).join('');
  const refs = (course.refs || []).map(s => `<li><a href="${escapeHtml(s.url || '#')}" target="_blank" rel="noopener">${escapeHtml(s.title)}</a></li>`).join('');
  document.getElementById('courseModalTitle').textContent = loc(course.title);
  document.getElementById('courseModalSummary').innerHTML = `
    <p>${escapeHtml(loc(course.description))}</p>
    <p class="modal-meta">${levelLabel(course.level)} · ${course.hours || ''} ${t('hours')}
      ${course.certificate ? ` · ${t('hasCert')}` : ''} ${course.project ? ` · ${t('hasProject')}` : ''}</p>
    ${outcomes || objs ? `<div class="academic-block"><h4>${t('outcomes')}</h4><ul>${outcomes || objs}</ul></div>` : ''}
    ${prereq ? `<div class="academic-block"><h4>${t('prerequisites')}</h4><ul>${prereq}</ul></div>` : ''}
    ${weeks ? `<div class="academic-block"><h4>${t('weeklyPlan')}</h4><ul>${weeks}</ul></div>` : ''}
    ${slides ? `<div class="academic-block"><h4>${t('slides')}</h4><ul>${slides}</ul></div>` : ''}
    ${videos ? `<div class="academic-block"><h4>${t('videos')}</h4><ul>${videos}</ul></div>` : ''}
    ${labs ? `<div class="academic-block"><h4>${t('labs')}</h4><ul>${labs}</ul></div>` : ''}
    ${assigns ? `<div class="academic-block"><h4>${t('assignmentsLabel')}</h4><ul>${assigns}</ul></div>` : ''}
    ${exams ? `<div class="academic-block"><h4>${t('examsLabel')}</h4><ul>${exams}</ul></div>` : ''}
    ${sources ? `<div class="academic-block"><h4>${t('sourcesLabel')}</h4><ul>${sources}</ul></div>` : ''}
    ${refs ? `<div class="academic-block"><h4>${t('refsLabel')}</h4><ul>${refs}</ul></div>` : ''}
    ${course.discussions?.soon ? `<p class="meta-tag">${t('discussions')} · ${t('soonLabel')}</p>` : ''}
    <h4 style="margin-top:12px">${t('lessons')}</h4>`;
  document.getElementById('courseModalLessons').innerHTML = (course.lessons || []).map(l => {
    const isDone = (progress.completed || []).includes(l.id);
    return `<li class="${isDone ? 'done' : ''}">
      <span>${isDone ? '✓ ' : ''}${escapeHtml(loc(l.title))}</span>
      <span style="display:flex;gap:6px;flex-wrap:wrap">
        <button type="button" onclick="quickSaveCourseLesson('${course.id}','${l.id}')">${t('lecSaveFromCourse')}</button>
        <button type="button" onclick="openLesson('${course.id}','${l.id}')">${t('continueLearning')}</button>
      </span>
    </li>`;
  }).join('');
  const enrollBtn = document.getElementById('courseEnrollBtn');
  if (enrollBtn) {
    enrollBtn.textContent = enrolled ? t('enrolled') : t('enroll');
    enrollBtn.disabled = enrolled;
  }
  openModal('courseModal');
}

function openLesson(courseId, lessonId) {
  const course = COURSES.find(c => c.id === courseId);
  const lesson = course?.lessons.find(l => l.id === lessonId);
  if (!course || !lesson) return;
  ensureCourseProgress(courseId);
  activeLesson = { courseId, lessonId };
  document.getElementById('lessonModalTitle').textContent = loc(lesson.title);
  document.getElementById('lessonModalMeta').textContent = `${loc(course.title)} · ${t('lessons')}`;
  document.getElementById('lessonModalBody').innerHTML = `<p>${escapeHtml(loc(lesson.body))}</p>`;
  const done = (getProgress()[courseId]?.completed || []).includes(lessonId);
  const btn = document.getElementById('lessonCompleteBtn');
  if (btn) {
    btn.disabled = done;
    btn.textContent = done ? t('completed') : t('markDone');
  }
  closeModal('courseModal');
  openModal('lessonModal');
  renderMine();
  renderCourses();
}

function completeCurrentLesson() {
  if (!activeLesson) return;
  const data = getProgress();
  if (!data[activeLesson.courseId]) data[activeLesson.courseId] = { enrolled: true, completed: [] };
  const entry = data[activeLesson.courseId];
  entry.enrolled = true;
  if (!Array.isArray(entry.completed)) entry.completed = [];
  if (!entry.completed.includes(activeLesson.lessonId)) entry.completed.push(activeLesson.lessonId);
  saveProgress(data);
  const btn = document.getElementById('lessonCompleteBtn');
  if (btn) { btn.disabled = true; btn.textContent = t('completed'); }
  renderMine();
  renderCourses();
  renderLandingCourses();
}

function renderAiSuggestions() {
  const box = document.getElementById('aiSuggestions');
  if (!box) return;
  const tips = currentLang === 'en'
    ? ['Explain Ohm’s law', 'Review Arduino Blink', 'Books on control systems', 'MIT electrical engineering']
    : ['اشرح قانون أوم', 'راجع كود Arduino Blink', 'كتب عن أنظمة التحكم', 'جامعة MIT تقنية كهرباء'];
  box.innerHTML = tips.map(s =>
    `<button type="button" onclick="document.getElementById('aiMessage').value=${JSON.stringify(s)};askStudyAi()">${escapeHtml(s)}</button>`
  ).join('');
}

function renderBooks(books) {
  const box = document.getElementById('aiBooks');
  if (!box) return;
  if (!books?.length) { box.innerHTML = ''; return; }
  box.innerHTML = `<h3 style="grid-column:1/-1;font-family:var(--font-display);color:var(--nile-deep);margin:8px 0 4px;">${t('booksTitle')}</h3>` +
    books.map(b => {
      const cover = b.coverUrl
        ? `<img src="${escapeHtml(b.coverUrl)}" alt="" loading="lazy" onerror="this.style.visibility='hidden'">`
        : `<div style="width:64px;height:90px;background:var(--stone-dark)"></div>`;
      const year = b.year ? ` · ${b.year}` : '';
      return `<div class="book-card">${cover}<div>
        <h4>${escapeHtml(b.title)}</h4>
        <div class="authors">${escapeHtml(b.authors || '—')}${year}</div>
        <div class="card-actions">
          <a class="btn btn-outline btn-sm" href="${escapeHtml(b.openLibraryUrl)}" target="_blank" rel="noopener">${t('openBook')}</a>
          <a class="btn btn-secondary btn-sm" href="${escapeHtml(b.readUrl || b.openLibraryUrl)}" target="_blank" rel="noopener">${t('readBook')}</a>
        </div></div></div>`;
    }).join('');
}

function renderSources(sources) {
  const box = document.getElementById('aiSources');
  if (!box) return;
  if (!sources?.length) { box.innerHTML = ''; return; }
  box.innerHTML = sources.map(s => `
    <div class="source-card">
      <h4>${t('sourcesTitle')}: ${escapeHtml(s.title)}</h4>
      <p style="color:var(--muted);font-size:0.92rem;margin-bottom:8px;">${escapeHtml((s.summary || '').slice(0, 420))}${(s.summary || '').length > 420 ? '…' : ''}</p>
      <a href="${escapeHtml(s.url)}" target="_blank" rel="noopener">${escapeHtml(s.source || 'wikipedia')} ↗</a>
    </div>`).join('');
}

async function askStudyAi() {
  const message = (document.getElementById('aiMessage')?.value || '').trim();
  const replyDiv = document.getElementById('aiReply');
  if (!contentUnlocked || !authToken) {
    if (replyDiv) replyDiv.innerHTML = gateCardHtml();
    openModal('registerModal');
    return;
  }
  if (!message) {
    replyDiv.innerHTML = `<div class="error">${currentLang === 'en' ? 'Type a question first' : 'اكتب سؤالك أولاً'}</div>`;
    return;
  }
  replyDiv.innerHTML = `<div class="loading">${t('aiLoading')}</div>`;
  document.getElementById('aiBooks').innerHTML = '';
  document.getElementById('aiSources').innerHTML = '';
  try {
    const result = await api('/ai/study', { method: 'POST', body: JSON.stringify({ message, language: currentLang }) });
    replyDiv.innerHTML = `<div class="success">${escapeHtml(result.reply || '').replace(/\n/g, '<br>')}</div>
      <div style="margin-top:6px;color:var(--muted);font-size:0.85rem;">${currentLang === 'en' ? 'Engine' : 'المحرك'}: ${escapeHtml(result.provider)} · ${escapeHtml(result.intent || '')}</div>`;
    renderSources(result.sources || []);
    renderBooks(result.books || []);
  } catch {
    const local = localStudyFallback(message);
    replyDiv.innerHTML = `<div class="error">${t('aiFailed')}</div><div class="success" style="margin-top:10px;">${escapeHtml(local).replace(/\n/g, '<br>')}</div>`;
    try {
      const books = await api(`/ai/books?q=${encodeURIComponent(message)}&lang=${currentLang}&limit=6`);
      renderBooks(books);
    } catch { /* */ }
  }
}

async function searchBooksOnly() {
  const message = (document.getElementById('aiMessage')?.value || '').trim();
  const replyDiv = document.getElementById('aiReply');
  if (!message) {
    replyDiv.innerHTML = `<div class="error">${currentLang === 'en' ? 'Type a topic' : 'اكتب موضوعاً'}</div>`;
    return;
  }
  replyDiv.innerHTML = `<div class="loading">${t('aiLoading')}</div>`;
  document.getElementById('aiSources').innerHTML = '';
  try {
    const books = await api(`/ai/books?q=${encodeURIComponent(message)}&lang=${currentLang}&limit=8`);
    replyDiv.innerHTML = `<div class="success">${currentLang === 'en' ? `Found ${books.length} books.` : `عُثر على ${books.length} كتاباً.`}</div>`;
    renderBooks(books);
  } catch (err) {
    replyDiv.innerHTML = `<div class="error">${err.message || t('aiFailed')}</div>`;
  }
}

function localStudyFallback(message) {
  const q = message.toLowerCase();
  for (const c of COURSES) {
    for (const l of (c.lessons || [])) {
      const blob = `${loc(c.title)} ${loc(l.title)} ${loc(l.body)}`.toLowerCase();
      if (q.split(/\s+/).filter(w => w.length > 2).some(w => blob.includes(w))) {
        return currentLang === 'en'
          ? `From «${loc(c.title)}» → ${loc(l.title)}:\n\n${loc(l.body)}`
          : `من دورة «${loc(c.title)}» ← ${loc(l.title)}:\n\n${loc(l.body)}`;
      }
    }
  }
  return currentLang === 'en'
    ? 'Start the API for live Wikipedia + Open Library. Browse courses and library meanwhile.'
    : 'شغّل الخادم لويكيبيديا وOpen Library. يمكنك تصفح الدورات والمكتبة الآن.';
}

async function subscribeNewsletter() {
  const email = (document.getElementById('newsletterEmail')?.value || '').trim();
  const msg = document.getElementById('newsletterMsg');
  if (!email || !email.includes('@')) {
    if (msg) msg.innerHTML = `<div class="error">${currentLang === 'en' ? 'Enter a valid email' : 'أدخل بريداً صالحاً'}</div>`;
    return;
  }
  let list = [];
  try { list = JSON.parse(localStorage.getItem(NEWSLETTER_KEY) || '[]'); } catch { list = []; }
  if (!list.includes(email)) list.push(email);
  localStorage.setItem(NEWSLETTER_KEY, JSON.stringify(list));
  try {
    await api('/newsletter', { method: 'POST', body: JSON.stringify({ email, language: currentLang }) });
  } catch { /* local ok */ }
  if (msg) msg.innerHTML = `<div class="success">${t('newsletterOk')}</div>`;
}

function applyRoleUi() {
  const override = localStorage.getItem(ROLE_KEY);
  const role = override || resolveUiRole(currentUser);
  document.querySelectorAll('[data-need-perm]').forEach(el => {
    const perm = el.getAttribute('data-need-perm');
    el.style.display = can(role, perm) ? '' : 'none';
  });
  document.querySelectorAll('[data-soon]').forEach(el => {
    el.setAttribute('title', t('comingSoonHint'));
    el.classList.add('is-soon');
  });
  const label = document.getElementById('roleUiLabel');
  if (label) label.textContent = `${t('roleLabel')}: ${role}`;
}

function setMsg(elId, text, type) {
  const el = document.getElementById(elId);
  if (!el) return;
  if (!text) { el.innerHTML = ''; return; }
  el.innerHTML = `<div class="${type}">${text}</div>`;
}

async function api(path, options) {
  const headers = { 'Content-Type': 'application/json', ...(options?.headers || {}) };
  if (authToken) headers.Authorization = `Bearer ${authToken}`;
  const response = await fetch(`${API_BASE}${path}`, { ...options, headers });
  let data = null;
  const text = await response.text();
  if (text) { try { data = JSON.parse(text); } catch { data = text; } }
  if (!response.ok) {
    const message = typeof data === 'string' ? data : (data?.message || data?.title || data?.detail || 'Error');
    throw new Error(message);
  }
  return data;
}

function applyAuthUi() {
  const guest = document.getElementById('authGuest');
  const userBox = document.getElementById('authUser');
  if (currentUser || (authToken && contentUnlocked)) {
    if (guest) guest.style.display = 'none';
    if (userBox) userBox.style.display = 'inline-flex';
    const nl = document.getElementById('userNameLabel');
    if (nl) nl.textContent = currentUser?.fullName || currentUser?.email || (currentLang === 'en' ? 'Member' : 'عضو');
  } else if (authToken && currentUser) {
    if (guest) guest.style.display = 'none';
    if (userBox) userBox.style.display = 'inline-flex';
    const nl = document.getElementById('userNameLabel');
    if (nl) nl.textContent = currentUser?.fullName || currentUser?.email || (currentLang === 'en' ? 'Member' : 'عضو');
  } else {
    if (guest) guest.style.display = 'inline-flex';
    if (userBox) userBox.style.display = 'none';
  }
  applyRoleUi();
  applySectionGates();
  applyVerifyBanner();
}

async function refreshAuth() {
  if (!authToken) {
    currentUser = null;
    contentUnlocked = false;
    applyAuthUi();
    renderGuestMarketing();
    return;
  }
  try {
    currentUser = await api('/auth/me');
    if (canUnlockCatalog()) {
      contentUnlocked = true;
      applyAuthUi();
      if (!catalogReady) await unlockAndLoad();
      else applySectionGates();
      loadMemberLectures().catch(() => {});
    } else {
      contentUnlocked = false;
      applyAuthUi();
      renderGuestMarketing();
    }
  } catch {
    authToken = '';
    localStorage.removeItem('wadnooh_token');
    currentUser = null;
    contentUnlocked = false;
    memberLectures = [];
    lecturesLoaded = false;
    applyAuthUi();
    renderGuestMarketing();
  }
}

async function doRegister() {
  try {
    const result = await api('/auth/register', {
      method: 'POST',
      body: JSON.stringify({
        fullName: document.getElementById('regName').value.trim(),
        email: document.getElementById('regEmail').value.trim(),
        phone: document.getElementById('regPhone').value.trim(),
        password: document.getElementById('regPassword').value
      })
    });
    authToken = result.token;
    localStorage.setItem('wadnooh_token', authToken);
    currentUser = result.user;
    let msg = t('verifyCheckInbox');
    if (result.devVerifyUrl) msg += ` — ${result.devVerifyUrl}`;
    if (result.devVerifyCode) msg += ` · ${result.devVerifyCode}`;
    setMsg('registerMsg', msg, 'success');
    if (canUnlockCatalog()) {
      contentUnlocked = true;
      applyAuthUi();
      await unlockAndLoad();
    } else {
      contentUnlocked = false;
      applyAuthUi();
      renderGuestMarketing();
    }
    setTimeout(() => closeModal('registerModal'), 1200);
  } catch (err) { setMsg('registerMsg', err.message, 'error'); }
}

async function doLogin() {
  try {
    const result = await api('/auth/login', {
      method: 'POST',
      body: JSON.stringify({
        email: document.getElementById('loginEmail').value.trim(),
        password: document.getElementById('loginPassword').value
      })
    });
    authToken = result.token;
    localStorage.setItem('wadnooh_token', authToken);
    currentUser = result.user;
    if (canUnlockCatalog()) {
      contentUnlocked = true;
      applyAuthUi();
      await unlockAndLoad();
      setMsg('loginMsg', currentLang === 'en' ? 'Welcome back — content unlocked' : 'مرحباً بعودتك — تم فتح المحتوى', 'success');
    } else {
      contentUnlocked = false;
      applyAuthUi();
      renderGuestMarketing();
      setMsg('loginMsg', t('verifyNeedConfirm'), 'success');
    }
    setTimeout(() => closeModal('loginModal'), 900);
  } catch (err) { setMsg('loginMsg', err.message, 'error'); }
}

function logout() {
  authToken = '';
  currentUser = null;
  localStorage.removeItem('wadnooh_token');
  lockContent();
}

async function loadMembershipPlans() {
  const box = document.getElementById('membershipPlans');
  const status = document.getElementById('membershipStatus');
  if (!box) return;
  try {
    const plans = await api('/membership/plans');
    if (currentUser?.membership) {
      const m = currentUser.membership;
      status.innerHTML = `${currentLang === 'en' ? 'Current plan' : 'باقتك الحالية'}: <strong>${currentLang === 'en' ? m.planNameEn : m.planNameAr}</strong>`;
    } else {
      status.innerHTML = currentLang === 'en' ? 'Free member — upgrade optionally' : 'عضوية مجانية — ترقية اختيارية';
    }
    box.innerHTML = plans.map(p => `
      <div class="card">
        <h4>${currentLang === 'en' ? p.nameEn : p.nameAr}</h4>
        <p>${currentLang === 'en' ? p.descriptionEn : p.descriptionAr}</p>
        <div class="meta-row">
          <span class="meta-tag">${p.price} ${p.currency}</span>
          <span class="meta-tag">${p.durationDays} ${currentLang === 'en' ? 'days' : 'يوم'}</span>
        </div>
        <div class="card-actions">
          <button class="btn btn-secondary btn-sm" type="button" onclick="subscribePlan(${p.id})">${t('subscribe')}</button>
        </div>
      </div>`).join('');
  } catch {
    if (status) status.innerHTML = currentLang === 'en'
      ? 'Membership plans appear when the API is online.'
      : 'باقات العضوية تظهر عند تشغيل الـ API.';
    box.innerHTML = '';
  }
}

async function subscribePlan(planId) {
  if (!authToken) { openModal('loginModal'); return; }
  try {
    const checkout = await api('/membership/subscribe', {
      method: 'POST',
      body: JSON.stringify({ purpose: 'membership', membershipPlanId: planId })
    });
    await startCheckout(checkout);
  } catch (err) { alert(err.message); }
}

async function startCheckout(checkout) {
  if (!checkout) return;
  if (checkout.provider === 'demo' || (checkout.checkoutUrl || '').includes('demoPay=')) {
    pendingDemoPaymentId = checkout.paymentId;
    document.getElementById('demoPaySummary').textContent = `${checkout.amount} ${checkout.currency} · #${checkout.paymentId}`;
    setMsg('demoPayMsg', '', '');
    openModal('demoPayModal');
    return;
  }
  window.location.href = checkout.checkoutUrl;
}

async function completeDemoPayment() {
  if (!pendingDemoPaymentId) return;
  try {
    const result = await api('/payments/demo/complete', {
      method: 'POST', body: JSON.stringify({ paymentId: pendingDemoPaymentId })
    });
    setMsg('demoPayMsg', result.message || 'OK', 'success');
    await refreshAuth();
    setTimeout(() => {
      closeModal('demoPayModal');
      document.querySelector('[data-tab="membership"]')?.click();
      loadMembershipPlans();
    }, 800);
  } catch (err) { setMsg('demoPayMsg', err.message, 'error'); }
}

function openModal(id) { document.getElementById(id)?.classList.add('open'); }
function closeModal(id) { document.getElementById(id)?.classList.remove('open'); }

function bindUi() {
  document.querySelectorAll('.workspace .tab').forEach(tab => {
    tab.addEventListener('click', () => {
      const name = tab.dataset.tab;
      if (tab.hasAttribute('data-soon')) return;
      document.querySelectorAll('.workspace .tab').forEach(x => x.classList.remove('active'));
      document.querySelectorAll('.workspace .content').forEach(c => c.classList.remove('active'));
      tab.classList.add('active');
      document.getElementById(name)?.classList.add('active');
      if (name === 'mine') {
        if (mineSubTab === 'lectures') loadMemberLectures().finally(() => renderMine());
        else renderMine();
      }
      if (name === 'membership') loadMembershipPlans();
      if (name === 'paths') renderPaths();
      if (name === 'universities') loadUniversities();
      if (name === 'library') renderLibrary();
      if (name === 'projects') renderProjectsTab();
      if (name === 'portfolio') renderPortfolio();
    });
  });
  document.getElementById('courseSearch')?.addEventListener('input', renderCourses);
  document.getElementById('levelFilter')?.addEventListener('change', renderCourses);
  document.getElementById('librarySearch')?.addEventListener('input', renderLibrary);
  document.getElementById('uniSearch')?.addEventListener('input', renderUniversities);
  document.getElementById('uniRegion')?.addEventListener('change', renderUniversities);
  document.getElementById('uniField')?.addEventListener('change', renderUniversities);
  document.getElementById('smartSearch')?.addEventListener('input', runSmartSearch);
  document.getElementById('aiMessage')?.addEventListener('keydown', (e) => { if (e.key === 'Enter') askStudyAi(); });
  document.querySelectorAll('.modal-overlay').forEach(overlay => {
    overlay.addEventListener('click', (e) => { if (e.target === overlay) overlay.classList.remove('open'); });
  });
}

document.addEventListener('DOMContentLoaded', () => {
  migrateProgress();
  bindUi();
  setLang(currentLang);
  applySectionGates();
  handleVerifyQuery().finally(() => {
    if (authToken) {
      refreshAuth();
    } else {
      renderGuestMarketing();
      applyAuthUi();
    }
  });
  if (sessionStorage.getItem('wep_open_register') === '1') {
    sessionStorage.removeItem('wep_open_register');
    setTimeout(() => openModal('registerModal'), 400);
  }
  const openTabName = sessionStorage.getItem('wep_open_tab');
  if (openTabName) {
    sessionStorage.removeItem('wep_open_tab');
    setTimeout(() => { if (typeof openTab === 'function') openTab(openTabName); }, 500);
  }
  const openCourse = sessionStorage.getItem('wep_open_course');
  if (openCourse) {
    sessionStorage.removeItem('wep_open_course');
    setTimeout(() => {
      if (!contentUnlocked) { openModal('registerModal'); return; }
      if (typeof openCourse === 'function') openCourse(openCourse);
      else if (typeof openTab === 'function') openTab('courses');
    }, 700);
  }
});
