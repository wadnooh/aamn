  const KEYS = {
    trips: 'aletihad_platform_trips_v1',
    bookings: 'aletihad_platform_bookings_v1',
    operators: 'aletihad_platform_operators_v1',
    buses: 'aletihad_platform_buses_v1',
    ops: 'aamn_bus_operations_v2',
    accounts: 'aletihad_accounts_registry_v1'
  };

  const cities = [
    'الخرطوم', 'أم درمان', 'بحري', 'مدني', 'سنار', 'كوستي', 'ربك', 'القضارف',
    'كسلا', 'بورتسودان', 'عطبرة', 'شندي', 'دنقلا', 'كريمة', 'الأبيض', 'النهود',
    'نيالا', 'الفاشر', 'الجنينة', 'الدمازين'
  ];

  const defaultAccounts = [
    {
      id: 'acc_opr_east',
      email: 'east@transport.sd',
      phone: '0911000200',
      fullName: 'اتحاد الشرق للنقل',
      role: 'company',
      tier: 'شركة نقل مميزة',
      accountKind: 'company',
      operatorId: 'opr_east',
      status: 'verified'
    },
    {
      id: 'acc_opr_nile',
      email: 'nile@transport.sd',
      phone: '0998765432',
      fullName: 'شركة نقل النيل',
      role: 'company',
      tier: 'شركة / مجموعة بصات',
      accountKind: 'company',
      operatorId: 'opr_nile',
      status: 'verified'
    },
    {
      id: 'acc_opr_independent',
      email: 'owner@bus.sd',
      phone: '0922222222',
      fullName: 'أحمد عثمان (صاحب بص)',
      role: 'operator',
      tier: 'صاحب بص فردي',
      accountKind: 'operator',
      operatorId: 'opr_independent',
      status: 'verified'
    },
    {
      id: 'acc_passenger_demo',
      email: 'passenger@travel.sd',
      phone: '0912345678',
      fullName: 'عمر خالد الصادق',
      role: 'passenger',
      tier: 'مستخدم / راكب',
      accountKind: 'customer',
      status: 'verified'
    }
  ];

  const defaultTrips = [
    { id: 'trip_krt_madani_0630', operator: 'شركة نقل النيل', bus: 'بص مكيف 49 مقعد', from: 'الخرطوم', to: 'مدني', date: '', time: '6:30 صباحاً', price: 8500, seats: 49, bookedSeats: [3, 7, 12, 18], status: 'published', service: 'مكيف', policy: 'استرداد قبل 6 ساعات' },
    { id: 'trip_krt_portsudan_2000', operator: 'اتحاد الشرق للنقل', bus: 'بص سفر مميز 45 مقعد', from: 'الخرطوم', to: 'بورتسودان', date: '', time: '8:00 مساءً', price: 28000, seats: 45, bookedSeats: [1, 2, 11, 20, 21], status: 'published', service: 'مميز', policy: 'استرداد قبل 12 ساعة' },
    { id: 'trip_krt_kassala_0700', operator: 'شركة كسلا للسفريات', bus: 'بص سياحي 47 مقعد', from: 'الخرطوم', to: 'كسلا', date: '', time: '7:00 صباحاً', price: 18000, seats: 47, bookedSeats: [5, 6, 9], status: 'published', service: 'سياحي', policy: 'تعديل الموعد حسب توفر المقاعد' },
    { id: 'trip_krt_obayid_1500', operator: 'صاحب بص مستقل', bus: 'بص 33 مقعد', from: 'الخرطوم', to: 'الأبيض', date: '', time: '3:00 عصراً', price: 22000, seats: 33, bookedSeats: [4, 14], status: 'published', service: 'عادي', policy: 'تأكيد نهائي بعد السداد' },
    { id: 'trip_krt_dongola_1900', operator: 'شمال السودان للنقل', bus: 'بص مكيف 49 مقعد', from: 'الخرطوم', to: 'دنقلا', date: '', time: '7:00 مساءً', price: 24000, seats: 49, bookedSeats: [8, 16, 22], status: 'published', service: 'مكيف', policy: 'استرداد منظم عند الإلغاء' },
    { id: 'trip_krt_nyala_1700', operator: 'دارفور للسفريات', bus: 'بص سفر طويل 45 مقعد', from: 'الخرطوم', to: 'نيالا', date: '', time: '5:00 مساءً', price: 36000, seats: 45, bookedSeats: [10, 17, 31], status: 'published', service: 'رحلة طويلة', policy: 'تأكيد قبل التحرك' }
  ];

  const defaultOperators = [
    { id: 'opr_nile', name: 'شركة نقل النيل', type: 'company', phone: '0998765432', subscription: 'باقة شركات شهرية', status: 'verified' },
    { id: 'opr_east', name: 'اتحاد الشرق للنقل', type: 'company', phone: '0911000200', subscription: 'باقة مميزة', status: 'verified' },
    { id: 'opr_independent', name: 'صاحب بص مستقل', type: 'individual', phone: '0922222222', subscription: 'باقة صاحب بص', status: 'pending' }
  ];

  const read = (key, fallback) => {
    try {
      const value = JSON.parse(localStorage.getItem(key) || 'null');
      return Array.isArray(fallback) ? (Array.isArray(value) ? value : fallback) : (value || fallback);
    } catch {
      return fallback;
    }
  };

  const write = (key, value) => {
    localStorage.setItem(key, JSON.stringify(value));
    window.dispatchEvent(new CustomEvent('aletihad:data-changed', { detail: { key } }));
  };

  const today = () => new Date().toISOString().slice(0, 10);
  const id = (prefix) => `${prefix}_${Date.now().toString(36)}${Math.random().toString(36).slice(2, 7)}`;
  const money = (value) => `${Number(value || 0).toLocaleString('ar-SA')} جنيه`;
  const availableSeats = (trip) => Math.max(0, Number(trip.seats || 0) - (trip.bookedSeats || []).length);

  function trips() {
    const list = read(KEYS.trips, defaultTrips);
    return list.map((trip) => ({ ...trip, date: trip.date || today(), bookedSeats: Array.isArray(trip.bookedSeats) ? trip.bookedSeats : [] }));
  }

  function saveTrips(list) {
    write(KEYS.trips, list);
  }

  function bookings() {
    return read(KEYS.bookings, []);
  }

  function saveBookings(list) {
    write(KEYS.bookings, list);
  }

  function operators() {
    return read(KEYS.operators, defaultOperators);
  }

  function saveOperators(list) {
    write(KEYS.operators, list);
  }

  function buses() {
    return read(KEYS.buses, []);
  }

  function saveBuses(list) {
    write(KEYS.buses, list);
  }

  function operations() {
    return read(KEYS.ops, []);
  }

  function saveOperations(list) {
    write(KEYS.ops, list);
  }

  function searchTrips({ from = '', to = '', date = '', passengers = 1 } = {}) {
    const needed = Number(passengers || 1);
    return trips().filter((trip) => {
      const sameFrom = !from || trip.from === from;
      const sameTo = !to || trip.to === to;
      const sameDate = !date || !trip.date || trip.date === date || trip.date === today();
      return trip.status === 'published' && sameFrom && sameTo && sameDate && availableSeats(trip) >= needed;
    });
  }

  function addTrip(data) {
    const next = {
      id: id('trip'),
      operator: data.operator || 'صاحب بص',
      bus: data.bus || 'بص سفر',
      from: data.from,
      to: data.to,
      date: data.date || today(),
      time: data.time || '8:00 صباحاً',
      price: Number(data.price || 0),
      seats: Number(data.seats || 45),
      bookedSeats: [],
      status: data.status || 'published',
      service: data.service || 'عادي',
      policy: data.policy || 'حسب سياسة الإلغاء والاسترداد'
    };
    const list = trips();
    list.unshift(next);
    saveTrips(list);
    return next;
  }

  function createBooking(data) {
    const list = trips();
    const trip = list.find((item) => item.id === data.tripId);
    if (!trip) throw new Error('trip_not_found');
    const passengers = Math.max(1, Number(data.passengers || 1));
    if (availableSeats(trip) < passengers) throw new Error('no_seats');

    const taken = new Set(trip.bookedSeats || []);
    const seats = [];
    for (let seat = 1; seat <= Number(trip.seats || 0) && seats.length < passengers; seat += 1) {
      if (!taken.has(seat)) seats.push(seat);
    }
    trip.bookedSeats = [...(trip.bookedSeats || []), ...seats];
    saveTrips(list);

    const randomPart = Math.floor(100000 + Math.random() * 900000);
    const sadadCode = `SADAD-${new Date().getFullYear()}-${randomPart}`;
    const billNumber = `${(new Date().getMonth() + 1).toString().padStart(2, '0')}${new Date().getDate().toString().padStart(2, '0')}${randomPart}`;

    const amount = Number(trip.price || 0) * passengers;
    const booking = {
      id: id('ticket'),
      tripId: trip.id,
      date: today(),
      travelDate: trip.date || today(),
      customer: data.customer || 'عميل',
      email: data.email || '',
      phone: data.phone || '',
      from: trip.from,
      to: trip.to,
      time: trip.time,
      operator: trip.operator,
      bus: trip.bus,
      passengers,
      seats,
      amount,
      sadadCode,
      billNumber,
      paymentStatus: 'pending',
      status: 'open',
      qr: `ETIHAD-${Date.now().toString(36).toUpperCase()}`
    };
    const bookingList = bookings();
    bookingList.unshift(booking);
    saveBookings(bookingList);

    const ops = operations();
    ops.unshift({
      id: booking.id,
      date: booking.date,
      type: 'sale',
      accountKind: 'customer',
      customer: booking.customer,
      phone: booking.phone,
      email: booking.email,
      service: `${booking.from} إلى ${booking.to} - ${booking.passengers} راكب`,
      amount: booking.amount,
      sadadCode: booking.sadadCode,
      billNumber: booking.billNumber,
      status: 'open',
      paymentStatus: 'pending',
      paymentRef: booking.sadadCode,
      notes: `المقاعد: ${booking.seats.join(', ')} - ${booking.operator} | رقم السداد: ${sadadCode}`
    });
    saveOperations(ops);
    return booking;
  }

  function updatePayment(ticketId, payment) {
    const bookingList = bookings();
    const booking = bookingList.find((item) => item.id === ticketId);
    if (booking) {
      booking.paymentStatus = 'review';
      booking.paymentMethod = payment.method || '';
      booking.paymentReference = payment.reference || '';
      booking.paymentNote = payment.note || '';
      booking.paymentSubmittedAt = new Date().toISOString();
      saveBookings(bookingList);
    }

    const ops = operations();
    const op = ops.find((item) => item.id === ticketId);
    if (op) {
      op.paymentStatus = 'review';
      op.paymentMethod = payment.method || '';
      op.paymentRef = payment.reference || op.paymentRef || '';
      op.notes = [op.notes, payment.note ? `ملاحظة السداد: ${payment.note}` : '', payment.method ? `طريقة السداد: ${payment.method}` : ''].filter(Boolean).join(' | ');
      saveOperations(ops);
    }
    return booking || op || null;
  }

  function accounts() {
    return read(KEYS.accounts, defaultAccounts);
  }

  function saveAccounts(list) {
    write(KEYS.accounts, list);
  }

  function findAccount(email) {
    if (!email) return null;
    const clean = email.trim().toLowerCase();
    return accounts().find((a) => (a.email || '').trim().toLowerCase() === clean) || null;
  }

  function registerAccount(data) {
    const list = accounts();
    const clean = (data.email || '').trim().toLowerCase();
    const existing = list.find((a) => (a.email || '').trim().toLowerCase() === clean);
    if (existing) {
      const err = new Error('EMAIL_EXISTS');
      err.code = 'EMAIL_EXISTS';
      err.existingRole = existing.role;
      throw err;
    }

    const isBusiness = data.role === 'company' || data.role === 'operator' || data.accountKind === 'operator' || data.accountKind === 'company';
    const operatorId = isBusiness ? (data.operatorId || id('opr')) : null;

    const newAcc = {
      id: id('acc'),
      fullName: data.fullName || 'مستخدم',
      email: clean,
      phone: data.phone || '',
      role: isBusiness ? (data.role || (data.tier?.includes('شركة') ? 'company' : 'operator')) : 'passenger',
      accountKind: isBusiness ? (data.accountKind || (data.tier?.includes('شركة') ? 'company' : 'operator')) : 'customer',
      tier: data.tier || (isBusiness ? 'صاحب بص فردي' : 'مستخدم / راكب'),
      operatorId: operatorId,
      status: 'verified',
      joinedAt: new Date().toISOString()
    };

    list.unshift(newAcc);
    saveAccounts(list);

    if (isBusiness) {
      const oprList = operators();
      if (!oprList.some((o) => o.id === operatorId)) {
        oprList.unshift({
          id: operatorId,
          name: newAcc.fullName,
          type: newAcc.role === 'company' ? 'company' : 'individual',
          phone: newAcc.phone,
          subscription: newAcc.tier,
          status: 'verified'
        });
        saveOperators(oprList);
      }
    }

    return newAcc;
  }

  function authenticate(email, password, requiredPortal) {
    const acc = findAccount(email);
    if (!acc) {
      return {
        success: false,
        code: 'NOT_FOUND',
        message: 'البريد الإلكتروني غير مسجل مسبقاً. يرجى إنشاء حساب جديد أولاً من تبويب حساب جديد.'
      };
    }

    const isOperatorRole = acc.role === 'operator' || acc.role === 'company' || acc.accountKind === 'operator' || acc.accountKind === 'company';

    if (requiredPortal === 'operator') {
      if (!isOperatorRole) {
        return {
          success: false,
          code: 'FORBIDDEN_PASSENGER',
          userRole: acc.role,
          message: '❌ تنبيه: هذا الحساب مسجل كـ (مستخدم / راكب) في بوابة حجز التذاكر، ولا يملك قاعدة بيانات في تخصص أصحاب البصات والشركات. منعاً لخلط البيانات، يرجى التوجه لبوابة الركاب والمسافرين.',
          redirectUrl: 'client.html'
        };
      }
    } else if (requiredPortal === 'passenger') {
      if (isOperatorRole) {
        return {
          success: true,
          isOperatorBrowsing: true,
          user: acc,
          notice: 'حسابك مسجل كـ (صاحب بص / شركة). يمكنك حجز التذاكر كمسافر، أو التوجه للوحة أصحاب البصات لإدارة أسطولك.'
        };
      }
    }

    return {
      success: true,
      user: acc
    };
  }

  window.AletihadPlatform = {
    KEYS,
    cities,
    defaultTrips,
    defaultOperators,
    defaultAccounts,
    today,
    money,
    availableSeats,
    trips,
    saveTrips,
    bookings,
    saveBookings,
    operators,
    saveOperators,
    buses,
    saveBuses,
    operations,
    saveOperations,
    searchTrips,
    addTrip,
    createBooking,
    updatePayment,
    accounts,
    saveAccounts,
    findAccount,
    registerAccount,
    authenticate
  };
})();
