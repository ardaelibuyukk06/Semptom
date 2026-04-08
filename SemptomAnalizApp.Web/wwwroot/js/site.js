// ═══════════════════════════════════════════════════════════════
//  SemptomAnaliz — Client-side JS
// ═══════════════════════════════════════════════════════════════

document.addEventListener('DOMContentLoaded', () => {

    // ── 1. BMI Canlı Hesaplama ───────────────────────────────────
    const boyInput  = document.getElementById('Boy');
    const kiloInput = document.getElementById('Kilo');
    const bmiBox    = document.getElementById('bmiBox');

    function hesaplaBmi() {
        if (!boyInput || !kiloInput || !bmiBox) return;
        const boy  = parseFloat(boyInput.value);
        const kilo = parseFloat(kiloInput.value);
        if (!boy || !kilo || boy < 50 || kilo < 10) { bmiBox.style.display = 'none'; return; }
        const bmi = kilo / ((boy / 100) ** 2);
        const { label, cls } = bmiKategori(bmi);
        bmiBox.innerHTML = `
            <div class="d-flex align-items-center gap-3">
                <div>
                    <div style="font-size:1.8rem;font-weight:800;line-height:1">${bmi.toFixed(1)}</div>
                    <div class="text-muted" style="font-size:.75rem;font-weight:600;text-transform:uppercase;letter-spacing:.05em">VKİ (BMI)</div>
                </div>
                <div class="flex-grow-1">
                    <div class="sa-bmi-track mb-2" style="margin-top:6px">
                        <div class="sa-bmi-marker" id="bmiMarker" style="left:${bmiYuzde(bmi)}%"></div>
                    </div>
                    <span class="badge bg-${cls} rounded-pill" style="font-size:.75rem">${label}</span>
                </div>
            </div>`;
        bmiBox.style.display = 'block';
        setTimeout(() => {
            const m = document.getElementById('bmiMarker');
            if (m) m.style.left = bmiYuzde(bmi) + '%';
        }, 50);
    }

    function bmiKategori(bmi) {
        if (bmi < 18.5) return { label: 'Zayıf',           cls: 'info'    };
        if (bmi < 25)   return { label: 'Normal',           cls: 'success' };
        if (bmi < 30)   return { label: 'Fazla Kilolu',     cls: 'warning' };
        if (bmi < 35)   return { label: 'Obez (Sınıf I)',   cls: 'danger'  };
        return              { label: 'Obez (Sınıf II+)', cls: 'danger'  };
    }
    function bmiYuzde(bmi) {
        return Math.min(100, Math.max(0, ((bmi - 15) / 30) * 100)).toFixed(1);
    }

    if (boyInput)  boyInput.addEventListener('input', hesaplaBmi);
    if (kiloInput) kiloInput.addEventListener('input', hesaplaBmi);
    hesaplaBmi();

    // ── 2. Semptom Chip Seçimi ───────────────────────────────────
    const chipContainer = document.getElementById('semptomChipContainer');
    const detailSection = document.getElementById('semptomDetaylar');
    const hiddenIds     = document.getElementById('semptomIdler');
    const hiddenSiddet  = document.getElementById('siddetler');
    const hiddenSure    = document.getElementById('sureler');

    if (chipContainer) {
        chipContainer.querySelectorAll('.sa-chip').forEach(chip => {
            chip.addEventListener('click', () => {
                chip.classList.toggle('selected');
                const id    = chip.dataset.id;
                const detayId = `detay-${id}`;
                let detay = document.getElementById(detayId);

                if (chip.classList.contains('selected')) {
                    if (!detay) {
                        detay = document.createElement('div');
                        detay.id = detayId;
                        detay.className = 'sa-chip-detail mb-2';
                        detay.dataset.semptomId = id;
                        detay.innerHTML = `
                            <div class="d-flex align-items-center gap-3 flex-wrap">
                                <strong class="text-primary" style="font-size:.85rem;min-width:120px">
                                    <i class="${chip.dataset.ikon} me-1"></i>${chip.dataset.ad}
                                </strong>
                                <div class="d-flex align-items-center gap-2">
                                    <label class="sa-form-label mb-0" style="white-space:nowrap">Şiddet:</label>
                                    <div class="btn-group btn-group-sm" role="group">
                                        <input type="radio" class="btn-check" name="s_${id}" id="sh_${id}" value="1" checked>
                                        <label class="btn btn-outline-primary" for="sh_${id}">Hafif</label>
                                        <input type="radio" class="btn-check" name="s_${id}" id="so_${id}" value="2">
                                        <label class="btn btn-outline-warning" for="so_${id}">Orta</label>
                                        <input type="radio" class="btn-check" name="s_${id}" id="ss_${id}" value="3">
                                        <label class="btn btn-outline-danger"  for="ss_${id}">Şiddetli</label>
                                    </div>
                                </div>
                                <div class="d-flex align-items-center gap-2">
                                    <label class="sa-form-label mb-0" style="white-space:nowrap">Süre:</label>
                                    <select class="form-select form-select-sm" style="width:90px" id="sure_${id}">
                                        ${[1,2,3,5,7,10,14,21,30].map(d => `<option value="${d}">${d} gün</option>`).join('')}
                                    </select>
                                </div>
                            </div>`;
                        detailSection.appendChild(detay);
                    }
                    detailSection.style.display = 'block';
                } else {
                    if (detay) detay.remove();
                    if (!detailSection.querySelector('.sa-chip-detail'))
                        detailSection.style.display = 'none';
                }
                if (window._updateSemptomSayaci) window._updateSemptomSayaci();
                if (window._kontrolRedFlag)      window._kontrolRedFlag();
            });
        });

        // ── Form gönderimi: doğrulama + yükleme modali ───────────
        const analizForm = document.getElementById('analizForm');
        if (analizForm) {
            analizForm.addEventListener('submit', async (e) => {
                e.preventDefault();

                const detaylar = detailSection.querySelectorAll('.sa-chip-detail');

                // Semptom seçilmemiş uyarısı
                if (!detaylar.length) {
                    window.SwalDark.fire({
                        icon: 'warning',
                        title: 'Semptom Seçilmedi',
                        html: '<span style="color:var(--sa-text-sec)">Analiz yapabilmek için lütfen en az bir semptom seçiniz.</span>',
                        confirmButtonText: '<i class="bi bi-check me-1"></i>Tamam',
                        iconColor: '#FCD34D'
                    });
                    return;
                }

                // Hidden input değerlerini doldur
                const ids = [], siddetArr = [], sureArr = [];
                detaylar.forEach(d => {
                    const sid = d.dataset.semptomId;
                    ids.push(sid);
                    const checked = d.querySelector(`input[name="s_${sid}"]:checked`);
                    siddetArr.push(checked ? checked.value : '2');
                    const sureEl = d.querySelector(`#sure_${sid}`);
                    sureArr.push(sureEl ? sureEl.value : '1');
                });
                hiddenIds.value    = ids.join(',');
                hiddenSiddet.value = siddetArr.join(',');
                hiddenSure.value   = sureArr.join(',');

                // Yükleme modali göster
                window.SwalDark.fire({
                    title: 'Analiz Yapılıyor',
                    html: `<span style="color:var(--sa-text-sec)">Semptomlarınız işleniyor, lütfen bekleyiniz...</span>`,
                    allowOutsideClick: false,
                    allowEscapeKey: false,
                    showConfirmButton: false,
                    didOpen: () => { Swal.showLoading(); }
                });

                // Formu programatik olarak gönder
                setTimeout(() => analizForm.submit(), 80);
            });
        }
    }

    // ── 3. Progress Bar Animasyonu ───────────────────────────────
    document.querySelectorAll('.sa-progress-fill[data-width]').forEach(bar => {
        const target = bar.dataset.width;
        setTimeout(() => { bar.style.width = target + '%'; }, 300);
    });

    // ── 4. BMI Marker (sonuç sayfası) ────────────────────────────
    const resultBmiMarker = document.getElementById('resultBmiMarker');
    if (resultBmiMarker) {
        setTimeout(() => {
            resultBmiMarker.style.left = resultBmiMarker.dataset.pct + '%';
        }, 400);
    }

    // ── 5. Counter Animasyonu (dashboard) ────────────────────────
    document.querySelectorAll('.sa-counter').forEach(el => {
        const target = parseInt(el.dataset.target, 10);
        if (!target) return;
        let current = 0;
        const step  = Math.max(1, Math.floor(target / 30));
        const timer = setInterval(() => {
            current = Math.min(current + step, target);
            el.textContent = current;
            if (current >= target) clearInterval(timer);
        }, 30);
    });

    // ── 6. Aciliyet Skoru Animasyonu ─────────────────────────────
    const scoreEl = document.getElementById('aciliyetSkorEl');
    if (scoreEl) {
        const target = parseInt(scoreEl.dataset.target, 10);
        let current  = 0;
        const timer  = setInterval(() => {
            current = Math.min(current + 2, target);
            scoreEl.textContent = current;
            if (current >= target) clearInterval(timer);
        }, 20);
    }

    // ── 7. Kategori Filtresi ─────────────────────────────────────
    const katBtnler = document.querySelectorAll('.sa-kat-btn');
    katBtnler.forEach(btn => {
        btn.addEventListener('click', () => {
            katBtnler.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            const kat = btn.dataset.kat;
            document.querySelectorAll('.sa-kat-grup').forEach(grup => {
                grup.style.display =
                    (kat === 'hepsi' || grup.dataset.kat === kat) ? 'block' : 'none';
            });
        });
    });

    // ── 8. Admin: Kullanıcı Silme Onayı ─────────────────────────

    document.querySelectorAll('.sa-admin-sil-btn').forEach(btn => {
        btn.addEventListener('click', async () => {
            const kullaniciId = btn.dataset.id;
            const kullaniciAd = btn.dataset.ad;

            const result = await window.SwalDark.fire({
                icon: 'warning',
                title: 'Kullanıcıyı Sil',
                html: `
                    <span style="color:var(--sa-text-sec)">
                        <b style="color:var(--sa-text)">${kullaniciAd}</b> adlı kullanıcıyı silmek istediğinize emin misiniz?<br>
                        <small style="color:var(--sa-muted);margin-top:6px;display:block">Bu işlem geri alınamaz.</small>
                    </span>`,
                showCancelButton: true,
                confirmButtonText: '<i class="bi bi-trash me-1"></i>Evet, Sil',
                cancelButtonText: 'Vazgeç',
                iconColor: '#F87171',
                customClass: {
                    popup:         'sa-swal-popup',
                    confirmButton: 'sa-swal-btn-confirm sa-swal-btn-confirm-danger',
                    cancelButton:  'sa-swal-btn-cancel'
                },
                reverseButtons: true
            });

            if (!result.isConfirmed) return;

            // Onaylandı: gizli form ile POST
            const form = document.createElement('form');
            form.method = 'POST';
            form.action = `/Admin/KullaniciSil`;
            form.style.display = 'none';

            const idInput = document.createElement('input');
            idInput.type = 'hidden';
            idInput.name = 'id';
            idInput.value = kullaniciId;
            form.appendChild(idInput);

            // Antiforgery token'ı sayfadan al
            const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
            if (tokenEl) {
                const tokenInput = document.createElement('input');
                tokenInput.type  = 'hidden';
                tokenInput.name  = '__RequestVerificationToken';
                tokenInput.value = tokenEl.value;
                form.appendChild(tokenInput);
            }

            document.body.appendChild(form);
            form.submit();
        });
    });

    // ── 10. Scroll Reveal (IntersectionObserver) ─────────────────
    const revealObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
                revealObserver.unobserve(entry.target);
            }
        });
    }, { threshold: 0.08, rootMargin: '0px 0px -30px 0px' });

    document.querySelectorAll('.sa-reveal').forEach(el => revealObserver.observe(el));

    // ── 12. Urgency Ring Fill Animation ──────────────────────────
    const ringFill = document.getElementById('urgencyRingFill');
    if (ringFill) {
        const target       = parseInt(ringFill.dataset.target, 10) || 0;
        const circumference = 2 * Math.PI * 50; // r=50 → 314.159…
        ringFill.style.strokeDasharray  = circumference;
        ringFill.style.strokeDashoffset = circumference;
        setTimeout(() => {
            ringFill.style.strokeDashoffset =
                circumference - (target / 100) * circumference;
        }, 350);
    }

    // ── 13. Semptom Counter Badge ─────────────────────────────────
    const semptomSayaciEl  = document.getElementById('semptomSayaci');
    const semptomSayaciNum = document.getElementById('semptomSayaciSayi');
    function updateSemptomSayaci() {
        if (!semptomSayaciEl || !semptomSayaciNum || !detailSection) return;
        const count = detailSection.querySelectorAll('.sa-chip-detail').length;
        semptomSayaciNum.textContent = count;
        semptomSayaciEl.style.display = count > 0 ? 'inline-flex' : 'none';
    }
    // Patch: expose updater so chip clicks can call it
    window._updateSemptomSayaci = updateSemptomSayaci;

    // ── 14. Kırmızı Bayrak — Kritik Semptom Tespiti ──────────────
    // Katalog ID'leri (SemptomKatalog.Id): 7=Nefes darlığı, 30=Göğüs ağrısı,
    // 31=Çarpıntı, 32=Baygınlık hissi
    const KRITIK_TEKLI   = [7, 30, 32];
    const KRITIK_KOMBIN  = [
        { ids: [30,  7], seviye: 'acil'   }, // Göğüs ağrısı + Nefes darlığı
        { ids: [30, 32], seviye: 'acil'   }, // Göğüs ağrısı + Baygınlık hissi
        { ids: [30, 31], seviye: 'acil'   }, // Göğüs ağrısı + Çarpıntı
        { ids: [ 7, 32], seviye: 'dikkat' }, // Nefes darlığı + Baygınlık hissi
    ];
    const MESAJLAR = {
        acil: {
            ikon:   'bi-exclamation-triangle-fill',
            baslik: 'Lütfen Önce Bir Sağlık Uzmanına Başvurun',
            metin:  'Seçtiğiniz semptom kombinasyonu acil tıbbi değerlendirme gerektirebilir. ' +
                    'Analiz yapmadan önce lütfen bir doktora veya acil servise başvurun.',
            cls:    'sa-redflag-acil',
        },
        dikkat: {
            ikon:   'bi-exclamation-circle-fill',
            baslik: 'Dikkat Gerektiren Semptom Tespit Edildi',
            metin:  'Seçtiğiniz semptomlar dikkat gerektiren bir tablo oluşturabilir. ' +
                    'Belirtileriniz şiddetlenirse zaman kaybetmeden bir sağlık uzmanına başvurunuz.',
            cls:    'sa-redflag-dikkat',
        },
        tekli: {
            ikon:   'bi-info-circle-fill',
            baslik: 'Kritik Semptom Seçildi',
            metin:  'Bu semptom tek başına bile tıbbi değerlendirme gerektirebilir. ' +
                    'Durumunuz hızla kötüleşirse sağlık kuruluşuna başvurun.',
            cls:    'sa-redflag-tekli',
        },
    };

    const redFlagBanner = document.getElementById('redFlagBanner');

    function kontrolRedFlag() {
        if (!redFlagBanner || !chipContainer) return;

        const seciliIdler = new Set(
            [...chipContainer.querySelectorAll('.sa-chip.selected')]
                .map(c => parseInt(c.dataset.id, 10))
        );

        // En yüksek seviyeyi bul: kombinasyon > tekli > yok
        let seviye = null;

        for (const k of KRITIK_KOMBIN) {
            if (k.ids.every(id => seciliIdler.has(id))) {
                seviye = k.seviye;
                break;
            }
        }

        if (!seviye) {
            const kritikTekliSecili = KRITIK_TEKLI.filter(id => seciliIdler.has(id));
            if (kritikTekliSecili.length > 0) seviye = 'tekli';
        }

        if (!seviye) {
            redFlagBanner.style.display = 'none';
            redFlagBanner.className = '';
            return;
        }

        const { ikon, baslik, metin, cls } = MESAJLAR[seviye];

        // Sadece içerik değiştiyse DOM'u güncelle (gereksiz reflow önle)
        const yeniHtml = `
            <div class="d-flex align-items-start gap-3">
                <i class="bi ${ikon} sa-redflag-ikon flex-shrink-0 mt-1"></i>
                <div>
                    <div class="sa-redflag-baslik">${baslik}</div>
                    <div class="sa-redflag-metin">${metin}</div>
                </div>
            </div>`;

        if (redFlagBanner.dataset.seviye !== seviye) {
            redFlagBanner.className = `sa-redflag-banner mb-4 ${cls}`;
            redFlagBanner.innerHTML = yeniHtml;
            redFlagBanner.dataset.seviye = seviye;
            redFlagBanner.style.display = 'block';
        }
    }

    window._kontrolRedFlag = kontrolRedFlag;

});
