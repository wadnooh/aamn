const fs = require('fs');

const fixFile = (filePath) => {
    let content = fs.readFileSync(filePath, 'utf8');

    const properBlock =                 <div class=service-card data-aos=fade-up data-delay=400>
                    <div class=service-icon>
                        <i class=fas fa-plug></i>
                    </div>
                    <h3>الأجهزة والإكسسوارات</h3>
                    <p>توفير أحدث الأجهزة والملحقات والإكسسوارات المعتمدة بأعلى معايير الجودة والضمان.</p>
                    <a href=services.html class=service-link>اقرأ المزيد <i class=fas fa-arrow-left></i></a>
                </div>
                <div class=service-card data-aos=fade-up data-delay=500>
                    <div class=service-icon>
                        <i class=fas fa-flask></i>
                    </div>
                    <h3>مبيعات المختبرات</h3>
                    <p>توريد وتجهيز متطلبات ومعدات المختبرات والأجهزة المتخصصة بحلول تقنية دقيقة.</p>
                    <a href=services.html class=service-link>اقرأ المزيد <i class=fas fa-arrow-left></i></a>
                </div>
            </div>
        </div>
    </section>;

    content = content.replace(/<div class=service-card data-aos=fade-up data-delay=400>[\s\S]*?<h3>الأجهزة والإكسسوارات<\/h3>[\s\S]*?<p>توفير أجهز[^\n<]*/, properBlock);
    
    fs.writeFileSync(filePath, content, 'utf8');
    console.log('Fixed:', filePath);
};

fixFile('e:/FLY/SudanTravelApp.API/wwwroot/index.html');
fixFile('e:/FLY/index.html');