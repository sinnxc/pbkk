namespace CalculatorApp;

public partial class Form1 : Form
{
    double firstNumber = 0;
    double secondNumber = 0;
    double result = 0;
    string operation = "";
    bool isResultShown = false;

    public Form1()
    {
        InitializeComponent();
    }

    private void NumberButton_Click(object sender, EventArgs e)
    {
        Button button = (Button)sender;

        if (txtDisplay.Text == "0" || isResultShown)
        {
            txtDisplay.Text = button.Text;
            isResultShown = false;
        }
        else
        {
            txtDisplay.Text += button.Text;
        }
    }

    private void OperatorButton_Click(object sender, EventArgs e)
    {
        Button button = (Button)sender;

        if (isResultShown)
        {
            isResultShown = false;
        }

        // Jika operator ditekan ulang sebelum memasukkan angka kedua, ganti operatornya
        if (!string.IsNullOrEmpty(operation) && txtDisplay.Text.Trim().EndsWith(operation))
        {
            string current = txtDisplay.Text.Trim();
            txtDisplay.Text = current.Substring(0, current.Length - operation.Length).Trim() + " " + button.Text + " ";
            operation = button.Text;
            return;
        }

        // Jika sudah ada operasi sebelumnya yang belum dihitung, hitung terlebih dahulu
        if (!string.IsNullOrEmpty(operation))
        {
            CalculateResult();
            if (isResultShown)
            {
                isResultShown = false;
                operation = button.Text;
                txtDisplay.Text += " " + button.Text + " ";
                return;
            }
        }

        operation = button.Text;
        txtDisplay.Text += " " + button.Text + " ";
    }

    private void btnEquals_Click(object sender, EventArgs e)
    {
        CalculateResult();
    }

    private void CalculateResult()
    {
        if (string.IsNullOrEmpty(operation))
            return;

        try
        {
            string text = txtDisplay.Text;
            string opDelim = " " + operation + " ";
            int opIndex = text.IndexOf(opDelim);

            if (opIndex == -1)
            {
                opIndex = text.IndexOf(operation);
                if (opIndex == -1)
                    return;
            }

            string firstPart = text.Substring(0, opIndex).Trim();
            int secondStart = opIndex + (text.Contains(opDelim) ? opDelim.Length : operation.Length);
            string secondPart = text.Substring(secondStart).Trim();

            if (string.IsNullOrEmpty(secondPart))
                return;

            firstNumber = double.Parse(firstPart);
            secondNumber = double.Parse(secondPart);

            switch (operation)
            {
                case "+":
                    result = firstNumber + secondNumber;
                    break;
                case "−":
                case "-":
                    result = firstNumber - secondNumber;
                    break;
                case "×":
                case "x":
                case "*":
                    result = firstNumber * secondNumber;
                    break;
                case "÷":
                case "/":
                    if (secondNumber == 0)
                        throw new DivideByZeroException();
                    result = firstNumber / secondNumber;
                    break;
            }

            txtDisplay.Text = result.ToString();
            operation = "";
            isResultShown = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error");
        }
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
        firstNumber = 0;
        secondNumber = 0;
        result = 0;
        operation = "";
        isResultShown = false;
        txtDisplay.Text = "0";
    }

    private void btnDecimal_Click(object sender, EventArgs e)
    {
        if (isResultShown)
        {
            txtDisplay.Text = "0.";
            isResultShown = false;
            return;
        }

        if (string.IsNullOrEmpty(operation))
        {
            if (!txtDisplay.Text.Contains("."))
                txtDisplay.Text += ".";
        }
        else
        {
            int opIndex = txtDisplay.Text.LastIndexOf(operation);
            string secondPart = txtDisplay.Text.Substring(opIndex + operation.Length);
            if (!secondPart.Contains("."))
            {
                if (secondPart.Trim() == "")
                    txtDisplay.Text += "0.";
                else
                    txtDisplay.Text += ".";
            }
        }
    }
}

