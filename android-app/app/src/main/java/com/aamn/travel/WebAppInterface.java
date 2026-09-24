package com.aamn.travel;

import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.Context;
import android.content.Intent;
import android.net.Uri;
import android.webkit.JavascriptInterface;
import android.widget.Toast;

/**
 * JavaScript interface bridging Web functionality with Native Android capabilities
 */
public class WebAppInterface {
    private final Context mContext;

    public WebAppInterface(Context context) {
        this.mContext = context;
    }

    /**
     * Check if running inside native Android WebView container
     */
    @JavascriptInterface
    public boolean isNativeApp() {
        return true;
    }

    /**
     * Get current application version
     */
    @JavascriptInterface
    public String getAppVersion() {
        return "1.3.0";
    }

    /**
     * Copy text (e.g. Sadad Reference or Bank Account Number) to Android clipboard
     */
    @JavascriptInterface
    public void copyToClipboard(String text, String label) {
        ClipboardManager clipboard = (ClipboardManager) mContext.getSystemService(Context.CLIPBOARD_SERVICE);
        ClipData clip = ClipData.newPlainText(label != null ? label : "Aamn Booking", text);
        if (clipboard != null) {
            clipboard.setPrimaryClip(clip);
            Toast.makeText(mContext, "تم نسخ " + (label != null ? label : "النص") + " بنجاح", Toast.LENGTH_SHORT).show();
        }
    }

    /**
     * Native share sheet for booking voucher or payment code
     */
    @JavascriptInterface
    public void shareText(String title, String content) {
        try {
            Intent sendIntent = new Intent();
            sendIntent.setAction(Intent.ACTION_SEND);
            sendIntent.putExtra(Intent.EXTRA_TEXT, content);
            sendIntent.putExtra(Intent.EXTRA_SUBJECT, title);
            sendIntent.setType("text/plain");

            Intent shareIntent = Intent.createChooser(sendIntent, title);
            shareIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
            mContext.startActivity(shareIntent);
        } catch (Exception e) {
            showToast("تعذر مشاركة النص");
        }
    }

    /**
     * Show native Android Toast notification
     */
    @JavascriptInterface
    public void showToast(String message) {
        Toast.makeText(mContext, message, Toast.LENGTH_SHORT).show();
    }

    /**
     * Native haptic feedback vibration
     */
    @JavascriptInterface
    public void vibrate(long milliseconds) {
        try {
            android.os.Vibrator v = (android.os.Vibrator) mContext.getSystemService(Context.VIBRATOR_SERVICE);
            if (v != null && v.hasVibrator()) {
                v.vibrate(milliseconds > 0 ? milliseconds : 35);
            }
        } catch (Exception ignored) {}
    }

    /**
     * Open WhatsApp directly with phone number and optional message
     */
    @JavascriptInterface
    public void openWhatsApp(String phone, String message) {
        try {
            String cleanPhone = phone != null ? phone.replaceAll("[^0-9+]", "") : "";
            String url = "https://api.whatsapp.com/send?phone=" + cleanPhone;
            if (message != null && !message.isEmpty()) {
                url += "&text=" + Uri.encode(message);
            }
            Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(url));
            intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
            mContext.startActivity(intent);
        } catch (Exception e) {
            showToast("تعذر فتح تطبيق واتساب");
        }
    }

    /**
     * Open location in Google Maps directly
     */
    @JavascriptInterface
    public void openGoogleMaps(String locationQuery) {
        try {
            String uriStr = "geo:0,0?q=" + Uri.encode(locationQuery != null ? locationQuery : "السودان - ميناء البري");
            Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(uriStr));
            intent.setPackage("com.google.android.apps.maps");
            intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
            mContext.startActivity(intent);
        } catch (Exception e) {
            openExternalUrl("https://maps.google.com/?q=" + Uri.encode(locationQuery));
        }
    }

    /**
     * Open external web URL or external activity intent
     */
    @JavascriptInterface
    public void openExternalUrl(String url) {
        try {
            if (url != null && !url.isEmpty()) {
                Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(url));
                intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                mContext.startActivity(intent);
            }
        } catch (Exception e) {
            showToast("لا يمكن فتح الرابط المطلوب");
        }
    }
}
