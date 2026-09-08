/* =========================================================
   WADNOOH AAMN - Interactive Code Studio Engine
   Languages: Python 3.12 (Core), JS, C#, PHP, C++, SQL
   ========================================================= */

(function () {
    const snippets = {
        python: "# ==========================================\n# WADNOOH AAMN - PYTHON CORE ENGINE v3.12\n# حلول الأتمتة والذكاء الاصطناعي المتطورة\n# ==========================================\n\nimport math\nfrom datetime import datetime\n\nclass WadNoohAAMN:\n    def __init__(self, client_name=\"شركة التقنية المتميزة\"):\n        self.client = client_name\n        self.modules = [\"AI Intelligence\", \"Data Analytics\", \"FastAPI Automation\", \"IoT Control\"]\n        self.timestamp = datetime.now().strftime(\"%Y-%m-%d %H:%M:%S\")\n\n    def analyze_operations(self, tasks_count=120):\n        efficiency_score = round(94.8 + math.log10(tasks_count) * 2, 2)\n        return {\n            \"status\": \"Optimal\",\n            \"efficiency\": f\"{efficiency_score}%\",\n            \"tasks_processed\": tasks_count,\n            \"cost_reduction\": \"35%\",\n            \"execution_speed\": \"0.14ms per query\"\n        }\n\n    def generate_report(self):\n        metrics = self.analyze_operations(350)\n        print(f\"📌 العميل: {self.client}\")\n        print(f\"⏱️ توقيت التنفيذ: {self.timestamp}\")\n        print(f\"🚀 المنظومة الأساسية: Python 3.12 Engine (ودنوح AAMN)\")\n        print(\"-\" * 45)\n        for k, v in metrics.items():\n            print(f\" • {k.replace('_', ' ').title()}: {v}\")\n        print(\"-\" * 45)\n        print(\"✅ تم إنجاز الأتمتة بنجاح 100% بدون أي أخطاء!\")\n\n# تشغيل المحرك\nengine = WadNoohAAMN()\nengine.generate_report()",

        py_ai: "# بايثون: نموذج ذكاء اصطناعي وتوقع الأداء المالي\nimport random\n\ndef predict_growth(current_revenue, months=6):\n    print(\"🤖 جاري تشغيل نموذج بايثون للتنبؤ الذكي...\")\n    forecast = []\n    val = current_revenue\n    for m in range(1, months + 1):\n        growth_rate = 1.0 + (random.randint(8, 16) / 100.0)\n        val = round(val * growth_rate, 2)\n        forecast.append((f\"الشهر {m}\", val))\n    \n    print(\"📈 توقعات نمو الإيرادات للأشهر القادمة:\")\n    for month, amount in forecast:\n        print(f\" • {month}: {amount:,.2f} ر.س\")\n    print(\"🎯 دقة التوقع الإحصائي: 98.4%\")\n\npredict_growth(25000, 6)",

        py_auto: "# بايثون: أتمتة معالجة البيانات وتصدير السجلات\nimport json\n\noperations = [\n    {\"id\": \"REQ-101\", \"service\": \"برمجة بايثون\", \"status\": \"مكتمل\", \"price\": 3200},\n    {\"id\": \"REQ-102\", \"service\": \"أتمتة تقارير\", \"status\": \"مكتمل\", \"price\": 1800},\n    {\"id\": \"REQ-103\", \"service\": \"تطوير لوحة تحكم\", \"status\": \"قيد التنفيذ\", \"price\": 4500}\n]\n\ntotal_sales = sum(o[\"price\"] for o in operations)\ncompleted = len([o for o in operations if o[\"status\"] == \"مكتمل\"])\n\nprint(\"📊 ملخص تقرير الأتمتة اليومي:\")\nprint(f\" • إجمالي الإيرادات: {total_sales:,} ر.س\")\nprint(f\" • العمليات المكتملة: {completed}/{len(operations)}\")\nprint(\"💾 تم حفظ وتصدير التقرير في قاعدة البيانات بنجاح.\")",

        py_iot: "# بايثون: التحكم بالدوائر والمتحكمات (MicroPython/IoT)\nclass CircuitSensor:\n    def __init__(self, pin=4):\n        self.pin = pin\n        self.status = \"ONLINE\"\n    \n    def read_telemetry(self):\n        voltage = 3.3\n        temperature = 24.5\n        current = 0.42\n        power = round(voltage * current, 2)\n        print(f\"🔌 حساس الدائرة (Pin {self.pin}): {self.status}\")\n        print(f\"⚡ الجهد: {voltage}V | التيار: {current}A | القدرة: {power}W\")\n        print(f\"🌡️ درجة الحرارة: {temperature}°C (مستقرة)\")\n\nsensor = CircuitSensor()\nsensor.read_telemetry()",

        javascript: "// JavaScript / TypeScript Interactive Execution\nconst aamnPlatform = {\n    name: \"ودنوح AAMN\",\n    version: \"3.5.0\",\n    languages: [\"Python (Core)\", \"JavaScript\", \"C#\", \"PHP\", \"C++\", \"SQL\"],\n    deployments: {\n        activeUsers: 1450,\n        uptime: \"99.98%\",\n        status: \"Production Ready\"\n    }\n};\n\nconsole.log(\"🌐 مرحباً بك في منصة ودنوح AAMN للبرمجيات!\");\nconsole.log(\"اللغات المدعومة:\", aamnPlatform.languages.join(\" · \"));\nconsole.log(\"الحالة العامة:\", aamnPlatform.deployments.status);\nconsole.log(\"نسبة الجهوزية:\", aamnPlatform.deployments.uptime);",

        csharp: "// C# .NET Enterprise Architecture\nusing System;\nusing System.Collections.Generic;\n\npublic class EnterpriseService\n{\n    public string SystemName => \"WADNOOH AAMN Core\";\n    public bool HighAvailability => true;\n\n    public void RunDiagnostics()\n    {\n        Console.WriteLine($\"[C# .NET] Initializing {SystemName}...\");\n        Console.WriteLine(\" ✓ Security Middleware: Active\");\n        Console.WriteLine(\" ✓ Database Pool: 100% Connected\");\n        Console.WriteLine(\" ✓ Enterprise Architecture Ready for High Load.\");\n    }\n}\n\nnew EnterpriseService().RunDiagnostics();",

        php: "<?php\n// PHP & Laravel Web Services\n$company = \"ودنوح AAMN للبرمجيات\";\n$services = [\n    \"بوابات الدفع الإلكتروني\" => \"Active\",\n    \"أنظمة إدارة المحتوى\" => \"Active\",\n    \"ربط واجهات API\" => \"Active\"\n];\n\necho \"🐘 خادم PHP يعمل بنجاح:\\n\";\necho \"الشركة: \" . $company . \"\\n\";\nforeach ($services as $service => $status) {\n    echo \" • \" . $service . \": [\" . $status . \"]\\n\";\n}\n?>",

        cpp: "// C++ Embedded Systems & Hardware Control\n#include <iostream>\n\nint main() {\n    std::cout << \"⚙️ [C++ Systems Engine]\" << std::endl;\n    std::cout << \"Memory Allocation: Optimized\" << std::endl;\n    std::cout << \"Hardware Interfaces: Ready\" << std::endl;\n    std::cout << \"Microcontroller Clock: 168 MHz\" << std::endl;\n    std::cout << \"Status: Ultra Fast Execution!\" << std::endl;\n    return 0;\n}",

        sql: "-- SQL Database Analytics & Performance Queries\nSELECT \n    service_name,\n    COUNT(id) AS total_requests,\n    SUM(amount) AS total_revenue,\n    AVG(satisfaction_score) AS avg_rating\nFROM aamn_operations\nWHERE status = 'completed'\nGROUP BY service_name\nORDER BY total_revenue DESC;\n\n/* Output: \n   1. Python Automation -> 142 Requests -> 245,000 SAR\n   2. Web Systems       -> 98 Requests  -> 180,000 SAR\n   3. Electrical & IoT  -> 76 Requests  -> 112,000 SAR\n*/"
    };

    const editor = document.getElementById('codeEditor');
    const terminal = document.getElementById('terminalOutput');
    const fileName = document.getElementById('ideFileName');

    window.switchLanguage = function (lang) {
        document.querySelectorAll('.lang-tab-btn').forEach(btn => {
            btn.classList.toggle('active', btn.dataset.lang === lang);
        });

        const extMap = {
            python: 'main.py - Python 3.12 (Core Engine)',
            javascript: 'app.js - JavaScript ES6 / TypeScript',
            csharp: 'Program.cs - C# .NET Enterprise',
            php: 'index.php - PHP Web Service',
            cpp: 'system.cpp - C++ Embedded Systems',
            sql: 'analytics.sql - SQL Database Engine'
        };

        if (fileName) fileName.textContent = extMap[lang] || 'script.txt';
        if (editor && snippets[lang]) editor.value = snippets[lang];

        runSimulation(lang);
    };

    window.loadSnippet = function (snipKey) {
        if (snippets[snipKey] && editor) {
            editor.value = snippets[snipKey];
            runSimulation('python');
        }
    };

    window.clearTerminal = function () {
        if (terminal) terminal.innerHTML = '<span class="prompt">$ </span>';
    };

    function runSimulation(lang) {
        if (!terminal || !editor) return;
        const code = editor.value;

        terminal.innerHTML = '<span class="prompt">$ compiling & executing...</span>\n<span class="info">[Runtime: ' + (lang || 'python') + ']</span>\n';

        setTimeout(() => {
            let output = '';
            const now = new Date().toLocaleTimeString('ar-SA');

            if (code.includes('predict_growth')) {
                output = '<span class="prompt">$ python predict.py</span>\n' +
                    '🤖 جاري تشغيل نموذج بايثون للتنبؤ الذكي...\n' +
                    '📈 توقعات نمو الإيرادات للأشهر القادمة:\n' +
                    ' • الشهر 1: 27,850.00 ر.س\n' +
                    ' • الشهر 2: 31,470.50 ر.س\n' +
                    ' • الشهر 3: 35,561.67 ر.س\n' +
                    ' • الشهر 4: 40,895.91 ر.س\n' +
                    ' • الشهر 5: 46,212.38 ر.س\n' +
                    ' • الشهر 6: 52,682.11 ر.س\n' +
                    '🎯 دقة التوقع الإحصائي: 98.4%\n' +
                    '<span class="success">✅ تمت المعالجة بنجاح في 0.08 ثانية</span>';
            } else if (code.includes('operations =')) {
                output = '<span class="prompt">$ python automation.py</span>\n' +
                    '📊 ملخص تقرير الأتمتة اليومي:\n' +
                    ' • إجمالي الإيرادات: 9,500 ر.س\n' +
                    ' • العمليات المكتملة: 2/3\n' +
                    '💾 تم حفظ وتصدير التقرير في قاعدة البيانات بنجاح.\n' +
                    '<span class="success">✅ حالة الأتمتة: سارية ومنتظمة 100%</span>';
            } else if (code.includes('CircuitSensor')) {
                output = '<span class="prompt">$ micropython circuit_iot.py</span>\n' +
                    '🔌 حساس الدائرة (Pin 4): ONLINE\n' +
                    '⚡ الجهد: 3.3V | التيار: 0.42A | القدرة: 1.39W\n' +
                    '🌡️ درجة الحرارة: 24.5°C (مستقرة)\n' +
                    '<span class="success">✅ استجابة المتحكم طبيعية وآمنة</span>';
            } else if (code.includes('EnterpriseService')) {
                output = '<span class="prompt">$ dotnet run</span>\n' +
                    '[C# .NET] Initializing WADNOOH AAMN Core...\n' +
                    ' ✓ Security Middleware: Active\n' +
                    ' ✓ Database Pool: 100% Connected\n' +
                    ' ✓ Enterprise Architecture Ready for High Load.\n' +
                    '<span class="success">✅ Build Succeeded (0 Errors, 0 Warnings)</span>';
            } else if (code.includes('aamnPlatform')) {
                output = '<span class="prompt">$ node app.js</span>\n' +
                    '🌐 مرحباً بك في منصة ودنوح AAMN للبرمجيات!\n' +
                    'اللغات المدعومة: Python (Core) · JavaScript · C# · PHP · C++ · SQL\n' +
                    'الحالة العامة: Production Ready\n' +
                    'نسبة الجهوزية: 99.98%\n' +
                    '<span class="success">✅ Process exited with code 0</span>';
            } else if (code.includes('std::cout')) {
                output = '<span class="prompt">$ g++ -O3 system.cpp -o system && ./system</span>\n' +
                    '⚙️ [C++ Systems Engine]\n' +
                    'Memory Allocation: Optimized\n' +
                    'Hardware Interfaces: Ready\n' +
                    'Microcontroller Clock: 168 MHz\n' +
                    'Status: Ultra Fast Execution!\n' +
                    '<span class="success">✅ Execution completed in 0.002s</span>';
            } else if (code.includes('SELECT')) {
                output = '<span class="prompt">$ psql -U aamn_db -c "analytics.sql"</span>\n' +
                    ' service_name       | total_requests | total_revenue | avg_rating \n' +
                    '--------------------+----------------+---------------+------------\n' +
                    ' Python Automation  |            142 | 245000.00     | 4.95       \n' +
                    ' Web & Mobile Apps  |             98 | 180000.00     | 4.90       \n' +
                    ' Electrical & IoT   |             76 | 112000.00     | 4.88       \n' +
                    '<span class="success">✅ (3 rows) Query completed in 1.4ms</span>';
            } else {
                output = '<span class="prompt">$ python main.py</span>\n' +
                    '📌 العميل: شركة التقنية المتميزة\n' +
                    '⏱️ توقيت التنفيذ: ' + now + '\n' +
                    '🚀 المنظومة الأساسية: Python 3.12 Engine (ودنوح AAMN)\n' +
                    '---------------------------------------------\n' +
                    ' • Status: Optimal\n' +
                    ' • Efficiency: 99.89%\n' +
                    ' • Tasks Processed: 350\n' +
                    ' • Cost Reduction: 35%\n' +
                    ' • Execution Speed: 0.14ms per query\n' +
                    '---------------------------------------------\n' +
                    '<span class="success">✅ تم إنجاز الأتمتة بنجاح 100% بدون أي أخطاء!</span>';
            }

            terminal.innerHTML = output;
        }, 250);
    }

    // Init with default Python
    if (editor) {
        editor.value = snippets.python;
        runSimulation('python');
    }

    document.getElementById('btnRunCode')?.addEventListener('click', () => {
        runSimulation('python');
    });

    document.querySelectorAll('.lang-tab-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            window.switchLanguage(btn.dataset.lang);
        });
    });

    // Mobile Navigation
    const hamburger = document.getElementById('hamburger');
    const mobileMenu = document.getElementById('mobileMenu');
    const closeMenu = document.getElementById('closeMenu');
    const overlay = document.getElementById('overlay');

    function toggleMenu(open) {
        if (!mobileMenu || !overlay) return;
        mobileMenu.classList.toggle('active', open);
        overlay.classList.toggle('active', open);
    }

    hamburger?.addEventListener('click', () => toggleMenu(true));
    closeMenu?.addEventListener('click', () => toggleMenu(false));
    overlay?.addEventListener('click', () => toggleMenu(false));
})();
