# Trap Pushback + Joint Constraint System
Prototype mekanik untuk game jam Unity 6000.3.12f1. Semua parameter penting
ada di Inspector, jadi iterasi desain tidak perlu sentuh kode sama sekali.

---

## 1. Setup Trap Pushback

File: `TrapSystem/IPushable.cs`, `TrapPushback.cs`, `PlayerPushReceiver.cs`

### Langkah:
1. Import ketiga file `.cs` ke folder `Assets/Scripts/TrapSystem/` di project-mu.
2. Pastikan Player kamu punya **Tag = "Player"** (Inspector → dropdown Tag di
   atas, kalau belum ada buat lewat "Add Tag...").
3. Buat GameObject untuk trap (misal cube pipih untuk floor trap, atau dinding).
   - Tambahkan **Collider** (BoxCollider dsb), centang **Is Trigger**.
   - Tambahkan komponen **TrapPushback**.
4. Atur di Inspector:
   - `Direction Mode`:
     - `TrapForward` → dorong searah sumbu Z biru trap (cocok untuk panel/spring trap).
     - `AwayFromTrapCenter` → dorong menjauh dari posisi trap (cocok untuk ranjau/ledakan radial).
     - `CustomDirection` → arah manual, isi `Custom Direction`.
   - `Push Force`, `Upward Boost` → kekuatan dorongan.
   - `Cooldown`, `One Shot` → trap berulang atau sekali pakai.
   - `Activation Delay` → kasih jeda kalau mau ada animasi ancang-ancang dulu
     sebelum dorongan kena (efek "kelihatan tapi telat ngehindar", ala jebakan
     di *The Professional*).
   - `On Trap Triggered` / `On Trap Ready` (UnityEvent) → drag function dari
     Animator, AudioSource.Play, atau VFX di sini, tanpa perlu kode tambahan.
5. **Kalau Player pakai Rigidbody** → selesai, tidak perlu langkah tambahan,
   `TrapPushback` otomatis pakai `Rigidbody.AddForce`.
6. **Kalau Player pakai CharacterController (tanpa Rigidbody)** → tambahkan
   komponen `PlayerPushReceiver` di GameObject Player yang sama. Komponen ini
   menampung gaya dorong lalu meredamnya bertahap (`Drag`) supaya gerakannya halus.
7. Play mode → jalan ke trap → karakter ke-push.

---

## 2. Setup Joint Constraint System

File: `JointSystem/JointConstraint.cs`, `ProceduralJointAnimator.cs`

### Langkah:
1. Import ke `Assets/Scripts/JointSystem/`.
2. Susun hierarchy "tulang" sebagai parent-child, misalnya:
   ```
   Root (bahu)
     └─ UpperLimb
          └─ LowerLimb
               └─ Tip (collider trap di sini kalau dipakai sbg pendulum trap)
   ```
3. Tambahkan komponen **JointConstraint** di tiap object (UpperLimb, LowerLimb, dst).
4. Atur per sendi:
   - `Joint Type`:
     - `Hinge` → 1 sumbu putar saja (pilih `Hinge Axis`: X/Y/Z). Cocok untuk
       siku, lutut, pintu, pendulum.
     - `BallSocket` → bebas 3 sumbu dengan limit masing-masing (X/Y/Z).
       Cocok untuk bahu, pangkal tentakel.
     - `Fixed` → tidak bisa berputar sama sekali (tulang kaku).
   - `Limit X/Y/Z` → rentang derajat minimum-maksimum, langsung kelihatan di
     Scene view sebagai gizmo (busur biru untuk Hinge, kerucut untuk BallSocket)
     waktu object di-select. Bentuk sendi = tinggal geser angka ini sambil
     lihat gizmo, tidak perlu trial-error lewat Play mode.
   - `Rotation Speed` → seberapa cepat sendi mengejar target rotasi (0 = instan).
5. Untuk gerakan otomatis (pendulum, tentakel, idle animation lengan robot):
   tambahkan **ProceduralJointAnimator** di GameObject yang sama dengan
   `JointConstraint`. Atur `Oscillation Axis`, `Amplitude`, `Speed`,
   `Phase Offset`.
   - Tips: kasih `Phase Offset` beda-beda tiap sendi dalam satu chain
     (mis. 0, 0.5, 1.0) supaya gerakannya jadi efek gelombang, bukan kaku
     gerak bareng.
6. Untuk gerakan manual/digerakkan script lain (AI, IK, input player), panggil
   `jointConstraint.SetTargetLocalEuler(Vector3 euler)` atau
   `AddDeltaEuler(Vector3 delta)` dari script-mu sendiri — limit otomatis
   diterapkan, kamu tidak perlu clamp manual.

### Gabungkan kedua sistem (contoh: swinging blade trap)
1. Buat chain joint pendek (Root → Arm → BladeTip).
2. `ProceduralJointAnimator` di `Arm` untuk bikin ayunan pendulum.
3. Tambahkan Collider (`Is Trigger`) + `TrapPushback` di `BladeTip`, supaya
   ujung lengan yang berayun itulah yang mendorong Player saat menyentuhnya.

---

## 3. Catatan Optimasi (kenapa script ditulis seperti ini)

- **Trigger-based, bukan polling jarak tiap frame.** `OnTriggerEnter` hanya
  jalan saat ada collision beneran, jauh lebih murah daripada cek
  `Vector3.Distance` ke Player tiap `Update()`.
- **Tidak ada `GetComponent` di dalam loop/Update.** Semua referensi
  (`CharacterController`, `JointConstraint`) di-cache di `Awake()` sekali saja.
- **Joint pakai Transform, bukan PhysX (ConfigurableJoint/CharacterJoint).**
  PhysX joint butuh Rigidbody + solver iteration tiap `FixedUpdate`, jauh
  lebih berat dan boros battery di mobile. Pendekatan di sini murni matematika
  Quaternion ringan, deterministik, dan tidak butuh Rigidbody sama sekali
  kecuali kamu memang perlu physics response (tabrakan, gravity, dll).
- **Gizmo digambar hanya di Editor** (`#if UNITY_EDITOR`), jadi tidak ada
  biaya sama sekali saat build/runtime di device.
- **Set layer collision matrix** (Project Settings → Physics) supaya trap
  hanya melakukan collision check terhadap layer Player, bukan semua layer
  di scene — penting kalau trap-nya banyak di level.
- **Hindari Update() kalau bisa di-skip:** kalau jumlah trap/joint sangat
  banyak (ratusan) dan kamu mulai lihat drop FPS di profiler, langkah lanjut
  yang murah: matikan `enabled = false` pada `TrapPushback`/`JointConstraint`
  yang berada jauh dari kamera (frustum/distance culling manual), karena
  Update() tetap dipanggil walau GameObject di luar layar.
- **`ForceMode.VelocityChange`** dipakai untuk dorongan trap karena ini
  perubahan kecepatan instan (tidak bergantung mass kayak `Force`/`Impulse`),
  hasilnya predictable dan murah dihitung — cocok untuk efek knockback yang
  konsisten di berbagai jenis musuh/Player.
