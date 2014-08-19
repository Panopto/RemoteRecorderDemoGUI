using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RemoteRecorderDemoGUI.PanoptoRemoteRecorderManagement;
using System.ComponentModel;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Timers;

namespace RemoteRecorderDemoGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static bool selfSigned = true; // Target server is a self-signed server
        private static bool hasBeenInitialized = false;

        static RemoteRecorderManagementClient rrMgr;
        static AuthenticationInfo authInfo;
        static Dictionary<string, Guid> folderInfo;
        static Dictionary<string, Guid> recorderInfo;
        static bool loginSuccess = true;
        static string loginFailureMessage = "";
        ScheduledRecordingResult recordingResult;
        Timer recordingTimer;

        public MainWindow()
        {
            InitializeComponent();

            rrMgr = new RemoteRecorderManagementClient();

            if (selfSigned)
            {
                // For self-signed servers
                EnsureCertificateValidation();
            }
        }

        /// <summary>
        /// Action to perform when user clicks login
        /// </summary>
        /// <param name="sender">object invoded this method</param>
        /// <param name="e">arguments passed in</param>
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            LoginLock();
            Status.Content = "Logging in...";

            authInfo = new AuthenticationInfo()
            {
                UserKey = UserID.Text,
                Password = UserPassword.Password
            };

            // Get data on seperate thread to not freeze UI
            BackgroundWorker bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            bgw.DoWork += new DoWorkEventHandler(LoginAction);
            bgw.ProgressChanged += new ProgressChangedEventHandler(LoginReportProgress);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(LoginFinished);

            bgw.RunWorkerAsync(new string[] { UserID.Text, UserPassword.Password });
        }

        /// <summary>
        /// Get folder and remote recorder user have access to
        /// </summary>
        /// <param name="sender">object invoking this method</param>
        /// <param name="e">argumnets</param>
        private void LoginAction(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgw = sender as BackgroundWorker;
            string[] args = e.Argument as string[];
            int progress = 0;

            // Get folders
            try
            {
                Dictionary<string, Guid> temp = RemoteRecorderWrapper.GetFolders(args[0], args[1]);
                progress += 50;
                bgw.ReportProgress(progress, new Dictionary<string, Guid>(temp));
            }
            catch (Exception ex)
            {
                bgw.ReportProgress(0, ex.Message);
            }

            // Get remote recorders
            try
            {
                Dictionary<string, Guid> temp = RemoteRecorderWrapper.GetRemoteRecorders(args[0], args[1]);
                progress += 50;
                bgw.ReportProgress(progress, new Dictionary<string, Guid>(temp));
            }
            catch (Exception ex)
            {
                bgw.ReportProgress(0, ex.Message);
            }
        }

        /// <summary>
        /// Report progress method when getting user's folder and remote recorder
        /// </summary>
        /// <param name="sender">object invoked this method</param>
        /// <param name="e">arguments</param>
        private void LoginReportProgress(object sender, ProgressChangedEventArgs e)
        {
            int progress = e.ProgressPercentage;

            // Handle failure
            if (progress == 0)
            {
                loginFailureMessage = e.UserState as string + "; ";
                loginSuccess = false;
            }

            // Store folders
            if (progress == 50 && loginSuccess)
            {
                Dictionary<string, Guid> temp = e.UserState as Dictionary<string, Guid>;
                folderInfo = new Dictionary<string, Guid>(temp);
            }

            // Store remote recorders
            if (progress == 100 && loginSuccess)
            {
                Dictionary<string, Guid> temp = e.UserState as Dictionary<string, Guid>;
                recorderInfo = new Dictionary<string, Guid>(temp);
            }
        }

        /// <summary>
        /// Process data retrieved
        /// </summary>
        /// <param name="sender">object invoked this method</param>
        /// <param name="e">arguments</param>
        private void LoginFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            // Retrieved data successfully
            if (loginSuccess)
            {
                // Process data
                //System.Collections.ArrayList keyArray = new System.Collections.ArrayList(folderInfo.Keys);
                //keyArray.Sort();
                //FillComboBox(Folder, keyArray);
                Folder.ItemsSource = folderInfo.Keys;
                //keyArray = new System.Collections.ArrayList(recorderInfo.Keys);
                //keyArray.Sort();
                //FillComboBox(RemoteRecorder, keyArray);
                RemoteRecorder.ItemsSource = recorderInfo.Keys;

                // Setup UI to schedule remote recording
                foreach (UIElement elem in RROptions.Children)
                {
                    elem.Visibility = System.Windows.Visibility.Visible;
                }

                StartStop.Visibility = System.Windows.Visibility.Visible;

                LoginLogout.Content = "Log Out";
                LoginLogout.Click -= Login_Click;
                LoginLogout.Click += Logout_Click;
                LoginLogout.IsEnabled = true;

                Status.Content = "Logged In";
            }
            else // Failed to retrieved data
            {
                Status.Content = "Login Failed: " + loginFailureMessage;
                loginFailureMessage = "";
                loginSuccess = true;
                LoginRelease();
            }
        }

        /// <summary>
        /// Fill combo box with data provided
        /// </summary>
        /// <param name="box">Box to fill</param>
        /// <param name="values">Data to fill</param>
        private void FillComboBox(ComboBox box, System.Collections.ArrayList values)
        {
            box.Items.Clear();

            foreach (string val in values)
            {
                ComboBoxItem boxItem = new ComboBoxItem();
                boxItem.Content = val;
                box.Items.Add(boxItem);
            }
        }

        /// <summary>
        /// Action performed when logged out
        /// </summary>
        /// <param name="sender">object invoked this method</param>
        /// <param name="e">arguments</param>
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            LoginLogout.IsEnabled = false;

            foreach (UIElement elem in RROptions.Children)
            {
                elem.Visibility = System.Windows.Visibility.Hidden;
            }

            StartStop.Visibility = System.Windows.Visibility.Hidden;

            LoginLogout.Content = "Login";
            LoginLogout.Click -= Logout_Click;
            LoginLogout.Click += Login_Click;

            authInfo = null;

            Status.Content = "Logged Out";

            LoginRelease();
        }

        /// <summary>
        /// Lock fields when logging in
        /// </summary>
        private void LoginLock()
        {
            UserID.IsEnabled = false;
            UserPassword.IsEnabled = false;
            LoginLogout.IsEnabled = false;
        }

        /// <summary>
        /// Release fields locked when logging in
        /// </summary>
        private void LoginRelease()
        {
            UserID.IsEnabled = true;
            UserPassword.IsEnabled = true;
            LoginLogout.IsEnabled = true;
        }

        /// <summary>
        /// Action performed when starting recording
        /// </summary>
        /// <param name="sender">Object invoked this method</param>
        /// <param name="e">arguments</param>
        private void StartRecording_Click(object sender, RoutedEventArgs e)
        {
            RecordingLock();
            Status.Content = "Starting Recording...";
            bool recordingSuccess = true;
            int length = 0;
            string recordingFailureMessage = "";

            // Get information necessary to start recording
            Guid folderID = Guid.Empty;
            if (Folder.SelectedIndex >= 0)
            {
                string folderName = Folder.Text;
                folderID = folderInfo[folderName];
            }
            else
            {
                recordingSuccess = false;
                recordingFailureMessage += "Choose a folder to record to";
            }

            Guid rrID = Guid.Empty;
            if (RemoteRecorder.SelectedIndex >= 0)
            {
                string rrName = RemoteRecorder.Text;
                rrID = recorderInfo[rrName];
            }
            else
            {
                recordingSuccess = false;
                recordingFailureMessage += "Choose a remote recorder to record from; ";
            }

            try
            {
                length = Convert.ToInt32(SessionLength.Text);
                if (length <= 0)
                {
                    recordingSuccess = false;
                    recordingFailureMessage += "Invalid session length input; ";
                }
            }
            catch (Exception)
            {
                recordingSuccess = false;
                recordingFailureMessage += "Invalid session length input; ";
            }

            // Start UI if information is correct
            if (recordingSuccess)
            {
                recordingResult = RemoteRecorderWrapper.StartRecordingSession(rrMgr, authInfo, length, SessionName.Text, folderID, rrID);
                
                if (recordingResult == null || recordingResult.SessionIDs == null || recordingResult.SessionIDs.Length == 0)
                {
                    recordingSuccess = false;
                    recordingFailureMessage += "Start Recording Failed: Unable to start recording";
                }
            }

            // Change UI to match recording state
            if (recordingSuccess)
            {
                StartStop.Content = "Stop Recording";
                StartStop.Click -= StartRecording_Click;
                StartStop.Click += StopRecording_Click;
                StartStop.IsEnabled = true;

                Status.Content = "Recording...";

                double lengthMilliseconds = length * 60 * 1000;
                recordingTimer = new Timer(lengthMilliseconds);
                recordingTimer.Elapsed += new ElapsedEventHandler(TimerStop);
                recordingTimer.Start();
            }
            else // Return UI to start recording state since starting recording failed
            {
                Status.Content = "Start Recording Failed: " + recordingFailureMessage;
                RecordingRelease();
            }
        }

        /// <summary>
        /// Action performed when stopping recording
        /// </summary>
        /// <param name="sender">Object invoked this method</param>
        /// <param name="e">arguments</param>
        private void StopRecording_Click(object sender, RoutedEventArgs e)
        {
            recordingTimer.Stop();
            recordingTimer = null;

            StartStop.IsEnabled = false;

            StartStop.Content = "Start Recording";
            StartStop.Click -= StopRecording_Click;
            StartStop.Click += StartRecording_Click;

            // Stop the recording
            RemoteRecorderWrapper.StopSessionRecording(rrMgr, authInfo, recordingResult.SessionIDs[0]);

            Status.Content = "Recording Stopped";

            RecordingRelease();
        }

        /// <summary>
        /// Timer action to invoke method to stop recording when user provided time has reached
        /// </summary>
        /// <param name="sender">object invoked this method</param>
        /// <param name="e">arguments</param>
        private void TimerStop(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(TimerStopAction));
        }

        /// <summary>
        /// Timer stop recording action
        /// </summary>
        private void TimerStopAction()
        {
            recordingTimer.Stop();
            recordingTimer = null;

            StartStop.IsEnabled = false;

            StartStop.Content = "Start Recording";
            StartStop.Click -= StopRecording_Click;
            StartStop.Click += StartRecording_Click;

            Status.Content = "Recording Stopped";

            RecordingRelease();
        }

        /// <summary>
        /// Lock fields when recording
        /// </summary>
        private void RecordingLock()
        {
            LoginLogout.IsEnabled = false;
            SessionName.IsEnabled = false;
            SessionLength.IsEnabled = false;
            Folder.IsEnabled = false;
            RemoteRecorder.IsEnabled = false;
            StartStop.IsEnabled = false;
        }

        /// <summary>
        /// Release fields locked when recording
        /// </summary>
        private void RecordingRelease()
        {
            LoginLogout.IsEnabled = true;
            SessionName.IsEnabled = true;
            SessionLength.IsEnabled = true;
            Folder.IsEnabled = true;
            RemoteRecorder.IsEnabled = true;
            StartStop.IsEnabled = true;
        }

        //========================= Needed to use self-signed servers

        /// <summary>
        /// Ensures that our custom certificate validation has been applied
        /// </summary>
        public static void EnsureCertificateValidation()
        {
            if (!hasBeenInitialized)
            {
                ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(CustomCertificateValidation);
                hasBeenInitialized = true;
            }
        }

        /// <summary>
        /// Ensures that server certificate is authenticated
        /// </summary>
        private static bool CustomCertificateValidation(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }

    }
}
