using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace ComputerSystems
{
    class Processor
    {
        private Label info; // Every processor has its own info box with performed tasks amount
        private int performance = 0, performedTasks = 0, performedOperations = 0;
        private bool busy = false, has_not_finished_tasks = false;
        private TasK executedTask;// = null;

        public Processor(int _productivity, Label _info)
        {
            performance = _productivity;
            info = _info;
        }

        public async void executeTask()
        {
            busy = true;            
            performedTasks++;
            performedOperations += executedTask.getOperationAmount();
            transferText(Convert.ToString(performedTasks));
            await Task.Delay(Convert.ToInt32(executedTask.getOperationAmount() / performance));
            busy = false;
        }

        public async void executeTask(int msecForTask) // Робота процесора-планувальника протягом заданого проміжку часу
        {
            busy = true;
            if (executedTask.getOperationAmount() <= msecForTask * performance) // Якщо за стільки-то мсекунд він встигне виконати завдання
            {
                performedOperations += executedTask.getOperationAmount();
                performedTasks++;
                transferText(Convert.ToString(performedTasks)); // Обновлюємо лічильник виконаних завдань

                await Task.Delay(Convert.ToInt32(executedTask.getOperationAmount() / performance)); // Працює стільки-то мсекунд
                busy = false;
                has_not_finished_tasks = false;
                return;
            }
            // Якщо за стільки-то мсекунд НЕ встигне виконати завдання, а лише певну к-сть операцій
            performedOperations += msecForTask * performance;
            executedTask.setOperationAmount(executedTask.getOperationAmount() - msecForTask * performance);
            await Task.Delay(msecForTask); // Працює стільки-то мсекунд
            has_not_finished_tasks = true;   // Мусить продовжити виконання завдання після чергового планування
            busy = false;   // Вільний до планування завдань
        }
        public int getPerformedTasksAmount() { return performedTasks; }
        public int getPerformedOperationsAmount() { return this.performedOperations; }
        public int getProductivity() { return performance; }
        public void transferText(String text)
        {
            if (info.InvokeRequired)
                info.Invoke((MethodInvoker)delegate()
                {
                    transferText(text);
                });
            else info.Text = text;
        }
        public void setTask(TasK aTask) { executedTask = aTask; }
        public bool isBusy() { return busy; }
        public bool hasNoFinishedTasks() { return has_not_finished_tasks; }
    }

    class TasK
    {
        private int operationAmount;
        private List<int> requiredProcessors = new List<int>();
        public TasK(List<int> _possibleProcessors, int _LowComplexity, int _HighComplexity)
        {
            operationAmount = new Random().Next(_LowComplexity, _HighComplexity);
            requiredProcessors = _possibleProcessors;
        }
        public int getOperationAmount() { return operationAmount; }
        public void setOperationAmount(int _operationAmount) { operationAmount = _operationAmount; }
        public List<int> getRequiredProcessors() { return requiredProcessors; }
    }

    struct _DataType
    {
        public int d_MODE;
        public int d_tenSecondsCount;
        public DateTime d_Start;
        public int d_weakestId, d_strongestId;
    }
    struct _UserInput
    {
        public double d_percent;
        public int d_LowComplexity;
        public int d_HighComplexity;
    }
}
