using System;
using System.Collections.Generic;
using System.Linq;

class Symbol
{
    public char Character;
    public int Frequency;
    public double Probability;
    public string Code = "";
}

class Program
{
    static List<Symbol> symbols = new();
    static string input = "";
    static string encoded = "";
  
    static List<bool> isLowerCase = new(); // Lưu trạng thái chữ thường

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("+) BƯỚC 1: Nhập chuỗi cần mã hóa");
        Console.Write("- Nhập chuỗi: ");
        string originalInput = Console.ReadLine();

        // Lưu định dạng chữ thường/hoa
        isLowerCase = originalInput.Select(c => char.IsLower(c)).ToList();

        // Chuyển hóa thành in hoa
        input = originalInput.ToUpper();

        Console.WriteLine($"\n- Chuỗi gốc: {originalInput}");
        Console.WriteLine($"- Sau khi chuyển thành in hoa: {input}");

        // BƯỚC 2: Đếm tần suất và tính xác suất
        Console.WriteLine("\n+) BƯỚC 2: Đếm tần suất và tính xác suất");
        var frequencyDict = new Dictionary<char, int>();
        foreach (char c in input)
        {
            if (frequencyDict.ContainsKey(c))
                frequencyDict[c]++;
            else
                frequencyDict[c] = 1;
        }

        int totalChars = input.Length;
        symbols = frequencyDict
            .Select(kv => new Symbol
            {
                Character = kv.Key,
                Frequency = kv.Value,
                Probability = (double)kv.Value / totalChars
            })
            .OrderByDescending(s => s.Probability)
            .ToList();

        Console.WriteLine("\n Danh sách ký tự, tần suất, xác suất:");
        foreach (var s in symbols)
            Console.WriteLine($"- '{s.Character}' ➜ Tần suất: {s.Frequency} ➜ Xác suất: {s.Probability:P2}");

        // BƯỚC 3: Xây dựng mã Shannon–Fano
        Console.WriteLine("\n+) BƯỚC 3: Xây dựng mã Shannon–Fano (chia nhóm & gán mã)");
        BuildShannonFano(symbols, 0, symbols.Count - 1);

        Console.WriteLine("\n BẢNG MÃ SHANNON–FANO:");
        foreach (var s in symbols)
            Console.WriteLine($"- Ký tự: '{s.Character}' | Tần suất: {s.Frequency} | Xác suất: {s.Probability:P2} | Mã: {s.Code}");

        Console.WriteLine("\n CÂY NHỊ PHÂN SHANNON–FANO:");
        PrintBinaryTree(symbols);

