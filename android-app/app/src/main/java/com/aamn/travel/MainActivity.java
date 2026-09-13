package com.aamn.travel;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Intent;
import android.graphics.Bitmap;
import android.net.Uri;
import android.os.Bundle;
import android.view.View;
import android.webkit.ValueCallback;
import android.webkit.WebChromeClient;
import android.webkit.WebResourceError;
import android.webkit.WebResourceRequest;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.ProgressBar;
import android.widget.Toast;

/**
 * Main Activity for Aamn Travel & Bus Booking Platform
 * Encapsulates the web platform at https://2-aa.com/ with native speed and capabilities.
 */
public class MainActivity extends Activity {

    public static final String APP_URL = "https://2-aa.com/";
    private static final int INPUT_FILE_REQUEST_CODE = 1001;

    private WebView mWebView;
    private ProgressBar mProgressBar;
    private ValueCallback<Uri[]> mFilePathCallback;
    private String mCameraPhotoPath;

    @SuppressLint("SetJavaScriptEnabled")
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        mWebView = findViewById(R.id.webview);
        mProgressBar = findViewById(R.id.progress_bar);

        configureWebView();

        if (savedInstanceState != null) {
            mWebView.restoreState(savedInstanceState);
        } else {
            // Check intent if launched from deep link
            Intent intent = getIntent();
            Uri data = intent != null ? intent.getData() : null;
            if (data != null && data.toString().contains("2-aa.com")) {
                mWebView.loadUrl(data.toString());
            } else {
                mWebView.loadUrl(APP_URL);
            }
        }
    }

    @SuppressLint("SetJavaScriptEnabled")
    private void configureWebView() {
        WebSettings webSettings = mWebView.getSettings();
        webSettings.setJavaScriptEnabled(true);
        webSettings.setDomStorageEnabled(true);
        webSettings.setDatabaseEnabled(true);
        webSettings.setLoadsImagesAutomatically(true);
        webSettings.setMixedContentMode(WebSettings.MIXED_CONTENT_ALWAYS_ALLOW);
        webSettings.setAllowFileAccess(true);
        webSettings.setAllowContentAccess(true);
        webSettings.setSupportZoom(false);
        webSettings.setBuiltInZoomControls(false);
        webSettings.setUseWideViewPort(true);
        webSettings.setLoadWithOverviewMode(true);
        webSettings.setCacheMode(WebSettings.LOAD_DEFAULT);
        webSettings.setUserAgentString(webSettings.getUserAgentString() + " AamnTravelAndroid/1.0");

        // Native JavaScript Bridge for Android Clipboard, Toasts and Sharing
        mWebView.addJavascriptInterface(new WebAppInterface(this), "AndroidBridge");

        // Custom WebViewClient for internal navigation, deep links and external intents
        mWebView.setWebViewClient(new WebViewClient() {
            @Override
            public boolean shouldOverrideUrlLoading(WebView view, WebResourceRequest request) {
                String url = request.getUrl().toString();
                if (url.startsWith("tel:") || url.startsWith("mailto:") || url.startsWith("whatsapp:") || url.startsWith("intent:")) {
                    try {
                        Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(url));
                        startActivity(intent);
                        return true;
                    } catch (Exception e) {
                        Toast.makeText(MainActivity.this, "لا يمكن فتح التطبيق المطلوب", Toast.LENGTH_SHORT).show();
                        return true;
                    }
                }
                return false; // Load inside WebView
            }

            @Override
            public void onPageStarted(WebView view, String url, Bitmap favicon) {
                super.onPageStarted(view, url, favicon);
                if (mProgressBar != null) mProgressBar.setVisibility(View.VISIBLE);
            }

            @Override
            public void onPageFinished(WebView view, String url) {
                super.onPageFinished(view, url);
                if (mProgressBar != null) mProgressBar.setVisibility(View.GONE);
            }

            @Override
            public void onReceivedError(WebView view, WebResourceRequest request, WebResourceError error) {
                super.onReceivedError(view, request, error);
                if (request.isForMainFrame()) {
                    if (mProgressBar != null) mProgressBar.setVisibility(View.GONE);
                    String offlineHtml = "<!DOCTYPE html><html lang='ar' dir='rtl'><head><meta charset='UTF-8'><meta name='viewport' content='width=device-width, initial-scale=1.0'>" +
                            "<style>body{font-family:sans-serif;background:#f0f9ff;color:#0f172a;display:flex;flex-direction:column;align-items:center;justify-content:center;height:90vh;margin:0;padding:20px;text-align:center;}" +
                            "h2{color:#0369a1;margin-bottom:10px;}p{color:#64748b;margin-bottom:24px;line-height:1.6;}" +
                            ".btn{background:#0284c7;color:#fff;border:0;padding:12px 28px;border-radius:10px;font-size:16px;font-weight:bold;cursor:pointer;box-shadow:0 4px 14px rgba(2,132,199,0.3);}</style></head>" +
                            "<body><div style='font-size:54px;margin-bottom:12px;'>🚌</div>" +
                            "<h2>تعذر الاتصال بالشبكة</h2>" +
                            "<p>يرجى التأكد من تشغيل البيانات أو الواي فاي ثم الضغط أدناه لتحديث الصفحة ومتابعة الحجز.</p>" +
                            "<button class='btn' onclick='window.location.href=\"" + APP_URL + "\"'>إعادة المحاولة الآن</button></body></html>";
                    view.loadDataWithBaseURL(APP_URL, offlineHtml, "text/html", "UTF-8", null);
                }
            }
        });

        // WebChromeClient for Progress Bar & File/Camera Uploads (Payment Slips)
        mWebView.setWebChromeClient(new WebChromeClient() {
            @Override
            public void onProgressChanged(WebView view, int newProgress) {
                if (mProgressBar != null) {
                    mProgressBar.setProgress(newProgress);
                    if (newProgress >= 100) {
                        mProgressBar.setVisibility(View.GONE);
                    } else {
                        mProgressBar.setVisibility(View.VISIBLE);
                    }
                }
            }

            @Override
            public boolean onShowFileChooser(WebView webView, ValueCallback<Uri[]> filePathCallback, FileChooserParams fileChooserParams) {
                if (mFilePathCallback != null) {
                    mFilePathCallback.onReceiveValue(null);
                }
                mFilePathCallback = filePathCallback;

                Intent contentSelectionIntent = new Intent(Intent.ACTION_GET_CONTENT);
                contentSelectionIntent.addCategory(Intent.CATEGORY_OPENABLE);
                contentSelectionIntent.setType("image/*");

                Intent chooserIntent = new Intent(Intent.ACTION_CHOOSER);
                chooserIntent.putExtra(Intent.EXTRA_INTENT, contentSelectionIntent);
                chooserIntent.putExtra(Intent.EXTRA_TITLE, "إرفاق إشعار الدفع / صورة");

                try {
                    startActivityForResult(chooserIntent, INPUT_FILE_REQUEST_CODE);
                } catch (Exception e) {
                    mFilePathCallback = null;
                    Toast.makeText(MainActivity.this, "تعذر فتح منتقي الملفات", Toast.LENGTH_SHORT).show();
                    return false;
                }
                return true;
            }
        });
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode == INPUT_FILE_REQUEST_CODE) {
            if (mFilePathCallback == null) {
                super.onActivityResult(requestCode, resultCode, data);
                return;
            }
            Uri[] results = null;
            if (resultCode == Activity.RESULT_OK && data != null) {
                String dataString = data.getDataString();
                if (dataString != null) {
                    results = new Uri[]{Uri.parse(dataString)};
                }
            }
            mFilePathCallback.onReceiveValue(results);
            mFilePathCallback = null;
        } else {
            super.onActivityResult(requestCode, resultCode, data);
        }
    }

    @Override
    public void onBackPressed() {
        if (mWebView != null && mWebView.canGoBack()) {
            mWebView.goBack();
        } else {
            super.onBackPressed();
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        if (mWebView != null) {
            mWebView.onResume();
        }
    }

    @Override
    protected void onPause() {
        if (mWebView != null) {
            mWebView.onPause();
        }
        super.onPause();
    }
}
