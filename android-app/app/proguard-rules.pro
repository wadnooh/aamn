# ProGuard rules for Aamn Bus Booking App
-keepattributes JavascriptInterface
-keepclassmembers class * {
    @android.webkit.JavascriptInterface <methods>;
}

# Keep models and webview interfaces
-keep class com.aamn.travel.WebAppInterface { *; }
-keepclassmembers class com.aamn.travel.WebAppInterface { *; }

# Keep AndroidX & Material
-keep class androidx.appcompat.** { *; }
-keep class com.google.android.material.** { *; }
