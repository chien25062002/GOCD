=====================================================
BIG NUMBER – USAGE GUIDE
=====================================================

Namespace:
CodeSketch.Number

-----------------------------------------------------
1. OVERVIEW
-----------------------------------------------------

BigNumber là struct dùng để biểu diễn số rất lớn trong các game idle / incremental.

Cấu trúc số:
    value = mantissa * (1000 ^ exponent)

Trong đó:
- mantissa: double, luôn nằm trong khoảng [1 .. 1000)
- exponent: int, biểu diễn bậc nghìn (K, M, B, T, aa, ab, ...)

Ví dụ:
- 1500      → 1.5 K
- 2,500,000 → 2.5 M
- 1e18      → 1.0 aa


-----------------------------------------------------
2. WHY USE BigNumber
-----------------------------------------------------

BigNumber được thiết kế cho:
- Idle game
- Incremental game
- Currency tăng theo cấp số lớn
- Tránh overflow double / long
- Format gọn gàng cho UI

Đặc điểm:
- Không GC allocation
- Không dùng BigInteger
- So sánh nhanh
- Cộng / trừ có ngưỡng bỏ qua số nhỏ (idle-friendly)


-----------------------------------------------------
3. BASIC CREATION
-----------------------------------------------------

Tạo từ số thường:

    BigNumber a = new BigNumber(1500);
    // => 1.5K

Tạo từ mantissa + exponent:

    BigNumber b = new BigNumber(2.5, 2);
    // => 2.5M


-----------------------------------------------------
4. NORMALIZATION RULE
-----------------------------------------------------

BigNumber luôn tự normalize:

- mantissa >= 1000 → chia 1000, exponent++
- mantissa < 1     → nhân 1000, exponent--

Ví dụ:
    new BigNumber(2500, 0)
    => mantissa = 2.5
       exponent = 1


-----------------------------------------------------
5. OPERATIONS
-----------------------------------------------------

5.1 Addition

    BigNumber a = new BigNumber(1.2, 3); // 1.2B
    BigNumber b = new BigNumber(5.0, 0); // 5

    BigNumber c = a + b;
    // Nếu chênh exponent > 6 → bỏ qua số nhỏ

Lý do:
- Chuẩn idle game
- Tránh cộng số rất nhỏ vào số cực lớn
- Tăng hiệu năng


5.2 Subtraction

    BigNumber c = a - b;

- Nếu a <= b → trả về Zero
- Nếu chênh exponent > 6 → coi như không trừ


5.3 Multiply / Divide

    a * 2.5
    a / 3.0

- multiplier / divisor <= 0 → Zero


-----------------------------------------------------
6. COMPARISON
-----------------------------------------------------

Hỗ trợ:
    >  <  >=  <=

Ví dụ:

    if (gold >= cost)
    {
        gold -= cost;
    }

So sánh dựa trên:
1. exponent
2. mantissa


-----------------------------------------------------
7. FORMAT OUTPUT
-----------------------------------------------------

Compact string:

    BigNumber a = new BigNumber(1532000);
    string s = a.ToCompactString();
    // "1.53M"

Tuỳ chỉnh số chữ số thập phân:

    a.ToCompactString(1); // "1.5M"
    a.ToCompactString(3); // "1.532M"


Đơn vị:
- ""   (unit 0)
- K    (10^3)
- M    (10^6)
- B    (10^9)
- T    (10^12)
- aa, ab, ..., zz, aaa, ...

Extended unit dùng alphabet base-26.


-----------------------------------------------------
8. PARSE FROM STRING
-----------------------------------------------------

Từ chuỗi compact:

    BigNumber a = BigNumber.FromString("2.5aa");

Hoặc dùng extension:

    BigNumber a = "2.5aa".ToBigNumber();

Lưu ý:
- Parse dựa trên UtilsNumber.ParseCompactNumber
- Phù hợp load save / config / debug


-----------------------------------------------------
9. ZERO VALUE
-----------------------------------------------------

Zero chuẩn:

    BigNumber.Zero

Đặc điểm:
- mantissa = 0
- exponent = 0

Zero luôn an toàn cho:
- +  -  *  /  compare


-----------------------------------------------------
10. BEST PRACTICES
-----------------------------------------------------

✔ Dùng BigNumber cho currency, score, DPS, income
✔ Dùng double thường cho timer, speed, ratio
✔ Không dùng BigNumber cho physics / transform
✔ Không convert BigNumber → double cho logic lớn

Khuyến nghị:
- Lưu save bằng string compact ("2.5aa")
- Hoặc lưu mantissa + exponent riêng


-----------------------------------------------------
11. DESIGN NOTES
-----------------------------------------------------

- Struct → không GC
- Không allocation khi toán học
- Threshold exponent diff = 6 để giữ hiệu năng
- Format invariant culture (UI-safe)

BigNumber được thiết kế để:
"Fast, simple, and safe for idle games"


=====================================================
END OF FILE
=====================================================
