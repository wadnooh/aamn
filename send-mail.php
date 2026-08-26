<?php
header('Content-Type: application/json; charset=utf-8');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: POST, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type');

if (['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(200);
    exit;
}

if (['REQUEST_METHOD'] !== 'POST') {
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit;
}

 = file_get_contents('php://input');
 = json_decode(, true);

if (!) {
     = ;
}

 = isset(['name']) ? trim(['name']) : '';
 = isset(['email']) ? trim(['email']) : '';
 = isset(['phone']) ? trim(['phone']) : '';
 = isset(['service']) ? trim(['service']) : 'استفسار عام';
 = isset(['message']) ? trim(['message']) : '';

if (empty() || empty() || empty()) {
    echo json_encode(['success' => false, 'error' => 'يرجى تعبئة جميع الحقول المطلوبة']);
    exit;
}

 = 'info@wadnooh.tech';
 = رسالة جديدة من الموقع:  .  .  ( .  . );

 = تم استلام رسالة جديدة من موقع ودنوح AAMN:\n\n;
 .= الاسم:  .  . \n;
 .= البريد الإلكتروني:  .  . \n;
 .= رقم الجوال:  .  . \n;
 .= الخدمة المطلوبة:  .  . \n;
 .= التاريخ:  . date('Y-m-d H:i:s') . \n\n;
 .= نص الرسالة:\n .  . \n;

 = From: noreply@wadnooh.com\r\n;
 .= Reply-To:  .  . \r\n;
 .= Content-Type: text/plain; charset=UTF-8\r\n;

 = @mail(, , , );

echo json_encode([
    'success' => true,
    'message' => 'تم استلام وتوثيق رسالتك بنجاح',
    'mail_sent' => ,
    'timestamp' => date('c')
]);