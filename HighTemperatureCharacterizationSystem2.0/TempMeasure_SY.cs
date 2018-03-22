using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.DAQmx;
using NationalInstruments;
using System.Data;
using System.Windows;

namespace HighTemperatureCharacterizationSystem2._0
{
    class TempMeasure
    {
        TempData temps;
        tempCallback tcb;
        private AnalogMultiChannelReader analogInReader;
        private AsyncCallback myAsyncCallback;
        private NationalInstruments.DAQmx.Task myTask;
        private NationalInstruments.DAQmx.Task runningTask;
        private AnalogWaveform<double>[] data;
        //private DataColumn[] dataColumn = null;
        private DataTable dataTable = null;

        public TempMeasure()
        {
            myAsyncCallback = new AsyncCallback(AnalogInCallback);
            dataTable = new DataTable();
            temps = new TempData();
        }
        public void AnalogInCallback(IAsyncResult ar)
        {
            try
            {
                if (runningTask != null && runningTask == ar.AsyncState)
                {
                    data = analogInReader.EndReadWaveform(ar);                  
                    dataRecord(data, ref dataTable);
                    tcb(datatoShow());
                    //analogInReader.BeginMemoryOptimizedReadWaveform(10, myAsyncCallback, myTask, data);
                    analogInReader.BeginMemoryOptimizedReadWaveform(1, myAsyncCallback, myTask, data);
                }
            }
            catch (DaqException exception)
            {
                MessageBox.Show(exception.Message);
                myTask.Dispose();               
                runningTask = null;
            }
         
        }
        public TempData  datatoShow()
        {
            return temps;
        }
        public void dataRecord(AnalogWaveform<double>[] sourceArray, ref DataTable data)
        {
            double temp1 = 0;
            double temp2 = 0;
            for (int i = 0; i < sourceArray[0].Samples.Count; ++i)
            {
                temp1 += sourceArray[0].Samples[i].Value;

            }
            temp1 /= sourceArray[0].Samples.Count;
            for (int i = 0; i < sourceArray[1].Samples.Count; ++i)
            {
                temp2 += sourceArray[1].Samples[i].Value;

            }
            temp2 /= sourceArray[1].Samples.Count;
            temps.temp1 = Math.Ceiling(temp1*10000)/10000;
            temps.temp2 = Math.Ceiling(temp2 * 10000) / 10000;            
            
        }
        public void startTempReading(tempCallback itcb)
        {         

            try
            {
                tcb = itcb;
                myTask = new NationalInstruments.DAQmx.Task();
                AIChannel channel1;
                AIChannel channel2;
                AIThermocoupleType thermocoupleType;
                
                

                thermocoupleType = AIThermocoupleType.K;
                string[] channellist = DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.AI, PhysicalChannelAccess.External);

                channel1 = myTask.AIChannels.CreateThermocoupleChannel(channellist[1],"", 0,1000, thermocoupleType, AITemperatureUnits.DegreesC);
                channel2 = myTask.AIChannels.CreateThermocoupleChannel(channellist[3], "", 0,1000, thermocoupleType, AITemperatureUnits.DegreesC);
                channel1.AutoZeroMode = AIAutoZeroMode.Once;
                channel2.AutoZeroMode = AIAutoZeroMode.Once;
               /* if (scxiModuleCheckBox.Checked)
                {
                    switch (autoZeroModeComboBox.SelectedIndex)
                    {
                        case 0:
                            autoZeroMode = AIAutoZeroMode.None;
                            break;
                        case 1:
                        default:
                            autoZeroMode = AIAutoZeroMode.Once;
                            break;
                    }
                    myChannel.AutoZeroMode = autoZeroMode;
                }*/

                myTask.Timing.ConfigureSampleClock("",4,
                    SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples, 1000);

                myTask.Control(TaskAction.Verify);

                analogInReader = new AnalogMultiChannelReader(myTask.Stream);

                runningTask = myTask;
                //InitializeDataTable(myTask.AIChannels, ref dataTable);
                //acquisitionDataGrid.DataSource = dataTable;

                // Use SynchronizeCallbacks to specify that the object 
                // marshals callbacks across threads appropriately.
                analogInReader.SynchronizeCallbacks = true;
                analogInReader.BeginReadWaveform(10, myAsyncCallback, myTask);
            }
            catch (DaqException exception)
            {
                //MessageBox.Show(exception.Message);
                myTask.Dispose();               
                runningTask = null;
            }
        }

    }


}
