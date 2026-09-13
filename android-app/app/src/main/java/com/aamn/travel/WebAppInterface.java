package com.aamn.travel;

import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.Context;
import android.content.Intent;
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
        Intent sendIntent = new Intent();
        sendIntent.setAction(Intent.ACTION_SEND);
        sendIntent.putExtra(Intent.EXTRA_TEXT, content);
        sendIntent.putExtra(Intent.EXTRA_SUBJECT, title);
        sendIntent.setType("text/plain");

        Intent shareIntent = Intent.createChooser(sendIntent, title);
        shareIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        mContext.startActivity(shareIntent);
    }

    /**
     * Show native Android Toast notification
     */
    @JavascriptInterface
    public void showToast(String message) {
        Toast.makeText(mContext, message, Toast.LENGTH_SHORT).show();
    }
}
