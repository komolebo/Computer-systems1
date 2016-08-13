using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace ComputerSystems
{
    
    public partial class Main : Form
    {
        private Timer t_time = new Timer(), t_interrupt = new Timer(), t_informer = new Timer();  // Таймери, що здійснюють переривання
        private Processor[] Processors = new Processor[5];  // Всі процесори
        private List<TasK> AllTasks = new List<TasK>();     // Список завдань

        private _DataType _fileOutput; // структура із всією інформацією для виведення у файл
        private _UserInput _userInput; // все, що ввів користувач
        public Main()
        {
            InitializeComponent();
            FIFO.Select();
            t_time.Tick += new EventHandler(this.t_Tick);           // Задіюємо всі-всі-всі
            t_interrupt.Tick += new EventHandler(this.t_Int);       //          всі-всі-всі
            t_informer.Tick += new EventHandler(this.t_info);       //          всі таймери
        }
        private void RunButton_Click(object sender, EventArgs e)
        {
            if(RunButton.Text == "RUN")
            {
                // Якщо нажали RUN:
                if (FIFO.Checked)
                    _fileOutput.d_MODE = 1;   // FIFO mode
                else if (WeakPlannerMode.Checked)
                    _fileOutput.d_MODE = 2;  //Weak planner mode
                else if (PowerPlannerMode.Checked)
                    _fileOutput.d_MODE = 3;   // Powerful planner mode
                else if (PowerPlannerMode2.Checked)
                    _fileOutput.d_MODE = 4;
                else
                {
                    Status.Text = "Select mode!";
                    return;
                }
                RunButton.Text = "STOP";
                // Спочатку очищаємо всі дані
                Info1.Text = Info2.Text = Info3.Text = Info4.Text = Info5.Text = "0";
                AllTasks.Clear();
                AddedTasksBox.Clear();
                _fileOutput.d_tenSecondsCount = 0;
                for (int i = 0; i < 5; i++)
                    Processors[i] = null;
                // Вмикаємо таймери
                t_time.Interval = 1;
                t_time.Start();
                t_interrupt.Interval = 1;
                t_interrupt.Start();
                t_informer.Interval = 10000;
                t_informer.Start();
                // Знімаємо дані для статистики
                Status.Text = "Click to start";
                _fileOutput.d_Start = DateTime.Now;
                StatisticsBox.Clear();
                StatisticsBox.Text = "Started at " + "\t" + _fileOutput.d_Start.Hour + ":" + _fileOutput.d_Start.Minute + ":" + _fileOutput.d_Start.Second +
                    ":" + _fileOutput.d_Start.Millisecond + Environment.NewLine;
                try
                {
                    // Зчитуємо введене користувачем
                    _userInput.d_percent = Convert.ToDouble(TaskProbability.Text);
                    _userInput.d_LowComplexity = Convert.ToInt32(LowScope.Text);
                    _userInput.d_HighComplexity = Convert.ToInt32(HighScope.Text);
                    // Перевіряємо чи користувач не лась
                    if (_userInput.d_LowComplexity > _userInput.d_HighComplexity)
                    {
                        RunButton.Text = "RUN";
                        Status.Text = "Wrong data";
                        t_time.Stop();
                        t_interrupt.Stop();
                        t_informer.Stop();
                        return;
                    }
                    // Створюємо потоки
                    Processors[0] = new Processor(Convert.ToInt16(Productivity1.Text), Info1);
                    Processors[1] = new Processor(Convert.ToInt16(Productivity2.Text), Info2);
                    Processors[2] = new Processor(Convert.ToInt16(Productivity3.Text), Info3);
                    Processors[3] = new Processor(Convert.ToInt16(Productivity4.Text), Info4);
                    Processors[4] = new Processor(Convert.ToInt16(Productivity5.Text), Info5);
                    // Знаходимо сильніший і слабший (треба для алгоритму)
                    _fileOutput.d_weakestId = getWeakestProcessorID();
                    _fileOutput.d_strongestId = getStrongestProcessorID();
                    // Перевірка по умові виконання найслабшим процесором задачі протягом від 10 до 200мс
                    if (_userInput.d_HighComplexity / Processors[_fileOutput.d_weakestId].getProductivity() > 200.0)
                    {
                        _userInput.d_LowComplexity = _userInput.d_HighComplexity = Processors[_fileOutput.d_weakestId].getProductivity() * 200;
                        LowScope.Text = HighScope.Text = Convert.ToString(_userInput.d_LowComplexity);
                    }
                    if(_userInput.d_LowComplexity / Processors[_fileOutput.d_weakestId].getProductivity() < 10.0)
                    {
                        _userInput.d_LowComplexity = _userInput.d_HighComplexity = Processors[_fileOutput.d_weakestId].getProductivity() * 10;
                        LowScope.Text = HighScope.Text = Convert.ToString(_userInput.d_HighComplexity);
                    }
                    // Запускаємо необхідний режим
                    if (_fileOutput.d_MODE == 1)
                        FIFO_Mode();
                    else if (_fileOutput.d_MODE == 2)
                        WeakPlanner_Mode();
                    else if (_fileOutput.d_MODE == 3)
                        PowerFulPlanner_Mode(20);
                    else if (_fileOutput.d_MODE == 4)
                        PowerFulPlanner_Mode(43);

                }
                catch   // Якщо знайдена помилка введення
                {
                    RunButton.Text = "RUN";  
                    Status.Text = "Wrong data";
                    t_time.Stop();
                    t_interrupt.Stop();
                    t_informer.Stop();
                }
            }
            else
            {
                // Якщо нажали STOP
                _fileOutput.d_MODE = 0; // Припиняємо виконання режиму
                t_time.Stop();          // Зупиняємо всі-всі-всі
                t_interrupt.Stop();     //           всі-всі-всі  
                t_informer.Stop();      //           всі таймери
                RunButton.Text = "RUN"; // Кнопка знову готова до запуску
                // Зчитуємо дані для статистики
                StatisticsBox.AppendText("Finished at " + "\t" +  DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second +
                    ":" + DateTime.Now.Millisecond + Environment.NewLine);
                StatisticsBox.AppendText("Session time    " + DateTime.Now.Subtract(_fileOutput.d_Start) + Environment.NewLine);
                StatisticsBox.AppendText("Performed " + "\t" + this.getPerformedTasksAmount() + Environment.NewLine);
            }
            
        }
        private void t_Tick(object sender, EventArgs e)  // Переривання для виведення системного часу
        {
            int hh = DateTime.Now.Hour;
            int mm = DateTime.Now.Minute;
            int ss = DateTime.Now.Second;
            int ms = DateTime.Now.Millisecond;
            string time = "";

            if (hh < 10) time += "0" + hh;
            else time += hh;
            time += ":";
            if (mm < 10) time += "0" + mm;
            else time += mm;
            time += ":";
            if (ss < 10) time += "0" + ss;
            else time += ss;
            time += ":";
            if (ms < 10) time += "0" + ms;
            else time += ms;

            TIMER_FIELD.Text = time;
        }

        private void t_Int(object sender, EventArgs e)  // Переривання для додавання в кожну 1мс випадкового завдання
        {
            // Додаємо задачу із певною імовірністю появи в дану мілісекунду
            if (new Random().Next(0, 100) < _userInput.d_percent)
            {
                List<int> proc_list = new List<int> {0, 1, 2, 3, 4};
                Random rand_digit = new Random();
                for (int i = 0; i < 4; i++) 
                {
                    int new_value = rand_digit.Next(0, 5);
                    if (proc_list.Contains(new_value))
                        proc_list.Remove(new_value);            // 
                }
                /*for (int i = 0; i < proc_list.Count; i++)
                {
                    AddedTasksBox.AppendText(proc_list[i] + " ");
                }
                AddedTasksBox.AppendText(Environment.NewLine);*/
                //if (proc_list.Count == 1) AddedTasksBox.AppendText(proc_list[0] + Environment.NewLine);
                AllTasks.Add(new TasK(proc_list, _userInput.d_LowComplexity, _userInput.d_HighComplexity));
            }
            TasksAmount.Text = Convert.ToString(AllTasks.Count);            
        }
        private void t_info(object sender, EventArgs e) // Переривання кожних 10сек для зняття показників
        {
            _fileOutput.d_tenSecondsCount++;
            StatisticsBox.AppendText("Info written to file ..." + Environment.NewLine);
            StatisticsBox.AppendText("Theory efficiency = " + getTheoryEfficiency() + Environment.NewLine);
            StatisticsBox.AppendText("Real efficiency = " + getRealEfficiency() + Environment.NewLine);

            using (StreamWriter writer = new StreamWriter("Statistics.txt", true))
            {
                switch (_fileOutput.d_MODE)
                {
                    case 1:
                        writer.Write("\tFIFO mode: " + Environment.NewLine);
                        break;
                    case 2:
                        writer.Write("\tWeak planner mode: " + Environment.NewLine);
                        break;
                    case 3:
                        writer.Write("\tPowerful planner mode, case a): " + Environment.NewLine);
                        break;
                    case 4:
                        writer.Write("\tPowerful planner mode, case b): " + Environment.NewLine);
                        break;
                }
                writer.Write("Started at:\t\t\t" + _fileOutput.d_Start + Environment.NewLine);
                writer.Write("Low scope: " + _userInput.d_LowComplexity + Environment.NewLine);
                writer.Write("High scope: " + _userInput.d_HighComplexity + Environment.NewLine);
                writer.Write("Performed tasks by system:\t" + this.getPerformedTasksAmount() + Environment.NewLine);
                writer.Write("Performed operations by system:\t" + this.getPerformedOperationsAmount() + Environment.NewLine);
                writer.Write("Energy conversion efficiency (η):\t" + getTheoryEfficiency() + Environment.NewLine);
                writer.Write("Real efficiency (η): \t" + getRealEfficiency() + Environment.NewLine + Environment.NewLine);
            }
        }

        private async void FIFO_Mode()
        {
            while (_fileOutput.d_MODE == 1)
            {
                if (AllTasks.Count < 1)
                {
                    await Task.Delay(1);
                    continue;
                }

                int possibleProcSize = AllTasks[0].getRequiredProcessors().Count;
                for(int i=0; i < possibleProcSize; i++)
                {
                    int i_th_processor = AllTasks[0].getRequiredProcessors()[i];
                    if (!Processors[i_th_processor].isBusy())
                    {
                        Processors[i_th_processor].setTask(AllTasks[0]);
                        AddedTasksBox.AppendText("  Processor " + Convert.ToInt16(i_th_processor+1) + " taken N=" + 
                            AllTasks[0].getOperationAmount() + " at " + DateTime.Now.ToString("hh:mm:ss:ms") + Environment.NewLine);
                        AllTasks.RemoveAt(0);
                        Processors[i_th_processor].executeTask();
                        break;
                    }
                    /*if (i == possibleProcSize - 1)
                        AddedTasksBox.AppendText("Not found" + Environment.NewLine);*/
                }
                await Task.Delay(1);
            }
        }
        private async void WeakPlanner_Mode()
        {
            int index = _fileOutput.d_weakestId;              // Знаходимо найслабший
            while (_fileOutput.d_MODE == 2)
            {
                if (AllTasks.Count < 1)     // Перевірка чи є задачі в черзі
                {
                    await Task.Delay(1);
                    continue;
                }               

                for (int i = 0; i < 5; i++)
                {
                    if (i == index || Processors[i].isBusy())     // Якщо це найслабший процесор, або даний процесор зайнятий,
                        continue;                                 // то пропускаємо планування задач для нього
                                
                    int j = 0;
                    while (AllTasks.Count > j && !AllTasks[j].getRequiredProcessors().Contains(i))    // Шукаємо завдання для кожного процесора
                        j++;

                    if(AllTasks.Count > j)                        // Якщо j в правильних межах 
                    {
                        Processors[i].setTask(AllTasks[j]);
                        AddedTasksBox.AppendText("  Processor " + Convert.ToInt16(i+1) + " taken complexity=" + 
                            AllTasks[0].getOperationAmount() + " at " + DateTime.Now.ToString("hh:mm:ss:ms") + Environment.NewLine);
                        AllTasks.RemoveAt(j);
                        Processors[i].executeTask();
                    }
                }
                await Task.Delay(1);
            }
        }

        private async void PowerFulPlanner_Mode(int plannerTimeForTasks)
        {
            int index = _fileOutput.d_strongestId; //getStrongestProcessorID(); // Індекс найсильнішого процесора
            while (_fileOutput.d_MODE == 3 || _fileOutput.d_MODE == 4)
            {
                if (AllTasks.Count < 1 || Processors[index].isBusy()) // Якщо є задачі і якщо планувальник звільнився після виконання
                {
                    await Task.Delay(1);
                    continue;
                }

                for (int i = 0; i < 5; i++)
                {
                    if (Processors[i].isBusy())  // Якщо даний процесор зайнятий, то йому нічого не плануємо, навіть якщо це планувальник
                        continue;

                    int j = 0;
                    while (AllTasks.Count > j && !AllTasks[j].getRequiredProcessors().Contains(i))
                        j++;

                    if(AllTasks.Count > j)  // Якщо знайшли
                    {
                        if (i == index)     // якщо задача адресована планувальнику
                        {
                            if (Processors[index].hasNoFinishedTasks()) // Якщо процесор-планувальник має незавершене своє завдання
                                Processors[index].executeTask(plannerTimeForTasks);
                            else
                            {
                                Processors[index].setTask(AllTasks[j]);
                                AllTasks.RemoveAt(j);
                                Processors[index].executeTask(plannerTimeForTasks);
                            }
                        }
                        else                // якщо задача адресована НЕ планувальнику
                        {
                            Processors[i].setTask(AllTasks[j]);
                            AddedTasksBox.AppendText("  Processor " + Convert.ToInt16(i+1) + " taken complexity=" + AllTasks[0].getOperationAmount() +
                                    " at " + DateTime.Now.ToString("hh:mm:ss:ms") + Environment.NewLine);
                            AllTasks.RemoveAt(j);
                            Processors[i].executeTask();
                        }
                    }
                }
                await Task.Delay(4);    // 4мс відбувається планування
            }
        }

        private int getWeakestProcessorID()
        {
            int index = 0;
            int MinProductivity = Processors[0].getProductivity();
            for (int i = 1; i < 5; i++) // Обираємо найслабший процесор
            {
                if (Processors[i].getProductivity() < MinProductivity)
                {
                    MinProductivity = Processors[i].getProductivity();
                    index = i;  // і-ий процесор - планувальник
                }
            }
            return index;
        }
        private int getStrongestProcessorID()
        {
            int index = 0;
            int MaxProductivity = Processors[0].getProductivity();
            for (int i = 1; i < 5; i++) // Обираємо сильніший процесор
            {
                if (Processors[i].getProductivity() > MaxProductivity)
                {
                    MaxProductivity = Processors[i].getProductivity();
                    index = i;  // i-ий процесор є планувальником
                }
            }
            return index;
        }
        private int getPerformedTasksAmount()
        {
            int sum = 0;    // Read amount of performed tasks
            for (int i = 0; i < 5; i++)
                sum += Processors[i].getPerformedTasksAmount();
            return sum;
        }
        private int getPerformedOperationsAmount()
        {
            int sum = 0;
            for (int i = 0; i < 5; i++)
            {
                sum += Processors[i].getPerformedOperationsAmount();
            }
            return sum;
        }
        private float getTheoryEfficiency()
        {
            int N_max = 0;
            for (int i = 0; i < 5; i++)
            {
                N_max += Processors[i].getProductivity();
            }
            N_max *= 10000 * _fileOutput.d_tenSecondsCount; // 10 seconds = 10000 msec
            return (float) getPerformedOperationsAmount() / N_max;
        }
        private float getRealEfficiency()
        {
            int N_max = 0;

            switch (_fileOutput.d_MODE)
            {
                case 1:
                    for (int i = 0; i < 5; i++)
                        N_max += Processors[i].getProductivity() * 10000 * _fileOutput.d_tenSecondsCount;
                    break;
                case 2:
                    int weakIndex = _fileOutput.d_weakestId; // getWeakestProcessorID();
                    for (int i = 0; i < 5; i++)
                    {
                        if (i == weakIndex)
                            continue;
                        N_max += Processors[i].getProductivity() * 10000 * _fileOutput.d_tenSecondsCount;
                    }
                    break;
                case 3:
                    int strongIndex = _fileOutput.d_strongestId;// getStrongestProcessorID();
                    for (int i = 0; i < 5; i++)
                    {
                        if (i == strongIndex)
                        {
                            N_max += Convert.ToInt32(Processors[strongIndex].getProductivity() * 20/24 * 10000 * _fileOutput.d_tenSecondsCount);
                            continue;
                        }
                        N_max += Processors[i].getProductivity() * 10000 * _fileOutput.d_tenSecondsCount;
                    }
                    break;
                case 4:
                    int strongIndex2 = _fileOutput.d_strongestId; // getStrongestProcessorID();
                    for (int i = 0; i < 5; i++)
                    {
                        if (i == strongIndex2)
                        {
                            N_max += Convert.ToInt32(Processors[strongIndex2].getProductivity() * 43 / 48 * 10000 * _fileOutput.d_tenSecondsCount);
                            continue;
                        }
                        N_max += Processors[i].getProductivity() * 10000 * _fileOutput.d_tenSecondsCount;
                    }
                    break;
            }

            return (float)getPerformedOperationsAmount() / N_max;
        }
    }
}
