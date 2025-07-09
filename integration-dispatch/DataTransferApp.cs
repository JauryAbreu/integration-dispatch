using integration_dispatch.Data;
using integration_dispatch.Interfaces;
using integration_dispatch.Model;
using integration_dispatch.Utils;

namespace integration_dispatch
{
    public partial class DataTransferApp : Form
    {
        private readonly IConfigLoader _configLoader;
        private readonly IDataTransferService _dataTransferService;
        private readonly LastRunManager _lastRunManager;
        private bool _isRunning;
        private Config _config;

        public DataTransferApp(IConfigLoader configLoader = null, IDataTransferService dataTransferService = null, LastRunManager lastRunManager = null)
        {
            InitializeComponent();
            _configLoader = configLoader ?? new ConfigLoader();
            _lastRunManager = lastRunManager ?? new LastRunManager();
            _dataTransferService = dataTransferService ?? CreateDataTransferService();
            InitializeAsync();
        }

        private Button btnRun;
        private Label lblStatus;
        private Label lblCounter;
        private DateTimePicker dtpStartDate;

        private async void InitializeAsync()
        {
            try
            {
                _config = await _configLoader.LoadConfigAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private IDataTransferService CreateDataTransferService()
        {
            var dbService = new DatabaseService(_config);
            return new DataTransferService(_config, dbService, status => lblStatus.Text += status, counter => lblCounter.Text = counter, _lastRunManager);
        }

        private async void btnRun_Click(object sender, EventArgs e)
        {
            if (_isRunning) return;

            _isRunning = true;
            lblStatus.Text = "Sending transactions...";
            btnRun.Enabled = false;

            try
            {
                await _dataTransferService.RunDataTransferAsync(dtpStartDate.Value);
                lblStatus.Text = "Process completed.";
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error: {ex.Message}";
            }
            finally
            {
                _isRunning = false;
                btnRun.Enabled = true;
            }
        }

        [STAThread]
        static async Task Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var app = new DataTransferApp();

            if (args.Length > 0 && int.TryParse(args[0], out int number))
            {

                var lastRunManager = new LastRunManager();

                var lastRunDate = await lastRunManager.LoadLastRunDateAsync();
                DateTime startDate = lastRunDate.HasValue ? lastRunDate.Value.AddMinutes(-1) : DateTime.Now.AddMinutes(number);
                await app._dataTransferService.RunDataTransferAsync(startDate);
            }
            Application.Run(app);
        }
    }
}