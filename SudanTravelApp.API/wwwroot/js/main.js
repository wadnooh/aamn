/**
 * AAMN - ودنوح AAMN للبرمجيات والكمبيوتر
 * Main JavaScript File
 */

document.addEventListener('DOMContentLoaded', () => {

    /* ============================================
       1. PRELOADER
    ============================================ */
    const preloader = document.getElementById('preloader');
    if (preloader) {
        setTimeout(() => {
            preloader.classList.add('hidden');
        }, 1600);
    }

    /* ============================================
       2. HEADER SCROLL EFFECT
    ============================================ */
    const header = document.getElementById('header');
    const handleScroll = () => {
        if (window.scrollY > 80) {
            header.classList.add('scrolled');
        } else {
            header.classList.remove('scrolled');
        }
    };
    window.addEventListener('scroll', handleScroll, { passive: true });
    handleScroll();

    /* ============================================
       3. MOBILE MENU
    ============================================ */
    const hamburger = document.getElementById('hamburger');
    const mobileMenu = document.getElementById('mobileMenu');
    const closeMenu = document.getElementById('closeMenu');
    const overlay = document.getElementById('overlay');

    const openMobileMenu = () => {
        mobileMenu.classList.add('open');
        overlay.classList.add('active');
        document.body.style.overflow = 'hidden';
    };

    const closeMobileMenu = () => {
        mobileMenu.classList.remove('open');
        overlay.classList.remove('active');
        document.body.style.overflow = '';
    };

    if (hamburger) hamburger.addEventListener('click', openMobileMenu);
    if (closeMenu) closeMenu.addEventListener('click', closeMobileMenu);
    if (overlay) overlay.addEventListener('click', closeMobileMenu);

    // Close mobile menu on link click
    const mobileLinks = document.querySelectorAll('.mobile-menu a');
    mobileLinks.forEach(link => link.addEventListener('click', closeMobileMenu));

    /* ============================================
       4. HERO PARTICLES
    ============================================ */
    const particlesContainer = document.getElementById('particles');
    if (particlesContainer) {
        const createParticle = () => {
            const particle = document.createElement('div');
            particle.className = 'particle';
            const size = Math.random() * 4 + 2;
            const left = Math.random() * 100;
            const duration = Math.random() * 8 + 6;
            const delay = Math.random() * 8;

            particle.style.cssText = `
                width: ${size}px;
                height: ${size}px;
                left: ${left}%;
                animation-duration: ${duration}s;
                animation-delay: ${delay}s;
                opacity: ${Math.random() * 0.5 + 0.2};
            `;
            particlesContainer.appendChild(particle);
        };

        for (let i = 0; i < 20; i++) {
            createParticle();
        }
    }

    /* ============================================
       5. COUNTER ANIMATION
    ============================================ */
    const animateCounter = (el, target, duration = 2000) => {
        let start = 0;
        const increment = target / (duration / 16);
        const timer = setInterval(() => {
            start += increment;
            if (start >= target) {
                el.textContent = target;
                clearInterval(timer);
            } else {
                el.textContent = Math.floor(start);
            }
        }, 16);
    };

    const counterObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const counters = entry.target.querySelectorAll('[data-target]');
                counters.forEach(counter => {
                    const target = parseInt(counter.getAttribute('data-target'));
                    animateCounter(counter, target);
                });
                counterObserver.unobserve(entry.target);
            }
        });
    }, { threshold: 0.3 });

    const heroStats = document.querySelector('.hero-stats');
    const statsBanner = document.querySelector('.stats-banner');
    if (heroStats) counterObserver.observe(heroStats);
    if (statsBanner) counterObserver.observe(statsBanner);

    /* ============================================
       6. SCROLL REVEAL ANIMATIONS
    ============================================ */
    const revealElements = document.querySelectorAll('.service-card, .project-card, .why-item, .feat-item, .stat-card, .testimonial-card, .about-grid, .section-header');

    revealElements.forEach(el => el.classList.add('reveal'));

    const revealObserver = new IntersectionObserver((entries) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                setTimeout(() => {
                    entry.target.classList.add('visible');
                }, index * 80);
                revealObserver.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1, rootMargin: '0px 0px -50px 0px' });

    revealElements.forEach(el => revealObserver.observe(el));

    /* ============================================
       7. PROJECTS FILTER
    ============================================ */
    const filterBtns = document.querySelectorAll('.filter-btn');
    const projectCards = document.querySelectorAll('.project-card');

    filterBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            const filter = btn.dataset.filter;

            filterBtns.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');

            projectCards.forEach(card => {
                if (filter === 'all' || card.dataset.category === filter) {
                    card.classList.remove('hidden');
                    card.style.animation = 'fadeIn 0.4s ease both';
                } else {
                    card.classList.add('hidden');
                }
            });
        });
    });

    /* ============================================
       8. TESTIMONIALS SLIDER
    ============================================ */
    const track = document.getElementById('testimonialTrack');
    const dotsContainer = document.getElementById('sliderDots');
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');

    if (track) {
        const cards = track.querySelectorAll('.testimonial-card');
        const totalSlides = cards.length;
        let currentSlide = 0;
        let slidesPerView = window.innerWidth <= 768 ? 1 : 2;
        let maxSlide = totalSlides - slidesPerView;

        // Create dots
        if (dotsContainer) {
            for (let i = 0; i <= maxSlide; i++) {
                const dot = document.createElement('div');
                dot.className = `dot ${i === 0 ? 'active' : ''}`;
                dot.addEventListener('click', () => goToSlide(i));
                dotsContainer.appendChild(dot);
            }
        }

        const updateDots = () => {
            const dots = dotsContainer?.querySelectorAll('.dot');
            dots?.forEach((dot, i) => {
                dot.classList.toggle('active', i === currentSlide);
            });
        };

        const goToSlide = (index) => {
            currentSlide = Math.max(0, Math.min(index, maxSlide));
            const cardWidth = cards[0].offsetWidth + 28;
            track.style.transform = `translateX(${currentSlide * cardWidth}px)`;
            updateDots();
        };

        if (prevBtn) prevBtn.addEventListener('click', () => goToSlide(currentSlide + 1));
        if (nextBtn) nextBtn.addEventListener('click', () => goToSlide(currentSlide - 1));

        // Auto-advance
        let autoPlay = setInterval(() => {
            const next = currentSlide >= maxSlide ? 0 : currentSlide + 1;
            goToSlide(next);
        }, 4000);

        track.addEventListener('mouseenter', () => clearInterval(autoPlay));
        track.addEventListener('mouseleave', () => {
            autoPlay = setInterval(() => {
                const next = currentSlide >= maxSlide ? 0 : currentSlide + 1;
                goToSlide(next);
            }, 4000);
        });

        // Responsive update
        window.addEventListener('resize', () => {
            slidesPerView = window.innerWidth <= 768 ? 1 : 2;
            maxSlide = totalSlides - slidesPerView;
            currentSlide = 0;
            goToSlide(0);
        });
    }

    /* ============================================
       9. BACK TO TOP BUTTON
    ============================================ */
    const backToTop = document.getElementById('backToTop');
    if (backToTop) {
        window.addEventListener('scroll', () => {
            if (window.scrollY > 400) {
                backToTop.classList.add('visible');
            } else {
                backToTop.classList.remove('visible');
            }
        }, { passive: true });

        backToTop.addEventListener('click', () => {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    }

    /* ============================================
       10. ACTIVE NAV LINK ON SCROLL
    ============================================ */
    const sections = document.querySelectorAll('section[id]');
    const navLinks = document.querySelectorAll('.nav-links a');

    window.addEventListener('scroll', () => {
        let current = '';
        sections.forEach(section => {
            const sectionTop = section.offsetTop - 120;
            if (window.scrollY >= sectionTop) {
                current = section.getAttribute('id');
            }
        });

        navLinks.forEach(link => {
            link.classList.remove('active');
            if (link.getAttribute('href') === `#${current}`) {
                link.classList.add('active');
            }
        });
    }, { passive: true });

    /* ============================================
       11. FORM VALIDATION (Contact Page)
    ============================================ */
    const contactForm = document.getElementById('contactForm');
    if (contactForm) {
        contactForm.addEventListener('submit', async (e) => {
            e.preventDefault();

            const name = document.getElementById('name')?.value.trim();
            const email = document.getElementById('email')?.value.trim();
            const phone = document.getElementById('phone')?.value.trim();
            const serviceEl = document.getElementById('service');
            const service = serviceEl?.selectedOptions?.[0]?.textContent?.trim() || serviceEl?.value || '';
            const message = document.getElementById('message')?.value.trim();
            const submitBtn = contactForm.querySelector('.btn-submit');

            // Validation
            if (!name || !email || !message) {
                showNotification('يرجى ملء جميع الحقول المطلوبة', 'error');
                return;
            }

            if (!isValidEmail(email)) {
                showNotification('يرجى إدخال بريد إلكتروني صحيح', 'error');
                return;
            }

            // Show loading
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> جاري الإرسال...';
            }

            const payload = {
                name,
                email,
                phone: phone || '',
                service,
                message,
                status: 'new',
                submitted_at: new Date().toISOString()
            };

            const saveLocalInquiry = () => {
                try {
                    const key = 'aamn_operations';
                    const current = JSON.parse(localStorage.getItem(key) || '[]');
                    current.unshift({
                        id: `op_${Date.now().toString(36)}${Math.random().toString(36).slice(2, 8)}`,
                        type: 'consultation',
                        status: 'open',
                        date: new Date().toISOString().slice(0, 10),
                        customer: name,
                        phone: phone || '',
                        email,
                        service: service || 'رسالة تواصل',
                        amount: 0,
                        followUp: '',
                        notes: message
                    });
                    localStorage.setItem(key, JSON.stringify(current));
                } catch {
                    // Local backup is best-effort only.
                }
            };

            const openDirectChannel = () => {
                const settings = (() => {
                    try { return JSON.parse(localStorage.getItem('aamn_site_settings') || '{}'); } catch { return {}; }
                })();
                const rawWhatsApp = String(settings.whatsapp || '966500000000').replace(/[^\d]/g, '');
                const text = [
                    'رسالة جديدة من موقع ودنوح AAMN',
                    `الاسم: ${name}`,
                    `البريد: ${email}`,
                    `الجوال: ${phone || 'غير مذكور'}`,
                    `الخدمة: ${service || 'غير محددة'}`,
                    `الرسالة: ${message}`
                ].join('\n');
                if (rawWhatsApp) {
                    window.open(`https://wa.me/${rawWhatsApp}?text=${encodeURIComponent(text)}`, '_blank', 'noopener,noreferrer');
                    return;
                }
                window.location.href = `mailto:${settings.email || 'info@wadnooh.tech'}?subject=${encodeURIComponent('رسالة من موقع ودنوح AAMN')}&body=${encodeURIComponent(text)}`;
            };

            try {
                const response = await fetch('/api/contact', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });

                if (response.ok) {
                    saveLocalInquiry();
                    showNotification('تم إرسال رسالتك بنجاح! سنتواصل معك قريباً.', 'success');
                    contactForm.reset();
                } else {
                    throw new Error('Server error');
                }
            } catch (err) {
                saveLocalInquiry();
                openDirectChannel();
                showNotification('تم تجهيز رسالتك وفتح واتساب لإرسالها مباشرة.', 'success');
                contactForm.reset();
            } finally {
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = '<i class="fas fa-paper-plane"></i> إرسال الرسالة';
                }
            }
        });
    }

    /* ============================================
       12. HELPERS
    ============================================ */
    const isValidEmail = (email) => {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    };

    const showNotification = (message, type = 'success') => {
        const existing = document.querySelector('.notification');
        if (existing) existing.remove();

        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.innerHTML = `
            <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'}"></i>
            <span>${message}</span>
            <button onclick="this.parentElement.remove()"><i class="fas fa-times"></i></button>
        `;

        Object.assign(notification.style, {
            position: 'fixed',
            bottom: '30px',
            right: '30px',
            background: type === 'success' ? '#1a7a4a' : '#c0392b',
            color: '#fff',
            padding: '16px 24px',
            borderRadius: '12px',
            display: 'flex',
            alignItems: 'center',
            gap: '12px',
            boxShadow: '0 8px 30px rgba(0,0,0,0.2)',
            zIndex: '9999',
            fontSize: '15px',
            fontFamily: 'Cairo, sans-serif',
            animation: 'fadeInUp 0.4s ease',
            maxWidth: '380px'
        });

        document.body.appendChild(notification);
        setTimeout(() => {
            if (notification.parentElement) notification.remove();
        }, 5000);
    };

    /* ============================================
       13. SMOOTH ANCHOR SCROLLING
    ============================================ */
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', (e) => {
            const target = document.querySelector(anchor.getAttribute('href'));
            if (target) {
                e.preventDefault();
                const offset = 80;
                const top = target.offsetTop - offset;
                window.scrollTo({ top, behavior: 'smooth' });
            }
        });
    });

    /* ============================================
       14. WHATSAPP FLOAT BUTTON
    ============================================ */
    const waBtn = document.createElement('a');
    waBtn.href = 'https://wa.me/966500000000';
    waBtn.target = '_blank';
    waBtn.rel = 'noopener noreferrer';
    waBtn.className = 'whatsapp-float';
    waBtn.innerHTML = '<i class="fab fa-whatsapp"></i>';
    waBtn.setAttribute('aria-label', 'تواصل عبر واتساب');

    Object.assign(waBtn.style, {
        position: 'fixed',
        bottom: '90px',
        right: '30px',
        width: '52px',
        height: '52px',
        background: '#25D366',
        color: '#fff',
        borderRadius: '50%',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        fontSize: '26px',
        zIndex: '500',
        boxShadow: '0 4px 20px rgba(37,211,102,0.4)',
        transition: 'all 0.3s ease',
        animation: 'pulse 2.5s ease infinite'
    });

    waBtn.addEventListener('mouseenter', () => {
        waBtn.style.transform = 'scale(1.1)';
    });
    waBtn.addEventListener('mouseleave', () => {
        waBtn.style.transform = 'scale(1)';
    });

    document.body.appendChild(waBtn);

    console.log('✅ AAMN Website initialized successfully');
});
