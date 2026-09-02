<?php
header('Content-Type: application/json; charset=utf-8');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: POST, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type');

if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(200);
    exit;
}

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit;
}

$raw = file_get_contents('php://input');
$data = json_decode($raw, true);

if (!$data) {
    $data = $_POST;
}

$name = isset($data['name']) ? trim($data['name']) : '';
$email = isset($data['email']) ? trim($data['email']) : '';
$phone = isset($data['phone']) ? trim($data['phone']) : '';
$service = isset($data['service']) ? trim($data['service']) : 'استفسار عام';
$message = isset($data['message']) ? trim($data['message']) : '';

if (empty($name) || empty($email) || empty($message)) {
    echo json_encode(['success' => false, 'error' => 'يرجى تعبئة جميع الحقول المطلوبة']);
    exit;
}

$to = 'info@2-aa.com';
$subject = "رسالة جديدة من الموقع: " . $name . " (" . $service . ")";

$body = "تم استلام رسالة جديدة من موقع 2-aa.com:\n\n";
$body .= "الاسم: " . $name . "\n";
$body .= "البريد الإلكتروني: " . $email . "\n";
$body .= "رقم الجوال: " . $phone . "\n";
$body .= "الخدمة المطلوبة: " . $service . "\n";
$body .= "التاريخ: " . date('Y-m-d H:i:s') . "\n\n";
$body .= "نص الرسالة:\n" . $message . "\n";

$headers = "From: noreply@2-aa.com\r\n";
$headers .= "Reply-To: " . $email . "\r\n";
$headers .= "Content-Type: text/plain; charset=UTF-8\r\n";

$mail_sent = @mail($to, $subject, $body, $headers);

echo json_encode([
    'success' => true,
    'message' => 'تم استلام وتوثيق رسالتك بنجاح',
    'mail_sent' => $mail_sent,
    'timestamp' => date('c')
]);