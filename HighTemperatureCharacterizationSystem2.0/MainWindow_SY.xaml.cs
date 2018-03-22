using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace HighTemperatureCharacterizationSystem2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SwitchCommandCenter visacom1;
        KE2400CommandCenter visacom2;
        KE2000CommandCenter ke2000;
        serialCom sc1;
        TempCommandCenter tcchot;
        TempCommandCenter tcccold;
        BackgroundWorker Mainworker = new BackgroundWorker();
        BackgroundWorker worker = new BackgroundWorker();
        BackgroundWorker worker_stability = new BackgroundWorker();
        BackgroundWorker worker_controllers = new BackgroundWorker();
        TempData Current_temps;
        TempData temps_toShow;
        List<TempData> temparray;
        List<double> coldslopearray;
        List<double> hotslopearray;
        string filePath = "C:\\test\\Temps.xml";
        double[] slopes=new double[2];
        bool check=false;
        bool writecheck = false;
        double[] xVals ;
        double[] y1Vals ;
        double[] y2Vals;
        List<double> xValsList = new List<double>();
        List<double> y1ValsList = new List<double>();
        List<double> y2ValsList = new List<double>();
        //FileStream fs;
       // System.IO.StreamWriter writer;
        System.IO.StreamWriter writer3;
        int secondstocheckslope = 10;
        int secondstocheckcontrollers = 1;
        double hotsidesetpointwrite = 15;
        double coldsidesetpointwrite = 10;
        double hotsidesetpointread = 10;
        double coldsidesetpointread = 10;
        double hotsideprocvalueread = 0;
        double coldsideprocvalueread = 0;
        private bool hotnonNumberEntered=false;
        private bool coldnonNumberEntered = false;
        private bool contollersUpdated = false;
        private bool controllerRead = false;
        private string controllerMesg = "";
        private bool controlMesgUpdated = false;
        public double[] measureTemps={100,125,150,175,200};
        private double[] measureTempshot;
        ////private double[] measureTemps;
        public double seebckTempInterval = 20;
        private bool errorflag = false;
        public string errorstring = "";
        int slopearray = 0;
        string ECdatafile = "";
        string SCdatafile = "";
        string LogFile = "";
        public int  averaging{get;set;}
        private string mesg = "";
        public double[] hotsideoffset;
        public double[] coldsideoffset;
        public MainWindow()
        {    

            //saveFilePath.Content = pathString;
           // System.IO.Directory.CreateDirectory(pathString);
            //MessageBox.Show("Burada");
            InitializeComponent();
            string path = "";
            // Configure open file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Title = "Save Log File Location";
            dlg.InitialDirectory = "C:\\data\\hts";
            dlg.FileName = "LogFile"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            //dlg.Filter = "Comma Seperated Values (.csv)|*.csv"; // Filter files by extension 
            dlg.Filter = "Text File (.txt)|*.txt";
            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                path = dlg.FileName.Remove(dlg.FileName.LastIndexOf("\\"));
            }
            else
            {
                path = "C:\\data\\hts\\default";
            }
            /*System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = fbd.ShowDialog();

            string[] path = Directory.GetFiles(fbd.SelectedPath);*/
            string subfolder = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss");
            string pathString = System.IO.Path.Combine(path, subfolder);
            System.IO.Directory.CreateDirectory(pathString);
            saveFilePath.Content = pathString.ToString();// pathString;
            ECdatafile =System.IO.Path.Combine(pathString, "ECData.csv");
            SCdatafile = System.IO.Path.Combine(pathString, "SCData.csv");
            LogFile = System.IO.Path.Combine(pathString, "Log.txt"); 
            Mainworker.WorkerReportsProgress= true;
            Mainworker.DoWork += Mainworker_DoWork;
            Mainworker.ProgressChanged += Mainworker_ProgressChanged;
            worker.WorkerReportsProgress = true;
            worker.DoWork+=new DoWorkEventHandler(worker_doWork);
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            worker_stability.WorkerReportsProgress = true;
            worker_stability.DoWork += new DoWorkEventHandler(worker_stability_doWork);
            worker_stability.ProgressChanged += new ProgressChangedEventHandler(worker_stability_ProgressChanged);
            worker_controllers.WorkerReportsProgress = true;
            worker_controllers.DoWork += new DoWorkEventHandler(worker_controllers_doWork);
            worker_controllers.ProgressChanged += new ProgressChangedEventHandler(worker_controllers_ProgressChanged);
            Current_temps = new TempData();
            temps_toShow = new TempData();
            temparray = new List<TempData>();
            coldslopearray=new List<double>();
            hotslopearray = new List<double>();
            averaging = 100;            
            xVals = new double[averaging + 1];
            y1Vals = new double[averaging + 1];
            y2Vals = new double[averaging + 1];
            visacom1 = new SwitchCommandCenter();
            tcchot = new TempCommandCenter();
            tcccold = new TempCommandCenter();
            //MessageBox.Show("Burada3");
            TempMeasure tmm = new TempMeasure();
            tempCallback tcb = new tempCallback(tempCallbackReader);
            tmm.startTempReading(tcb);
            startTempReading();
            //testOutput.Text += "\n" + "Tempreature Reading is Started\n";
            hotContTemp.Text = hotsidesetpointread.ToString();
            coldContTemp.Text = coldsidesetpointread.ToString();
            startControllerReading();
            //MessageBox.Show("Burada4");
            //fs = new FileStream("C:\\test\\templog.csv",FileMode.OpenOrCreate);
            //writer3=new System.IO.StreamWriter("C:\\test\\tempslopelog.csv");    
           //hotsideoffset = new double[measureTempsPoints.Length];
            //coldsideoffset = new double[measureTempsPoints.Length];
           // measureTempshot = new double[measureTempsPoints.Length];
           // measureTempscold = new double[measureTempsPoints.Length];

        }
        private bool stabilizeTemp(double measureTempsHot,double measureTempsCold, double precision, 
            double incrementhot, double incrementcold,double decrementhot,double decrementcold,
            BackgroundWorker bgk)
        {
            bool waitforStabilize=true;
            while(waitforStabilize)
            {
                if (hotsidesetpointwrite > 0.8 * measureTempsHot// && hotsidesetpointwrite < 1.5 * measureTempsHot
                               && coldsidesetpointwrite > 0.8 * measureTempsCold //&& coldsidesetpointwrite < 1.5 * measureTempsCold
                               && temps_toShow.temp2 > (measureTempsHot - precision) && temps_toShow.temp2 < (measureTempsHot + precision)
                               && temps_toShow.temp1 > (measureTempsCold - precision) && temps_toShow.temp1 < (measureTempsCold + precision))
             {
                mesg = "Temperature Stabilized " + temps_toShow.temp2;
                bgk.ReportProgress(20);
                waitforStabilize = false;
                //return true;
                }
             //situation  -<5 -<5
             else if (hotsidesetpointwrite > 0.8 * measureTempsHot// && hotsidesetpointwrite < 1.5 * measureTempsHot
             && coldsidesetpointwrite > 0.8 * measureTempsCold //&& coldsidesetpointwrite < 1.5 * measureTempsCold
             && temps_toShow.temp2 < (measureTempsHot - precision) && temps_toShow.temp1 < (measureTempsCold - precision))
            {
                mesg = "Trying to increase temperature of both sides for EC Measurement ";
                bgk.ReportProgress(10);
                hotsidesetpointwrite += incrementhot;
                coldsidesetpointwrite += incrementcold;
            }
            //situation  ~5 -<5
            else if (hotsidesetpointwrite > 0.8 * measureTempsHot //&& hotsidesetpointwrite < 1.5 * measureTempsHot
           && coldsidesetpointwrite > 0.8 * measureTempsCold //&& coldsidesetpointwrite < 1.5 * measureTempsCold
            && temps_toShow.temp2 < (measureTempsHot - precision) && temps_toShow.temp1 > (measureTempsHot - precision)
            && temps_toShow.temp1 < (measureTempsCold + precision))
            {
                mesg = "Trying to increase temperature of hot side for EC Measurement ";
                bgk.ReportProgress(10);
                hotsidesetpointwrite += incrementhot;
                //coldsidesetpointwrite += 5;
            }//situation  -<5 ~5
            else if (hotsidesetpointwrite > 0.8 * measureTempsHot //&& hotsidesetpointwrite < 1.5 * measureTempsHot
             && coldsidesetpointwrite > 0.8 * measureTempsCold //&& coldsidesetpointwrite < 1.5 * measureTempsCold
             && temps_toShow.temp1 < (measureTempsCold - precision) && temps_toShow.temp2 > (measureTempsHot - precision)
             && temps_toShow.temp2 < (measureTempsHot + precision))
            {
                bgk.ReportProgress(10);
                mesg = "Trying to increase temperature of cold side for EC Measurement ";
                // hotsidesetpointwrite += 5;
                coldsidesetpointwrite += incrementcold;
            }
            //situation  +<5 +<5
            else if (hotsidesetpointwrite > 0.8 * measureTempsHot //&& hotsidesetpointwrite < 1.5 * measureTempsHot
             && coldsidesetpointwrite > 0.8 * measureTempsCold //&& coldsidesetpointwrite < 1.5 * measureTempsCold
             && temps_toShow.temp2 > (measureTempsHot + precision) && temps_toShow.temp1 > (measureTempsCold + precision))
            {
                mesg = "Trying to decrease temperature of both sides for EC Measurement ";
                bgk.ReportProgress(10);
                hotsidesetpointwrite -= decrementhot;
                coldsidesetpointwrite -= decrementcold;
            }
            //situation  ~5 +<5
            else if (hotsidesetpointwrite > 0.8 * measureTempsHot //&& hotsidesetpointwrite < 1.5 * measureTempsHot
           && coldsidesetpointwrite > 0.8 * measureTempsCold //&& coldsidesetpointwrite < 1.5 * measureTempsCold
            && temps_toShow.temp2 > (measureTempsHot + precision) && temps_toShow.temp1 < (measureTempsCold + precision)
            && temps_toShow.temp1 > (measureTempsCold - precision))
            {
                mesg = "Trying to decrease temperature of hot side for EC Measurement ";
                bgk.ReportProgress(10);
                hotsidesetpointwrite -= decrementhot;
            }  //situation  +<5 ~5
            else if (hotsidesetpointwrite > 0.8 * measureTempsHot// && hotsidesetpointwrite < 1.5 * measureTempsHot
           && coldsidesetpointwrite > 0.8 * measureTempsCold //&& coldsidesetpointwrite < 1.5 * measureTempsCold
            && temps_toShow.temp1 > (measureTempsCold + precision) && temps_toShow.temp2 < (measureTempsHot + precision)
            && temps_toShow.temp2 > (measureTempsHot - precision))
            {
                mesg = "Trying to decrease temperature of cold side for EC Measurement ";
                bgk.ReportProgress(10);
                //hotsidesetpointwrite -= 2;
                coldsidesetpointwrite -= decrementcold;
            }
            //situation  +<5 -<5
            else if (hotsidesetpointwrite > 0.8 * measureTempsHot //&& hotsidesetpointwrite < 1.5 * measureTempsHot
             && coldsidesetpointwrite > 0.8 * measureTempsCold //&& coldsidesetpointwrite < 1.5 * measureTempsCold
             && temps_toShow.temp1 > (measureTempsCold + precision) && temps_toShow.temp2 < (measureTempsHot - precision))
            {
                mesg = "Trying to decrease temperature of cold side \n and increase hot side for EC Measurement ";
                bgk.ReportProgress(10);
                hotsidesetpointwrite += incrementhot;
                coldsidesetpointwrite -= decrementcold;
            }
            //situation  -<5 +<5
            else if (hotsidesetpointwrite > 0.8 * measureTempsHot// && hotsidesetpointwrite < 1.5 * measureTempsHot
             && coldsidesetpointwrite > 0.8 * measureTempsCold //&& coldsidesetpointwrite < 1.5 * measureTempsCold
             && temps_toShow.temp2 > (measureTempsHot + precision) && temps_toShow.temp1 < (measureTempsCold - precision))
            {
                mesg = "Trying to decrease temperature of hot side \n and increase cold side for EC Measurement ";
                bgk.ReportProgress(10);
                hotsidesetpointwrite -= decrementhot ;
                coldsidesetpointwrite += incrementcold;
            }
            else
            {
                mesg = "Trying to set temperatures ";
                bgk.ReportProgress(10);
                hotsidesetpointwrite = measureTempsHot;
                coldsidesetpointwrite = measureTempsCold;
            }
                bgk.ReportProgress(32);
                contollersUpdated = true;
                while (contollersUpdated)
                {
                    System.Threading.Thread.Sleep(1000);
                }
                int waitEquiTemp = 0;
                while (waitEquiTemp < 100 && waitforStabilize)
                {
                    mesg = "Waiting for Equilibrium for " + (100 - waitEquiTemp) * 6 + " seconds ";
                    bgk.ReportProgress(25);
                    System.Threading.Thread.Sleep(6000);
                    waitEquiTemp++;
                }
            }

            return true;
        }

        void Mainworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
           // throw new NotImplementedException();
            //progressBar.Value = e.ProgressPercentage;
            //testOutput.Text = mesg;
            status.Content = mesg;
            if (e.ProgressPercentage == 32)
            {
                setPoints.Content = "Cold Side= " + coldsidesetpointwrite + " Hot Side= " + hotsidesetpointwrite;
            }
            using (StreamWriter writercurr = new StreamWriter(LogFile, true))
            {
                string now = DateTime.Now.ToString();
                writercurr.WriteLine( now + " Message update : " + mesg);
                //writercurr.WriteLine(now + " Temperature cold: "+temps_toShow.temp1+" Temperature hot: "+temps_toShow.temp2);
            }
        }

        private void Mainworker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                BackgroundWorker bgk = (BackgroundWorker)sender;
                double temphotcheck;
                double tempcoldcheck;
                double tempbottomapproacher = 1;
                double temptopapproacher = 1;
                int bottomcounter = 0;
                int topcounter = 0;
                int percentage = (int)(20 / measureTemps.Length);
                int progress = 0;
                string reporttext = "";
                //double hotsideoffset = 0;
                //double coldsideoffset = 0;
                #region
                for (int i = 0; i < measureTemps.Length; i++)
                {
                    mesg = "Started to measure "+measureTemps[i];
                    bgk.ReportProgress(0);
                    bool waitflaghot = true;
                    bool waitflagcold = true;
                    #region
                    while (waitflaghot && waitflagcold)
                    {                        
                        mesg = "Started to read the controllers";
                        bgk.ReportProgress(0);                       
                        controllerRead = true;
                        while (controllerRead)
                        {                            
                            System.Threading.Thread.Sleep(1000);
                            mesg = "Waiting for the controllers";
                            bgk.ReportProgress(0);
                        }
                        if (i == 0)
                        {
                            if (hotsidesetpointread > (measureTemps[i] - 100) && measureTemps[i] < 600 && hotsidesetpointread < measureTemps[i])
                            {

                                hotsidesetpointwrite = measureTemps[i];

                            }
                            else if (hotsidesetpointread < measureTemps[i] - 100 && measureTemps[i] < 600)
                            {
                                hotsidesetpointwrite += 75;
                            }
                            else if (hotsidesetpointread > measureTemps[i])
                            {
                                hotsidesetpointwrite = measureTemps[i];
                            }
                            else if (hotsidesetpointread > 0.8 * measureTemps[i] && hotsidesetpointread < 1.5 * measureTemps[i])
                            {
                                hotsidesetpointwrite = measureTemps[i];
                                waitflaghot = false;
                            }
                            if (coldsidesetpointread > (measureTemps[i] - 100) && measureTemps[i] < 600 && coldsidesetpointread < measureTemps[i])
                            {
                                coldsidesetpointwrite = measureTemps[i];

                            }
                            else if (coldsidesetpointread < measureTemps[i] - 100 && measureTemps[i] < 600)
                            {
                                coldsidesetpointwrite += 75;
                            }
                            else if (coldsidesetpointread > measureTemps[i])
                            {
                                coldsidesetpointwrite = measureTemps[i];
                            }
                            else if (coldsidesetpointread > 0.8 * measureTemps[i] && coldsidesetpointread < 1.5 * measureTemps[i])
                            {
                                coldsidesetpointwrite = measureTemps[i];
                                waitflagcold = false;
                            }
                            mesg = "Setting Tempreature to: " + hotsidesetpointwrite + " " + coldsidesetpointwrite;
                            bgk.ReportProgress(0);
                        }
                        else
                        {
                            waitflagcold = false;
                            waitflaghot = false;
                        }
                        bgk.ReportProgress(32);
                        contollersUpdated = true;
                        while (contollersUpdated)
                        {
                            System.Threading.Thread.Sleep(1000);
                            mesg = "Waiting for the controllers";
                            bgk.ReportProgress(0);
                        }
                    }
                    #endregion
                    bool waitflaghotSC = true;
                    bool waitflagcoldSC = true;
                    bool waitflagEC = true;
                    int stepcounter = 0;
                    double[] temperatures=new double[2];
                    while (waitflaghotSC)
                    {
                        bool SCMeasured = true;
                        controllerRead = true;
                        bool waitforStabilize = true;
                        #region
                        while (waitflagEC)
                        {
                            mesg = "Trying to stabilize temperature for EC Measurement ";
                            bgk.ReportProgress(10);
                            #region
                            //situation ~5 ~5
                           // if (hotsidesetpointwrite > 0.8 * measureTemps[i] && hotsidesetpointwrite < 1.5 * measureTemps[i]
                           //    && coldsidesetpointwrite > 0.8 * measureTemps[i] && coldsidesetpointwrite < 1.5 * measureTemps[i]
                           //    && temps_toShow.temp2 > (measureTemps[i] - 5) && temps_toShow.temp2 < (measureTemps[i] + 5)
                           //    && temps_toShow.temp1 > (measureTemps[i] - 5) && temps_toShow.temp1 < (measureTemps[i] + 5))
                           // {
                           //     mesg = "Measuring EC at " + temps_toShow.temp2;
                           //     bgk.ReportProgress(20);
                                
                           //    // coldsideoffset = coldsidesetpointwrite - measureTemps[0];
                           //    // hotsideoffset = hotsidesetpointwrite - measureTemps[0];
                           //     measureEC();
                           //     waitflagEC = false;
                           //     waitforStabilize = false;
                           // }                          
                           // //situation  -<5 -<5
                           // else if (hotsidesetpointwrite > 0.8 * measureTemps[i] && hotsidesetpointwrite < 1.5 * measureTemps[i]
                           //  && coldsidesetpointwrite > 0.8 * measureTemps[i] && coldsidesetpointwrite < 1.5 * measureTemps[i]
                           //  && temps_toShow.temp2 < (measureTemps[i] - 5) && temps_toShow.temp1 < (measureTemps[i] - 5))
                           // {
                           //     mesg = "Trying to increase temperature of both sides for EC Measurement ";
                           //     bgk.ReportProgress(10);
                           //     hotsidesetpointwrite += 5;
                           //     coldsidesetpointwrite += 5;
                           // }
                           // //situation  ~5 -<5
                           //     else if (hotsidesetpointwrite > 0.8 * measureTemps[i] && hotsidesetpointwrite < 1.5 * measureTemps[i]
                           //    && coldsidesetpointwrite > 0.8 * measureTemps[i] && coldsidesetpointwrite < 1.5 * measureTemps[i]
                           //     && temps_toShow.temp2 < (measureTemps[i] - 5) && temps_toShow.temp1 >( measureTemps[i] - 5)
                           //     && temps_toShow.temp1 < (measureTemps[i] + 5))
                           // {
                           //     mesg = "Trying to increase temperature of hot side for EC Measurement ";
                           //     bgk.ReportProgress(10);
                           //     hotsidesetpointwrite += 5;
                           //     //coldsidesetpointwrite += 5;
                           // }//situation  -<5 ~5
                           // else if (hotsidesetpointwrite > 0.8 * measureTemps[i] && hotsidesetpointwrite < 1.5 * measureTemps[i]
                           //  && coldsidesetpointwrite > 0.8 * measureTemps[i] && coldsidesetpointwrite < 1.5 * measureTemps[i]
                           //  && temps_toShow.temp1 < (measureTemps[i] - 5) && temps_toShow.temp2 > (measureTemps[i] - 5)
                           //  && temps_toShow.temp2 < (measureTemps[i] + 5))
                           // {
                           //     bgk.ReportProgress(10);
                           //     mesg = "Trying to increase temperature of cold side for EC Measurement ";
                           //     // hotsidesetpointwrite += 5;
                           //     coldsidesetpointwrite += 5;
                           // }
                           // //situation  +<5 +<5
                           // else if (hotsidesetpointwrite > 0.8 * measureTemps[i] && hotsidesetpointwrite < 1.5 * measureTemps[i]
                           //  && coldsidesetpointwrite > 0.8 * measureTemps[i] && coldsidesetpointwrite < 1.5 * measureTemps[i]
                           //  && temps_toShow.temp2 > (measureTemps[i] + 5) && temps_toShow.temp1 > (measureTemps[i] + 5))
                           // {
                           //     mesg = "Trying to decrease temperature of both sides for EC Measurement ";
                           //     bgk.ReportProgress(10);
                           //     hotsidesetpointwrite -= 2;
                           //     coldsidesetpointwrite -= 2;
                           // }
                           // //situation  ~5 +<5
                           // else if (hotsidesetpointwrite > 0.8 * measureTemps[i] && hotsidesetpointwrite < 1.5 * measureTemps[i]
                           //&& coldsidesetpointwrite > 0.8 * measureTemps[i] && coldsidesetpointwrite < 1.5 * measureTemps[i]
                           // && temps_toShow.temp2 > (measureTemps[i] + 5) && temps_toShow.temp1 < (measureTemps[i] + 5)
                           // && temps_toShow.temp1 > (measureTemps[i] - 5))
                           // {
                           //     mesg = "Trying to decrease temperature of hot side for EC Measurement ";
                           //     bgk.ReportProgress(10);
                           //     hotsidesetpointwrite -= 2;
                           // }  //situation  +<5 ~5
                           // else if (hotsidesetpointwrite > 0.8 * measureTemps[i] && hotsidesetpointwrite < 1.5 * measureTemps[i]
                           //&& coldsidesetpointwrite > 0.8 * measureTemps[i] && coldsidesetpointwrite < 1.5 * measureTemps[i]
                           // && temps_toShow.temp1 > (measureTemps[i] + 5) && temps_toShow.temp2 < (measureTemps[i] + 5)
                           // && temps_toShow.temp2 > (measureTemps[i] - 5))
                           // {
                           //     mesg = "Trying to decrease temperature of cold side for EC Measurement ";
                           //     bgk.ReportProgress(10);
                           //     //hotsidesetpointwrite -= 2;
                           //     coldsidesetpointwrite -= 2;
                           // }
                           // //situation  +<5 -<5
                           // else if (hotsidesetpointwrite > 0.8 * measureTemps[i] && hotsidesetpointwrite < 1.5 * measureTemps[i]
                           //  && coldsidesetpointwrite > 0.8 * measureTemps[i] && coldsidesetpointwrite < 1.5 * measureTemps[i]
                           //  && temps_toShow.temp1 > (measureTemps[i] + 5) && temps_toShow.temp2 < (measureTemps[i] - 5))
                           // {
                           //     mesg = "Trying to decrease temperature of cold side \n and increase hot side for EC Measurement ";
                           //     bgk.ReportProgress(10);
                           //     hotsidesetpointwrite +=5;
                           //     coldsidesetpointwrite -= 2;
                           // }
                           // //situation  -<5 +<5
                           // else if (hotsidesetpointwrite > 0.8 * measureTemps[i] && hotsidesetpointwrite < 1.5 * measureTemps[i]
                           //  && coldsidesetpointwrite > 0.8 * measureTemps[i] && coldsidesetpointwrite < 1.5 * measureTemps[i]
                           //  && temps_toShow.temp2 > (measureTemps[i] + 5) && temps_toShow.temp1 < (measureTemps[i] - 5))
                           // {
                           //     mesg = "Trying to decrease temperature of hot side \n and increase cold side for EC Measurement ";
                           //     bgk.ReportProgress(10);
                           //     hotsidesetpointwrite -= 2;
                           //     coldsidesetpointwrite += 5;
                           // }
                           // else
                           // {
                           //     mesg = "Trying to set temperatures ";
                           //     bgk.ReportProgress(10);
                           //     hotsidesetpointwrite=measureTemps[i];
                           //     coldsidesetpointwrite=measureTemps[i];
                            // }
                            #endregion
                            temperatures[0] = measureTemps[i];
                            temperatures[1] = measureTemps[i];
                            double precision=5;
                            double incrementhot=5;
                            double incrementcold=5;
                            double decrementhot=2;
                            double decrementcold=2;
                            waitflagEC = !stabilizeTemp(temperatures[0],temperatures[1],precision,
                                incrementhot,incrementcold,decrementhot,decrementcold,bgk);
                            if (!waitflagEC)
                            {
                                measureEC();
                                mesg="Measuring EC at "+temps_toShow.temp2;
                            }  
                        }
                        #endregion
                        while (controllerRead && stepcounter < 2)
                        {
                            System.Threading.Thread.Sleep(1000);
                        }
                        bool waitflaghotSCstep = true;
                        while(waitflaghotSCstep)
                        {
#region
                            //if (hotsidesetpointwrite > 0.8 * measureTemps[i] && hotsidesetpointwrite < 1.5 * measureTemps[i]
                            //   && coldsidesetpointwrite > 0.8 * measureTemps[i] && coldsidesetpointwrite < 1.5 * measureTemps[i]
                            //   && temps_toShow.temp2 > (measureTemps[i] - 5) && temps_toShow.temp2 < (measureTemps[i] + 5)
                            //   && temps_toShow.temp1 > (measureTemps[i] - 5) && temps_toShow.temp1 < (measureTemps[i] + 5))
                            //{
#endregion
                            hotsidesetpointwrite += 2;
                            stepcounter++;
                            mesg = "Increasing temperature of hot side to " + hotsidesetpointwrite;
                            bgk.ReportProgress(27);
                            contollersUpdated = true;
                            bgk.ReportProgress(32);
                            while (contollersUpdated)
                            {
                                System.Threading.Thread.Sleep(1000);
                            }
                            System.Threading.Thread.Sleep(1000);
                            int scsample = 0;
                            using (StreamWriter writer2 = new StreamWriter(SCdatafile, true))
                            {
                                 writer2.WriteLine("Temperature : ," + measureTemps[i]+" dt= "+2*stepcounter);
                            }
                            while (scsample < 25 )//(SCMeasured)
                            {
                                System.Threading.Thread.Sleep(5000);
                                mesg = "Measuring " + scsample + ". sample of SC at " + 
                                    measureTemps[i] + " with dT= " + 2 * stepcounter;
                                bgk.ReportProgress(40);
                                measureSC();
                                scsample++;
                                int x = hotslopearray.Count;
                                if (Math.Abs(hotslopearray.ElementAt(x - 1)) < 50 &&
                                 Math.Abs(hotslopearray.ElementAt(x - 1)) < 50 &&
                                 Math.Abs(hotslopearray.ElementAt(x - 2)) < 50)
                                {
                                    SCMeasured = false;
                                }
                            }
                            if (stepcounter > 3)
                            {
                                waitflaghotSCstep = false;
                                waitflaghotSC = false;
                            }
                        }
                    }
                #endregion
                    System.Threading.Thread.Sleep(5000);                   
                    mesg = "Completed " + measureTemps[i];
                    bgk.ReportProgress(50);
                }
                hotsidesetpointwrite = 10;
                coldsidesetpointwrite = 10;
                contollersUpdated = true;
                bgk.ReportProgress(32);
                while (contollersUpdated)
                {
                    System.Threading.Thread.Sleep(1000);
                }
                mesg = "Completed All !";
                bgk.ReportProgress(100);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.StackTrace);
            }
        }
        private void measureEC()
        {
            double currstart = -0.1;//Ampere
            double currfinal = 0.1;//Ampere
            int numberofreadings = 20;
            double currinterval = (currfinal - currstart) / numberofreadings;
            double[] voltageReading = new double[numberofreadings + 1];
            double[] currentValue = new double[numberofreadings + 1];
            KE2400CommandCenter k24 = new KE2400CommandCenter();           
            KE2000CommandCenter k20 = new KE2000CommandCenter();
            k24.OpenPort();
            k20.OpenPort();
            using (StreamWriter writercurr = new StreamWriter(ECdatafile, true))
            {
                writercurr.WriteLine("/n/nTemperature : " + temps_toShow.temp1 + "," + temps_toShow.temp2);



                for (int i = 0; i < numberofreadings + 1; i++)
                {
                    currentValue[i] = currstart + i * currinterval;
                    k24.setcurr(currentValue[i]);
                    k24.StartOutput();
                    System.Threading.Thread.Sleep(100);
                    voltageReading[i] = k20.readVoltage();
                    k24.StopOutput();
                    writercurr.WriteLine(currentValue[i] + "," + voltageReading[i]);
                }
            }
            LeastSquare lsEC = new LeastSquare();
            lsEC.LeastSquareFit(currentValue, voltageReading, 0, numberofreadings);
            //System.Windows.MessageBox.Show("Resistance is : " + lsEC.slope + " R2 : " + lsEC.goodfit);

        }
        
        private void testButton_Click_1(object sender, RoutedEventArgs e)
        {             
             //dataflush(1);       
            measureEC();
            /*if (visacom2.OpenPort())
            {
                testOutput.Text = visacom2.currmsg + " initialized\n";
            }
            else
            {
                testOutput.Text = "Error\n";
            }
            visacom2.setcurr(0.071);
            visacom2.StartOutput();
            //System.Threading.Thread.Sleep(1000);
            ke2000.OpenPort();
            double v = ke2000.readVoltage();
            visacom2.StopOutput();
            ke2000.OpenPort();
            //double v=ke2000.readVoltage();
            //testOutput.Text += "Voltage: "+v;
            MessageBox.Show("Voltage: " + v);*/
            //double[] xVals = { 0,0.001,0.002,0.003,0.004,0.005,0.007,0.008};
            //double[] yVals = { -0.0023, -0.0019,-0.00162,-0.0013,-0.00073,-0.00043,-0.00013,0.00018};
            //LeastSquare ls = new LeastSquare();
           // ls.LeastSquareFit(xVals,yVals,0,xVals.Length);
           // Console.WriteLine("yintercept= "+ls.yintercept+"\nsloe= "+ls.slope);  

        }
        private void writeSetPointControllers(double tcold,double thot)
        {
            if(tcold<600 && thot<600)
            {
                switchTemmpSide(1);//side 1 is cold side           
                tcccold.setsetvalueI(tcold);
                tcccold.reset();
                switchTemmpSide(2);//side 2 is hot side            
                tcchot.setsetvalueI(thot);
                tcchot.reset();
            }
            
        }
        private double[] readSetPointControllers()
        {
            double[] returndata=new double[2];
            switchTemmpSide(1);//side 1 is hot side           
            returndata[0] = tcccold.getSetValueI();
            tcccold.reset();
            switchTemmpSide(2);//side 2 is cold side       
            returndata[1] = tcchot.getSetValueI();
            tcchot.reset();
            return returndata;
        }
        private double[] readProcValueControllers()
        {
            double[] returndata = new double[2];
            switchTemmpSide(1);//side 1 is hot side           
            returndata[0] = tcccold.getProcValueI();
            tcccold.reset();
            switchTemmpSide(2);//side 2 is cold side           
            returndata[1] =  tcchot.getProcValueI();
            tcchot.reset();
            return returndata;
        }
        private void writeSetPointControllers(double tcold, double thot, BackgroundWorker bkg)
        {
            if (tcold < 600 && thot < 600)
            {
                switchTemmpSide(1);//side 1 is cold side 
                controllerMesg = "Setting Cold Side to " + tcold;
                bkg.ReportProgress(0);
                tcccold.setsetvalueI(tcold);
                controllerMesg = "Resetting Cold Side";
                tcccold.reset();
                switchTemmpSide(2);//side 2 is hot side 
                controllerMesg = "Setting Hot Side to " + thot;
                bkg.ReportProgress(0);
                tcchot.setsetvalueI(thot);
                switchTemmpSide(2);//side 2 is hot side 
                controllerMesg = "Resetting Hot Side";
                tcchot.reset();
            }

        }
        private double[] readSetPointControllers(BackgroundWorker bkg)
        {
            double[] returndata = new double[2];
            switchTemmpSide(1);//side 1 is hot side 
            System.Threading.Thread.Sleep(2000);
            controllerMesg = "Resetting Cold Side";
            bkg.ReportProgress(0);
            tcccold.reset();
            controllerMesg = "Reading Set Point of Cold Side " ;
            bkg.ReportProgress(0);
            returndata[0] = tcccold.getSetValueI();
            
            //tcccold.reset();
            switchTemmpSide(2);//side 2 is cold side 
            System.Threading.Thread.Sleep(2000);
            controllerMesg = "Resetting Hot Side";
            bkg.ReportProgress(0);
            tcchot.reset();
            controllerMesg = "Reading Set Point of Hot Side";
            bkg.ReportProgress(0);
            returndata[1] = tcchot.getSetValueI();
          
            return returndata;
        }
        private double[] readProcValueControllers(BackgroundWorker bkg)
        {
            double[] returndata = new double[2];
            switchTemmpSide(1);//side 1 is hot side 
            System.Threading.Thread.Sleep(2000);
            controllerMesg = "Resetting Cold Side";
            bkg.ReportProgress(0);
            tcccold.reset();
            controllerMesg = "Reading Proc Point of Cold Side";
            bkg.ReportProgress(0);
            returndata[0] = tcccold.getProcValueI();            
            switchTemmpSide(2);//side 2 is cold side  
            System.Threading.Thread.Sleep(2000);
            controllerMesg = "Resetting Hot Side";
            bkg.ReportProgress(0);
            tcchot.reset();
            controllerMesg = "Reading Proc Value of Hot side";
            bkg.ReportProgress(0);
            returndata[1] = tcchot.getProcValueI();           
            return returndata;
        }
        private bool switchTemmpSide(int sideSelector)
        {
            switch (sideSelector)
            {
                case 1:
                    if (visacom1.OpenPort())
                    {
                         //controllerMesg = visacom1.currmsg + " initialized for case 1\n";
                         //controlMesgUpdated = true;
                     }
                    else
                    {
                       // controllerMesg = "Error\n";
                        //controlMesgUpdated = true;
                     }
                     byte[] openSwitches1={201,203,209};
                     byte[] closedSwitches1={217,218,219};
                    if (visacom1.setSwitches(openSwitches1,closedSwitches1) )
                    {
                        //controllerMesg += "switching returned ok for case 1\n";
                       // controlMesgUpdated = true;
                    }
                    else
                    {
                       // controllerMesg = "Error in switching";
                       // controlMesgUpdated = true;
                    }
                    return true;
                    //visacom1.ClosePort();
                case 2:
                    if (visacom1.OpenPort())
                     {
                         //controllerMesg = visacom1.currmsg + " initialized for case 2\n";
                         //controlMesgUpdated = true;
                    }
                    else
                    {
                       // controllerMesg = "Error\n";
                       // controlMesgUpdated = true;
                     }
                    byte[] closedSwitches2={201,203,209};
                    byte[] openSwitches2={217,218,219};
                    if (visacom1.setSwitches(openSwitches2,closedSwitches2) )
                    {
                       // controllerMesg += "switching returned ok for case 2\n";
                       // controlMesgUpdated = true;
                    }
                    else
                    {
                       // controllerMesg = "Error in switching\n";
                       // controlMesgUpdated = true;
                     }
                    return true;
                default:
                    return false;

            }
        }
        private void worker_controllers_doWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bkgWorker = (BackgroundWorker)sender;
            while (true)
            {
                //bkgWorker.ReportProgress(0);
                System.Threading.Thread.Sleep(1000 * secondstocheckcontrollers);
                if (contollersUpdated)
                {
                    writeSetPointControllers(coldsidesetpointwrite, hotsidesetpointwrite,bkgWorker );
                    contollersUpdated = false;
                    bkgWorker.ReportProgress(10);
                }
                if (controllerRead)
                {
                    bkgWorker.ReportProgress(25);
                    double[] tempSetpoint=readSetPointControllers(bkgWorker );
                    double[] tempProcValue=readProcValueControllers(bkgWorker );
                    hotsidesetpointread=tempSetpoint[1] ;
                    coldsidesetpointread = tempSetpoint[0];
                    hotsideprocvalueread = tempProcValue[1];
                    coldsideprocvalueread = tempProcValue[0];
                    controllerRead = false;
                    bkgWorker.ReportProgress(90);
                }
                //bkgWorker.ReportProgress(100);
            }
        }
        private void worker_controllers_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            contStatus.Content = controllerMesg;
            if (e.ProgressPercentage == 90)
            {
                readCont.IsEnabled = true;
            }
            else if (e.ProgressPercentage == 10)
            {
                contButton.IsEnabled = true;
            }
            if (controlMesgUpdated)
            {
                //testOutput.Text = controllerMesg;
                //controlMesgUpdated = false;
            }
                coldSetValue.Content = coldsidesetpointread;
                hotSetValue.Content = hotsidesetpointread;
                coldProcValue.Content = coldsideprocvalueread;
                hotProcValue.Content = hotsideprocvalueread;
                
        }
        private void worker_stability_doWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bkgWorker = (BackgroundWorker)sender;
            while (true)
            {
                System.Threading.Thread.Sleep(1000 * secondstocheckslope);                
                check = true;        
                bkgWorker.ReportProgress(0);
            }
           
            
        }
        private void worker_stability_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string text = e.ProgressPercentage + " ";
        }
        private void worker_doWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bkgWorker = (BackgroundWorker)sender;
            int averagingcount=0;
            while (true)
            {
                System.Threading.Thread.Sleep(100);
                //MessageBox.Show("000Temp 1 " + Current_temps.temp1 + "\nTemp 2 " + Current_temps.temp2);
                temps_toShow.temp1 = Math.Round(Current_temps.temp1, 2);
                temps_toShow.temp2 = Math.Round(Current_temps.temp2, 2);
                //writer2.WriteLine(temps_toShow.temp1+","+temps_toShow.temp2);
                if (xValsList.Count < averaging)
                {
                    xValsList.Add(averagingcount);
                    y1ValsList.Add(temps_toShow.temp1);
                    y2ValsList.Add(temps_toShow.temp2);
                    if (check)
                    {
                        //xVals = xValsList.ToArray();
                        // y1Vals = y1ValsList.ToArray();
                        // y2Vals = y2ValsList.ToArray();
                        //slopes = checkaverage();

                        //check = false;
                        writecheck = false;
                    }
                }
                else
                {
                    xValsList.RemoveAt(0);
                    y1ValsList.RemoveAt(0);
                    y2ValsList.RemoveAt(0);
                    xValsList.Add(averagingcount);
                    y1ValsList.Add(temps_toShow.temp1);
                    y2ValsList.Add(temps_toShow.temp2);
                    if (check)
                    {
                        xVals = xValsList.ToArray();
                        y1Vals = y1ValsList.ToArray();
                        y2Vals = y2ValsList.ToArray();
                        slopes = checkaverage();
                       // MessageBox.Show("Here");
                       
                        //writer2.WriteLine(slopes[0] + "," + slopes[1]);
                        check = false;
                        writecheck = true;
                    }
                }        
                averagingcount++;                
                bkgWorker.ReportProgress(0);
            }            
        }
        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            
            //MessageBox.Show("Temp 1 " + Current_temps.temp1 + "\nTemp 2 " + Current_temps.temp2);
            temperature1.Content= temps_toShow.temp1.ToString();
            //Console.Write("Temperature  1 is: " + temps_toShow.temp1 + "\n");
            temperature2.Content =temps_toShow.temp2.ToString();
            
            if (writecheck)
            {
                if (slopearray < 5)
                {
                    coldslopearray.Add(slopes[0]);
                    hotslopearray.Add(slopes[1]);
                    slopearray++;
                }
                else
                {
                    coldslopearray.RemoveAt(0);
                    hotslopearray.RemoveAt(0);
                    coldslopearray.Add(slopes[0]);
                    hotslopearray.Add(slopes[1]);
                }   
                hotslopearray.Add(slopes[0]);
                Slope.Content = "Slope 1: " + slopes[0] + "  Slope 2: " + slopes[1];
                writecheck = false;
               
            }
             
            //Console.Write("Temperature  1 is: " + temps_toShow.temp1+"\n");
            //Console.Write("Temperature  2 is: " + temps_toShow.temp2 + "\n");
            //averaging = 10;
            //for (int i = 0; i <= averaging; i++)
            //{
               // temparray.Add(temps_toShow);

           // }
           // MessageBox.Show("Flushing Temp data");
            //dataflush(1);



        }
        public void tempCallbackReader(TempData temps)
        {
           
            //temperature1.Content ="Temperature  1 is: "+ temps.temp1;
            //temperature2.Content ="Temperature  2 is: "+ temps.temp2;
            Current_temps.temp2 = Math.Round(temps.temp2,5);
            Current_temps.temp1 = Math.Round(temps.temp1,5);
            //MessageBox.Show("Temp 1 "+Current_temps.temp1+"\nTemp 2 "+Current_temps.temp2);

        }       
        private void startTempReading()
        {
            this.worker.RunWorkerAsync();
        }
        private void startControllerReading()
        {
            this.worker_controllers.RunWorkerAsync();
        }
        private double[] checkaverage()
        {             
            LeastSquare ls1 = new LeastSquare();
            //MessageBox.Show(" Calculating average");
            
            ls1.LeastSquareFit(xVals,y1Vals,0,averaging);
            LeastSquare ls2 = new LeastSquare();
            ls2.LeastSquareFit(xVals, y2Vals, 0, averaging);
            double[] data={Math.Round(ls1.slope*1e5,2),Math.Round(ls2.slope*1e5,2)};
            //using (StreamWriter writer2 = new StreamWriter("C:\\test\\tempslopelog.csv"))
            //{
           //     writer2.WriteLine(data[0] + "," + data[1]);
           // }
                       
            return data;
        }
        private bool dataflush(int selector)
        {
            switch (selector)
            {
                case 1:
                    /*for (int i = 0; i < 5; i++)
                    {
                        TempData test = new TempData();
                        test.temp1 = 22.44*i;
                        test.temp2 = 22.55*i;
                        temparray.Add(test);
                    }*/
                    tempdatasavingTemplate dataInstance = new tempdatasavingTemplate();
                    dataInstance.savingdata = temparray;
                   // MessageBox.Show("Saving Data");
                    SaveConfig(dataInstance,filePath);
                    break;
                case 2:
                    Console.WriteLine("Case 2");
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }


            return false;
        }
        private bool SaveConfig(tempdatasavingTemplate dataInstance, string filePath)
        {
            bool success = false;
            using (FileStream flStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                try
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(tempdatasavingTemplate));
                    xmlSerializer.Serialize(flStream, dataInstance);
                    success = true;
                }
                catch (Exception ex)
                {
                   // System.Windows.Forms.MessageBox.Show(ex.Message);
                }
                finally
                {
                    flStream.Close();
                }
            }
            return success;
        }

        private void CalcSlope_Click(object sender, RoutedEventArgs e)
        {
            this.worker_stability.RunWorkerAsync();
            CalcSlope.IsEnabled = false;
              
            //check = true;
            

        }

        private void addressBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void contButtonClick(object sender, RoutedEventArgs e)
        {
            //System.Windows.MessageBox.Show("button clicked");
            bool inputError = false;
            try
            {
                if (hotContTemp.Text != "" && coldContTemp.Text != "")
                {
                    hotsidesetpointwrite = Convert.ToDouble(hotContTemp.Text);
                    coldsidesetpointwrite = Convert.ToDouble(coldContTemp.Text);
                    inputError = false;

                }
                else
                {
                    System.Windows.MessageBox.Show("You should enter a value for Cold and HOt Side");
                    inputError = true;
                }
            }
            catch (System.FormatException formatError)
            {
                System.Windows.MessageBox.Show("Hot Side Temperature is not Valid\n" + formatError.Message);
                inputError = true;
            }
            catch (System.OverflowException limitError)
            {
                System.Windows.MessageBox.Show("Hot Side Temperature is out of Limits\n" + limitError.Message);
                inputError = true;
            }

            //writeSetPointControllers(coldsidesetpointwrite, hotsidesetpointwrite);
            contollersUpdated = true;
            contButton.IsEnabled = false;
           
        }

        private void coldContTemp_TextChanged(object sender, TextChangedEventArgs e)
        {
           
        }
        
        
        private void hotContTemp_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {            
           
        }
        private void coldContTemp_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            
        }
        

        private void hotContTemp_TextChanged(object sender, TextChangedEventArgs e)
        {
           

        }

        private void readControllers(object sender, RoutedEventArgs e)
        {
            controllerRead = true;
            readCont.IsEnabled=false;
        }
        private void measureSC()
        {
            
            using (StreamWriter writer2 = new StreamWriter(SCdatafile, true))
            {

                KE2000CommandCenter k20 = new KE2000CommandCenter();
                k20.OpenPort();
                double seebeckVoltage = k20.readVoltage();
                writer2.WriteLine(temps_toShow.temp1 + "," + temps_toShow.temp2 + "," + seebeckVoltage);
            }
        }
        private void measureSCstepclick(object sender, RoutedEventArgs e)
        {
            this.Mainworker.RunWorkerAsync();
            readCont.IsEnabled = false;
            this.worker_stability.RunWorkerAsync();
            CalcSlope.IsEnabled = false;
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension 

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
            }
        }

       

       
       
    }
}
