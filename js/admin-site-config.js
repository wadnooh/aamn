(function () {
  const settingsKey = 'aamn_bus_site_settings_v2';
  const departmentsKey = 'aamn_bus_departments_v2';
  const itemsKey = 'aamn_bus_dept_items_v2';
  const defaultSettings = {
    brandAr: 'الاتحاد للخدمات',
    brandEn: '',
    email: 'info@2-aa.com',
    phone: '+966500000000',
    whatsapp: '966500000000',
    domain: '2-aa.com',
    address: 'السودان - خدمة إلكترونية لجميع المدن',
    hours: 'طوال أيام الأسبوع حسب توفر الرحلات',
    description: 'منصة حجز بصات سفرية داخل السودان تربط الركاب بأصحاب البصات مع محفظة واشتراكات أعمال.'
  };
  const defaultDepartments = [
    { key: 'booking', title: 'حجز التذاكر', icon: 'fas fa-ticket', order: 1, status: 'active', description: 'بحث وحجز مقاعد البصات السفرية بين مدن السودان مع تأكيد فوري وبيانات رحلة واضحة.' },
    { key: 'operators', title: 'أصحاب البصات', icon: 'fas fa-briefcase', order: 2, status: 'active', description: 'تسجيل شركات وأصحاب البصات، توثيق البيانات، وإدارة الأسطول والرحلات من لوحة واحدة.' },
    { key: 'buses', title: 'إدارة البصات', icon: 'fas fa-bus', order: 3, status: 'active', description: 'إضافة البصات، عدد المقاعد، الصور، اللوحات، مستوى الخدمة، وحالة الجاهزية للنشر.' },
    { key: 'routes', title: 'الخطوط والمدن', icon: 'fas fa-route', order: 4, status: 'active', description: 'ربط جميع مدن السودان بخطوط سفر منظمة وأسعار ومواعيد قابلة للتحديث.' },
    { key: 'wallet', title: 'المحفظة والضمان', icon: 'fas fa-wallet', order: 5, status: 'active', description: 'محفظة مالية تحفظ حقوق العميل وصاحب البص، وتتابع المدفوعات والاسترداد والعمولات.' },
    { key: 'subscriptions', title: 'اشتراكات الأعمال', icon: 'fas fa-id-card', order: 6, status: 'active', description: 'باقات شهرية وسنوية لأصحاب البصات تتيح نشر الرحلات وإدارة الحجوزات والتقارير.' },
    { key: 'support', title: 'الدعم والمتابعة', icon: 'fas fa-headset', order: 7, status: 'active', description: 'متابعة الحجوزات والشكاوى والتعديلات والتنبيهات لضمان تجربة سفر مستقرة.' }
  ];
  const defaultItems = {
    booking: ['بحث حسب المدينة والتاريخ', 'اختيار المقعد ونوع الخدمة', 'تأكيد الحجز برسالة للعميل', 'إدارة الإلغاء والاسترداد'],
    operators: ['ملف صاحب البص أو الشركة', 'توثيق الهوية والسجل', 'متابعة الاشتراك والصلاحيات', 'تقارير الحجوزات والمبيعات'],
    buses: ['بيانات البص وعدد المقاعد', 'اللوحة والصور ومستوى الخدمة', 'حالة البص وجدول الجاهزية', 'نشر أو إيقاف البص من لوحة التحكم'],
    routes: ['الخرطوم، مدني، بورتسودان، كسلا، القضارف', 'الأبيض، نيالا، الفاشر، عطبرة، دنقلا', 'مواعيد الانطلاق والوصول', 'أسعار مرنة حسب الخط والشركة'],
    wallet: ['رصيد العميل وصاحب البص', 'حجز المبلغ حتى تأكيد الرحلة', 'عمولة المنصة وتقارير التسوية', 'استرداد منظم عند الإلغاء'],
    subscriptions: ['باقة أساسية لصاحب بص واحد', 'باقة شركات لعدة بصات', 'إعلانات وتثبيت رحلات مميزة', 'تقارير شهرية وفواتير اشتراك'],
    support: ['متابعة الحجز قبل السفر', 'تنبيهات تغيير الموعد', 'الشكاوى والمفقودات', 'دعم واتساب وبريد']
  };

  function read(key, fallback) {
    try {
      return JSON.parse(localStorage.getItem(key) || 'null') || fallback;
    } catch {
      return fallback;
    }
  }
  function hasLegacyText(value) {
    const text = JSON.stringify(value || '').toLowerCase();
    const legacyWords = [
      'aa' + 'mn',
      'wad' + 'nooh',
      'wd' + ' nooh',
      'ود' + 'نوح',
      'ود' + ' نوح',
      'برمجيات' + ' والكمبيوتر',
      'كهر' + 'باء',
      'إلكترو' + 'نيات',
      'أج' + 'هزة',
      'مخت' + 'برات',
      'vi' + 'p',
      'business' + ' wallet'
    ];
    return legacyWords.some((word) => text.includes(word.toLowerCase()));
  }
  function esc(value) {
    return String(value || '').replace(/[&<>"']/g, (s) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[s]));
  }
  function visibleDepartments() {
    const stored = read(departmentsKey, defaultDepartments);
    const rows = Array.isArray(stored)
      ? stored
      : Object.entries(stored || {}).map(([key, dept]) => ({
        key,
        title: dept.title,
        icon: dept.icon,
        order: dept.order,
        status: dept.status,
        description: dept.description || dept.desc
      }));
    return rows
      .filter((dept) => dept.status !== 'hidden')
      .sort((a, b) => Number(a.order || 0) - Number(b.order || 0));
  }
  function itemNames(key) {
    const data = read(itemsKey, {});
    const rows = Array.isArray(data[key]) ? data[key] : [];
    const names = rows.filter((item) => item.status !== 'unavailable').map((item) => item.name).filter(Boolean);
    return names.length ? names : (defaultItems[key] || []);
  }

  let settings = read(settingsKey, defaultSettings);
  if (hasLegacyText(settings) || settings.domain === ('wad' + 'nooh.com') || settings.email === ('info@' + 'wad' + 'nooh.tech')) {
    settings = { ...defaultSettings };
    try {
      localStorage.setItem(settingsKey, JSON.stringify(settings));
    } catch {
      // Keep rendering with clean defaults even when storage is unavailable.
    }
  }
  const storedDepartments = read(departmentsKey, defaultDepartments);
  const storedItems = read(itemsKey, defaultItems);
  if (hasLegacyText(storedDepartments) || hasLegacyText(storedItems)) {
    try {
      localStorage.setItem(departmentsKey, JSON.stringify(defaultDepartments));
      localStorage.setItem(itemsKey, JSON.stringify(defaultItems));
    } catch {
      // The default render path below still uses clean platform data.
    }
  }
  if (settings) {
    const brandAr = 'الاتحاد للخدمات';
    const fullName = `${brandAr} لحجز البصات السفرية`;

    document.querySelectorAll('.logo-main').forEach((el) => {
      el.textContent = brandAr;
    });
    document.querySelectorAll('.logo-sub').forEach((el) => {
      el.textContent = 'لحجز البصات السفرية';
    });
    document.querySelectorAll('.footer-logo-text').forEach((el) => {
      el.textContent = brandAr;
    });
    document.querySelectorAll('.footer-brand p').forEach((el) => {
      el.textContent = `${fullName} - ${settings.description || 'منصة حجز بصات سفرية تربط الركاب بأصحاب البصات داخل السودان.'}`;
    });
    document.querySelectorAll('a[href^="mailto:"], .contact-item span').forEach((el) => {
      if (settings.email && /@/.test(el.textContent || el.getAttribute('href') || '')) {
        if (el.tagName === 'A') {
          el.href = `mailto:${settings.email}`;
          el.textContent = settings.email;
        } else {
          el.textContent = settings.email;
        }
      }
    });
    document.querySelectorAll('a[href^="tel:"], .contact-item span[dir="ltr"]').forEach((el) => {
      if (!settings.phone) return;
      if (el.tagName === 'A') {
        el.href = `tel:${settings.phone.replace(/\s+/g, '')}`;
        el.textContent = settings.phone;
      } else if (/^\+|[0-9]/.test(el.textContent || '')) {
        el.textContent = settings.phone;
      }
    });
    if (settings.address) {
      document.querySelectorAll('.contact-item span').forEach((el) => {
        if ((el.textContent || '').includes('الرياض') || (el.textContent || '').includes('المملكة') || (el.textContent || '').includes('السودان')) {
          el.textContent = settings.address;
        }
      });
    }
    if (document.title.includes('الاتحاد للخدمات') || document.title.includes('الاتحاد للخدمات') || document.title.includes('الاتحاد')) {
      const pageName = document.title.split('-')[0].split('|')[0].trim();
      document.title = pageName && !/الاتحاد للخدمات|الاتحاد للخدمات|الاتحاد/i.test(pageName) ? `${pageName} - ${fullName}` : fullName;
    }
  }

  const commonText = new Map([
    ['خدماتنا', 'أقسام المنصة'],
    ['أعمالنا', 'الخطوط والرحلات'],
    ['احصل على عرض', 'احجز أو سجل بصك'],
    ['احصل على عرض سعر', 'احجز أو سجل بصك'],
    ['طلب عرض سعر', 'ابدأ الآن'],
    ['شريكك في الحجز والبصات والخطوط والمحفظة.', 'منصة حجز بصات سفرية تربط الركاب بأصحاب البصات داخل السودان.'],
    ['شريكك في الحجز والبصات والخطوط والمحفظة بجودة والتزام.', 'منصة حجز بصات سفرية داخل السودان مع محفظة واشتراكات لأصحاب البصات.']
  ]);
  document.querySelectorAll('a, h1, h2, h3, h4, p, span, button, option, small, li').forEach((el) => {
    const text = (el.textContent || '').trim();
    if (commonText.has(text)) el.textContent = commonText.get(text);
  });

  const departments = visibleDepartments();
  const servicesGrid = document.querySelector('.services-grid');
  if (servicesGrid) {
    servicesGrid.innerHTML = departments.map((dept, index) => `
      <div class="service-card" data-aos="fade-up" data-delay="${index * 100}">
        <div class="service-icon"><i class="${esc(dept.icon || 'fas fa-circle')}"></i></div>
        <h3>${esc(dept.title)}</h3>
        <p>${esc(dept.description)}</p>
        <a href="services.html" class="service-link">التفاصيل <i class="fas fa-arrow-left"></i></a>
      </div>
    `).join('');
  }

  const servicesDetailed = document.querySelector('.services-detailed');
  if (servicesDetailed) {
    servicesDetailed.innerHTML = departments.map((dept, index) => `
      <div class="service-detail-card${index % 2 ? ' reverse' : ''}">
        <div class="sd-icon"><i class="${esc(dept.icon || 'fas fa-circle')}"></i></div>
        <div class="sd-content">
          <h3>${esc(dept.title)}</h3>
          <p>${esc(dept.description)}</p>
          <ul class="sd-features">
            ${itemNames(dept.key).map((name) => `<li><i class="fas fa-check"></i> ${esc(name)}</li>`).join('')}
          </ul>
          <a href="${dept.key === 'booking' || dept.key === 'wallet' ? 'client.html' : (dept.key === 'operators' || dept.key === 'buses' || dept.key === 'routes' || dept.key === 'subscriptions' ? 'operator.html' : 'contact.html')}" class="btn-primary">${dept.key === 'booking' ? 'احجز الآن' : (dept.key === 'support' ? 'طلب دعم' : 'ابدأ الآن')} <i class="fas fa-arrow-left"></i></a>
        </div>
      </div>
    `).join('');
  }

  const serviceSelect = document.querySelector('select[name="service"]');
  if (serviceSelect) {
    const isContactPage = /contact\.html$/i.test(location.pathname);
    if (isContactPage) {
      serviceSelect.innerHTML = [
        '<option value="">اختر نوع الطلب</option>',
        '<option value="support">دعم ومتابعة تذكرة</option>',
        '<option value="wallet">المحفظة والضمان</option>',
        '<option value="route">استفسار عن خط أو مدينة</option>',
        '<option value="other">أخرى</option>'
      ].join('');
    } else {
      serviceSelect.innerHTML = '<option value="">اختر نوع الطلب</option>' + departments.map((dept) => `<option value="${esc(dept.key)}">${esc(dept.title)}</option>`).join('') + '<option value="other">أخرى</option>';
    }
  }

  document.querySelectorAll('.footer-services ul').forEach((list) => {
    list.innerHTML = departments.map((dept) => `<li><a href="services.html"><i class="fas fa-angle-left"></i> ${esc(dept.title)}</a></li>`).join('');
  });
})();
