(function () {
  const KEYS = {
    trips: 'aletihad_platform_trips_v1',
    bookings: 'aletihad_platform_bookings_v1',
    operators: 'aletihad_platform_operators_v1',
    buses: 'aletihad_platform_buses_v1',
    ops: 'aamn_bus_operations_v2'
  };

  const cities = [
    'الخرطوم', 'أم درمان', 'بحري', 'مدني', 'سنار', 'كوستي', 'ربك', 'القضارف',
    'كسلا', 'بورتسودان', 'عطبرة', 'شندي', 'دنقلا', 'كريمة', 'الأبيض', 'النهود',
    'نيالا', 'الفاشر', 'الجنينة', 'الدمازين'
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
      status: 'open',
      paymentStatus: 'pending',
      paymentRef: booking.qr,
      notes: `المقاعد: ${booking.seats.join(', ')} - ${booking.operator}`
    });
    saveOperations(ops);
    return booking;
  }

  window.AletihadPlatform = {
    KEYS,
    cities,
    defaultTrips,
    defaultOperators,
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
    createBooking
  };
})();