        // MENU CHỌN BƯỚC
        while (true)
        {
            Console.WriteLine("\n CHỌN CHỨC NĂNG TIẾP THEO:");
            Console.WriteLine("1. Tính Entropy và đánh giá hiệu quả");
            Console.WriteLine("2. Mã hóa chuỗi");
            Console.WriteLine("3. Giải mã chuỗi nhị phân");
            Console.WriteLine("0. Thoát chương trình");
            Console.Write("Lựa chọn của bạn: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    TinhEntropy(symbols);
                    break;
                case "2":
                    MaHoa(symbols);
                    break;
                case "3":
                    if (string.IsNullOrEmpty(encoded))
                    {
                        Console.WriteLine("! Bạn cần thực hiện mã hóa trước khi giải mã!");
                    }
                    else
                    {
                        GiaiMa(symbols);
                    }
                    break;
                case "0":
                    Console.WriteLine(" Tạm biệt!");
                    return;
                default:
                    Console.WriteLine("! Lựa chọn không hợp lệ!");
                    break;
            }
        }
    }

    static void BuildShannonFano(List<Symbol> symbols, int start, int end)
    {
        if (start >= end) return;

        double totalProb = symbols.Skip(start).Take(end - start + 1).Sum(s => s.Probability);
        int bestSplit = start;
        double minDiff = double.MaxValue;

        for (int i = start; i < end; i++)
        {
            double prob1 = symbols.Skip(start).Take(i - start + 1).Sum(s => s.Probability);
            double prob2 = symbols.Skip(i + 1).Take(end - i).Sum(s => s.Probability);
            double diff = Math.Abs(prob1 - prob2);

            if (diff < minDiff)
            {
                minDiff = diff;
                bestSplit = i;
            }
        }

        for (int i = start; i <= bestSplit; i++) symbols[i].Code += "0";
        for (int i = bestSplit + 1; i <= end; i++) symbols[i].Code += "1";

        BuildShannonFano(symbols, start, bestSplit);
        BuildShannonFano(symbols, bestSplit + 1, end);
    }

    static void PrintBinaryTree(List<Symbol> symbols)
    {
        Console.WriteLine("(*) Gốc");
        PrintNode(symbols, "", "", true);
    }

    static void PrintNode(List<Symbol> symbols, string prefix, string indent, bool isLast)
    {
        var group = symbols.Where(s => s.Code.StartsWith(prefix)).ToList();
        if (group.Count == 1 && group[0].Code == prefix)
        {
            Console.WriteLine($"{indent}└── {group[0].Character} ({group[0].Code})");
            return;
        }

        if (prefix != "")
        {
            Console.WriteLine($"{indent}{(isLast ? "└──" : "├──")} [{prefix}]");
            indent += isLast ? "    " : "│   ";
        }

        string leftPrefix = prefix + "0";
        string rightPrefix = prefix + "1";

        var leftGroup = symbols.Where(s => s.Code.StartsWith(leftPrefix)).ToList();
        var rightGroup = symbols.Where(s => s.Code.StartsWith(rightPrefix)).ToList();

        if (leftGroup.Count == 1 && leftGroup[0].Code == leftPrefix)
            Console.WriteLine($"{indent}├── {leftGroup[0].Character} ({leftGroup[0].Code})");
        else if (leftGroup.Count > 0)
            PrintNode(symbols, leftPrefix, indent, false);

        if (rightGroup.Count == 1 && rightGroup[0].Code == rightPrefix)
            Console.WriteLine($"{indent}└── {rightGroup[0].Character} ({rightGroup[0].Code})");
        else if (rightGroup.Count > 0)
            PrintNode(symbols, rightPrefix, indent, true);
    }

    static void TinhEntropy(List<Symbol> symbols)
    {
        Console.WriteLine("\n+) TÍNH ENTROPY VÀ ĐÁNH GIÁ HIỆU QUẢ");
        Console.WriteLine("\n- Giải thích công thức:");
        Console.WriteLine("- Entropy (H) = -∑ P(x) * log₂(P(x))");
        Console.WriteLine("- Độ dài mã trung bình (L) = ∑ P(x) * |mã của x|");
        Console.WriteLine("- Hiệu suất = H / L * 100%");

        double entropy = 0, avgLength = 0;

        Console.WriteLine("\n Chi tiết tính toán:");
        Console.WriteLine("Ký tự | P(x)   | L(x) | -P*log₂(P) | P*L");
        Console.WriteLine("------|--------|------|-------------|------");

        foreach (var s in symbols)
        {
            double p = s.Probability;
            int len = s.Code.Length;
            double e = -p * Math.Log(p, 2);
            double l = p * len;
            entropy += e;
            avgLength += l;

            Console.WriteLine($"  {s.Character}   | {p:F4} |  {len}   |   {e:F4}   | {l:F4}");
        }

        Console.WriteLine($"\n- Entropy (H): {entropy:F4} bits/symbol");
        Console.WriteLine($"- Độ dài mã trung bình (L): {avgLength:F4} bits");

        double efficiency = entropy / avgLength * 100;
        Console.WriteLine($"- Hiệu suất mã hóa: {efficiency:F2}%");

        string eval = efficiency >= 90 ? "Hiệu quả"
                      : efficiency >= 50 ? "Trung bình"
                      : "Kém hiệu quả";
        Console.WriteLine($"- Đánh giá: {eval}");
    }

    static void MaHoa(List<Symbol> symbols)
    {
        Console.WriteLine("\n+) MÃ HÓA CHUỖI:");
        encoded = "";
        foreach (char c in input)
        {
            var code = symbols.First(s => s.Character == c).Code;
            Console.WriteLine($"Ký tự '{c}' ➜ Mã: {code}");
            encoded += code;
        }
        Console.WriteLine($"\n=> Chuỗi đã mã hóa: {encoded}");
    }

    static void GiaiMa(List<Symbol> symbols)
    {
        Console.WriteLine("\n+) GIẢI MÃ CHUỖI:");
        string decoded = "";
        string current = "";
        var codeToChar = symbols.ToDictionary(s => s.Code, s => s.Character);

        foreach (char bit in encoded)
        {
            current += bit;
            if (codeToChar.ContainsKey(current))
            {
                char ch = codeToChar[current];
                Console.WriteLine($"Mã: {current} ➜ Ký tự: '{ch}'");
                decoded += ch;
                current = "";
            }
        }

        Console.WriteLine($"\n=> Chuỗi đã giải mã: {decoded}");
        // 🔄 Khôi phục chữ thường nếu cần
        string restored = "";
        for (int i = 0; i < decoded.Length && i < isLowerCase.Count; i++)
        {
            restored += isLowerCase[i] ? char.ToLower(decoded[i]) : decoded[i];
        }
        Console.WriteLine($"\n=> Chuỗi khôi phục đúng định dạng: {restored}");
    }
}
