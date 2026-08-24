(function () {
  const key = 'aamn_site_settings';
  let settings = null;
  try {
    settings = JSON.parse(localStorage.getItem(key) || 'null');
  } catch {
    settings = null;
  }
  if (!settings) return;

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
})();
