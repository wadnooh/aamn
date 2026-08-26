(function () {
  const settingsKey = 'aamn_site_settings';
  const departmentsKey = 'aamn_departments';
  const itemsKey = 'aamn_department_items';
  const defaultDepartments = [
    { key: 'computers', title: 'خدمات الكمبيوتر', icon: 'fas fa-computer', order: 1, status: 'active', description: 'بيع وتجميع وصيانة أجهزة الكمبيوتر واللابتوبات وترقية القطع وحل مشاكل الأنظمة.' },
    { key: 'programming', title: 'برمجة المواقع والتطبيقات', icon: 'fas fa-code', order: 2, status: 'active', description: 'تصميم وبرمجة مواقع وتطبيقات عملية وسريعة تناسب نشاطك وتظهر هويتك بوضوح.' },
    { key: 'electricity', title: 'أنظمة الكهرباء', icon: 'fas fa-bolt', order: 3, status: 'active', description: 'تركيب وصيانة التمديدات واللوحات وأنظمة الإنارة والحماية الكهربائية.' },
    { key: 'electronics', title: 'الإلكترونيات', icon: 'fas fa-microchip', order: 4, status: 'active', description: 'توريد وتركيب وصيانة الدوائر والملحقات والحلول الإلكترونية للأعمال والمنازل.' },
    { key: 'devices', title: 'الأجهزة والإكسسوارات', icon: 'fas fa-print', order: 5, status: 'active', description: 'توفير أجهزة كمبيوتر وملحقات وشاشات وطابعات وحلول شبكات حسب الاحتياج.' },
    { key: 'labs', title: 'مبيعات المختبرات', icon: 'fas fa-flask', order: 6, status: 'active', description: 'توريد أجهزة ومستهلكات المختبرات، الميزان الحساس، المجاهر، ومعدات التشغيل والسلامة.' },
    { key: 'support', title: 'الدعم والصيانة', icon: 'fas fa-screwdriver-wrench', order: 7, status: 'active', description: 'دعم فني مستمر، متابعة الأعطال، تحديثات البرامج، وحماية البيانات بعد التسليم.' }
  ];
  const defaultItems = {
    computers: ['تجميع أجهزة مكتبية واحترافية', 'ترقية الرام والتخزين وكروت الشاشة', 'تثبيت الأنظمة والبرامج الأساسية', 'صيانة الأعطال ونقل البيانات'],
    programming: ['مواقع تعريفية ومتاجر إلكترونية', 'تطبيقات ويب ولوحات تحكم', 'ربط بوابات دفع ونماذج تواصل', 'تحسين السرعة وتجربة الجوال'],
    electricity: ['تمديدات كهربائية آمنة', 'لوحات توزيع وتنظيم أحمال', 'إنارة داخلية وخارجية', 'صيانة وحماية كهربائية'],
    electronics: ['دوائر إلكترونية ووحدات تحكم', 'حساسات وتنبيهات تشغيل', 'صيانة ملحقات وأجهزة صغيرة', 'تجهيز حلول ذكية حسب الطلب'],
    devices: ['أجهزة مكتبية ولابتوبات', 'شاشات وطابعات وملحقات', 'راوترات وسويتشات وحلول شبكات', 'قطع غيار وترقيات حسب الطلب'],
    labs: ['مجاهر رقمية وأجهزة قياس', 'موازين حساسة ومزودات طاقة مختبر', 'مستهلكات وأدوات تشغيل وسلامة', 'توريد وصيانة ومعايرة حسب الطلب'],
    support: ['زيارات صيانة دورية', 'تحديثات أنظمة وبرامج', 'حماية بيانات ونسخ احتياطي', 'متابعة واتساب أو بريد عند الحاجة']
  };

  function read(key, fallback) {
    try {
      return JSON.parse(localStorage.getItem(key) || 'null') || fallback;
    } catch {
      return fallback;
    }
  }
  function esc(value) {
    return String(value || '').replace(/[&<>"']/g, (s) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[s]));
  }
  function visibleDepartments() {
    return read(departmentsKey, defaultDepartments)
      .filter((dept) => dept.status !== 'hidden')
      .sort((a, b) => Number(a.order || 0) - Number(b.order || 0));
  }
  function itemNames(key) {
    const data = read(itemsKey, {});
    const rows = Array.isArray(data[key]) ? data[key] : [];
    const names = rows.filter((item) => item.status !== 'unavailable').map((item) => item.name).filter(Boolean);
    return names.length ? names : (defaultItems[key] || []);
  }

  const settings = read(settingsKey, null);
  if (settings) {
    const brandAr = settings.brandAr || 'ودنوح';
    const brandEn = settings.brandEn || 'AAMN';
    const fullName = `${brandAr} ${brandEn} للبرمجيات والكمبيوتر`;

    document.querySelectorAll('.logo-main').forEach((el) => {
      el.innerHTML = `${brandAr} <span class="brand-en">${brandEn}</span>`;
    });
    document.querySelectorAll('.logo-sub').forEach((el) => {
      el.textContent = 'للبرمجيات والكمبيوتر';
    });
    document.querySelectorAll('.footer-logo-text').forEach((el) => {
      el.textContent = `${brandAr} · ${brandEn}`;
    });
    document.querySelectorAll('.footer-brand p').forEach((el) => {
      el.textContent = `${fullName} - ${settings.description || 'شريكك في البرمجة والكمبيوتر والكهرباء والإلكترونيات.'}`;
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
        if ((el.textContent || '').includes('الرياض') || (el.textContent || '').includes('المملكة')) {
          el.textContent = settings.address;
        }
      });
    }
    if (document.title.includes('ودنوح') || document.title.includes('AAMN')) {
      document.title = document.title.replace(/ودنوح\s*AAMN\s*للبرمجيات والكمبيوتر|ودنوح|AAMN/g, fullName);
    }
  }

  const departments = visibleDepartments();
  const servicesGrid = document.querySelector('.services-grid');
  if (servicesGrid) {
    servicesGrid.innerHTML = departments.map((dept, index) => `
      <div class="service-card" data-aos="fade-up" data-delay="${index * 100}">
        <div class="service-icon"><i class="${esc(dept.icon || 'fas fa-circle')}"></i></div>
        <h3>${esc(dept.title)}</h3>
        <p>${esc(dept.description)}</p>
        <a href="services.html" class="service-link">اقرأ المزيد <i class="fas fa-arrow-left"></i></a>
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
          <a href="contact.html" class="btn-primary">طلب عرض سعر <i class="fas fa-arrow-left"></i></a>
        </div>
      </div>
    `).join('');
  }

  const serviceSelect = document.querySelector('select[name="service"]');
  if (serviceSelect) {
    serviceSelect.innerHTML = '<option value="">اختر الخدمة المطلوبة</option>' + departments.map((dept) => `<option value="${esc(dept.key)}">${esc(dept.title)}</option>`).join('') + '<option value="other">أخرى</option>';
  }

  document.querySelectorAll('.footer-services ul').forEach((list) => {
    list.innerHTML = departments.map((dept) => `<li><a href="services.html"><i class="fas fa-angle-left"></i> ${esc(dept.title)}</a></li>`).join('');
  });
})();
