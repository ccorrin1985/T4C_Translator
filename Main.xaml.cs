using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace T4C_Translator
{
	public partial class Main : Window, INotifyPropertyChanged
	{
		#region Class Variables

		private bool testMode;
		private const string ContainerName = "libretranslate";
		private const string ImageName = "libretranslate/libretranslate";
		private const string DockerUri = "npipe://./pipe/docker_engine";
		private readonly string configFilePath = "config.json";
		private readonly string logFilePath = @"\TranslatedLogs.txt";
		private float translationSeconds = 10;
		private readonly float noShade = 0;
		private readonly float halfShade = 0.5F;
		private readonly float fullShade = 1;
		private readonly string keywordMatch = @"""([^""]*)""";
		private readonly string bracketMatch = @"\{([^{}]*)\}";
		private readonly System.Windows.Media.Brush whiteBrush = new SolidColorBrush(Colors.White);
		private readonly System.Windows.Media.Brush grayBrush = new SolidColorBrush(Colors.Gray);
		private long lastReadFilePosition = 0;
		private FileSystemWatcher? logFileWatcher;
		private readonly int messageDelay = 3000;
		private readonly string originalWindowMessage = "T4C Log Translator - Waiting to translate...";
		private string windowMessage = "T4C Log Translator - Waiting to translate...";
		private string emptyFlowDocument;
		readonly Encoding enc1252 = Encoding.GetEncoding(1252);
		private bool loadFinished = false;
		private List<TranslatedLine> translatedLines = new List<TranslatedLine>();
		private List<TranslatedLineType> translatedLineTypes = new List<TranslatedLineType>();
		private List<DispatcherTimer> timers = new List<DispatcherTimer>();
		private List<string> hyperlinkXamlNames = new List<string>();
		private DateTime lastTranslatedLineTimestamp = DateTime.MinValue;
		private string lastTranslatedLine = string.Empty;

		#endregion

		#region Properties

		private ICollectionView _quickTranslatefilteredItems;
		public ICollectionView QuickTranslatefilteredItems => _quickTranslatefilteredItems;

		private ObservableCollection<TranslatedLine> _quickTranslateComboBoxItems;
		public ObservableCollection<TranslatedLine> QuickTranslateComboBoxItems
		{
			get => _quickTranslateComboBoxItems;
			set
			{
				if (_quickTranslateComboBoxItems != value)
				{
					_quickTranslateComboBoxItems = value;
					OnPropertyChanged(nameof(QuickTranslateComboBoxItems));
				}
			}
		}

		private ICollectionView _sourceLanguagefilteredItems;
		public ICollectionView SourceLanguagefilteredItems => _sourceLanguagefilteredItems;

		private ObservableCollection<string> _sourceLanguageComboBoxItems;
		public ObservableCollection<string> SourceLanguageComboBoxItems
		{
			get => _sourceLanguageComboBoxItems;
			set
			{
				if (_sourceLanguageComboBoxItems != value)
				{
					_sourceLanguageComboBoxItems = value;
					OnPropertyChanged(nameof(SourceLanguageComboBoxItems));
				}
			}
		}

		private ICollectionView _outputLanguagefilteredItems;
		public ICollectionView OutputLanguagefilteredItems => _outputLanguagefilteredItems;

		private ObservableCollection<string> _outputLanguageComboBoxItems;
		public ObservableCollection<string> OutputLanguageComboBoxItems
		{
			get => _outputLanguageComboBoxItems;
			set
			{
				if (_outputLanguageComboBoxItems != value)
				{
					_outputLanguageComboBoxItems = value;
					OnPropertyChanged(nameof(OutputLanguageComboBoxItems));
				}
			}
		}

		private string _characterName;
		public string CharacterName
		{
			get => _characterName;
			set
			{
				if (_characterName != value)
				{
					_characterName = value;
					OnPropertyChanged(nameof(CharacterName));
				}
			}
		}

		private string _sourceFolder;
		public string SourceFolder
		{
			get => _sourceFolder;
			set
			{
				if (_sourceFolder != value)
				{
					_sourceFolder = value;
					OnPropertyChanged(nameof(SourceFolder));
				}
			}
		}

		private string _outputFileName;
		public string OutputFileName
		{
			get => _outputFileName;
			set
			{
				if (_outputFileName != value)
				{
					_outputFileName = value;
					OnPropertyChanged(nameof(OutputFileName));
				}
			}
		}

		private string _apiLimit;
		public string APILimit
		{
			get => _apiLimit;
			set
			{
				if (_apiLimit != value)
				{
					_apiLimit = value;
					OnPropertyChanged(nameof(APILimit));
				}
			}
		}

		private string _apiCount;
		public string APICount
		{
			get => _apiCount;
			set
			{
				if (_apiCount != value)
				{
					if (loadFinished)
						SaveConfig();
					_apiCount = value;
					OnPropertyChanged(nameof(APICount));
				}
			}
		}

		private string _apiKey;
		public string APIKey
		{
			get => _apiKey;
			set
			{
				if (_apiKey != value)
				{
					_apiKey = value;
					OnPropertyChanged(nameof(APIKey));
				}
			}
		}

		private string _selectedOutputLanguage;
		public string SelectedOutputLanguage
		{
			get => _selectedOutputLanguage;
			set
			{
				if (_selectedOutputLanguage != value)
				{
					_selectedOutputLanguage = value;
					OnPropertyChanged(nameof(SelectedOutputLanguage));			
				}
			}
		}

		private string _selectedSourceLanguage;
		public string SelectedSourceLanguage
		{
			get => _selectedSourceLanguage;
			set
			{
				if (_selectedSourceLanguage != value)
				{
					_selectedSourceLanguage = value;
					OnPropertyChanged(nameof(SelectedSourceLanguage));
				}
			}
		}

		private string _selectedQTOutputLanguage;
		public string SelectedQTOutputLanguage
		{
			get => _selectedQTOutputLanguage;
			set
			{
				if (_selectedQTOutputLanguage != value)
				{
					_selectedQTOutputLanguage = value;
					OnPropertyChanged(nameof(SelectedQTOutputLanguage));
					OnPropertyChanged(nameof(TranslatedTextLabel));
					OnPropertyChanged(nameof(TexttotranslateLabel));
					OnPropertyChanged(nameof(QuickTranslatefilteredItems));
					OnPropertyChanged(nameof(QuickTranslateComboBoxItems));
				}
			}
		}

		private string _selectedQTSourceLanguage;
		public string SelectedQTSourceLanguage
		{
			get => _selectedQTSourceLanguage;
			set
			{
				if (_selectedQTSourceLanguage != value)
				{
					_selectedQTSourceLanguage = value;
					OnPropertyChanged(nameof(SelectedQTSourceLanguage));
					OnPropertyChanged(nameof(TranslatedTextLabel));
					OnPropertyChanged(nameof(TexttotranslateLabel));
					OnPropertyChanged(nameof(QuickTranslatefilteredItems));
					OnPropertyChanged(nameof(QuickTranslateComboBoxItems));
				}
			}
		}

		private TranslatedLine _selectedQuickTranslate;
		public TranslatedLine SelectedQuickTranslate
		{
			get => _selectedQuickTranslate;
			set
			{
				_selectedQuickTranslate = value;
				OnPropertyChanged(nameof(SelectedQuickTranslate));
			}
		}

		private ObservableCollection<IgnoreChat> _ignoreChatComboBoxItems;
		public ObservableCollection<IgnoreChat> IgnoreChatComboBoxItems
		{
			get => _ignoreChatComboBoxItems;
			set
			{
				if (_ignoreChatComboBoxItems != value)
				{
					_ignoreChatComboBoxItems = value;
					OnPropertyChanged(nameof(IgnoreChatComboBoxItems));
				}
			}
		}

		private IgnoreChat _selectedIgnoreChat;
		public IgnoreChat SelectedIgnoreChat
		{
			get => _selectedIgnoreChat;
			set
			{
				_selectedIgnoreChat = value;
				OnPropertyChanged(nameof(SelectedIgnoreChat));
			}
		}

		private ObservableCollection<FontColor> _fontColorComboBoxItems;
		public ObservableCollection<FontColor> FontColorComboBoxItems
		{
			get => _fontColorComboBoxItems;
			set
			{
				if (_fontColorComboBoxItems != value)
				{
					_fontColorComboBoxItems = value;
					OnPropertyChanged(nameof(FontColorComboBoxItems));
				}
			}
		}

		private FontColor _selectedFontColor;
		public FontColor SelectedFontColor
		{
			get => _selectedFontColor;
			set
			{
				_selectedFontColor = value;
				OnPropertyChanged(nameof(SelectedFontColor));
				OnPropertyChanged(nameof(SelectedFontColorLight));
			}
		}

		public string SelectedFontColorLight
		{
			get => ChangeColorBrightness(SelectedFontColor.Color, 0.5F);
		}

		private ObservableCollection<FontSize> _fontSizeComboBoxItems;
		public ObservableCollection<FontSize> FontSizeComboBoxItems
		{
			get => _fontSizeComboBoxItems;
			set
			{
				if (_fontSizeComboBoxItems != value)
				{
					_fontSizeComboBoxItems = value;
					OnPropertyChanged(nameof(FontSizeComboBoxItems));
				}
			}
		}

		private FontSize _selectedFontSize;
		public FontSize SelectedFontSize
		{
			get => _selectedFontSize;
			set
			{
				_selectedFontSize = value;
				OnPropertyChanged(nameof(SelectedFontSize));
			}
		}

		private bool isTranslating;
		public bool IsTranslating
		{
			get => isTranslating;
			set
			{
				if (isTranslating != value)
				{
					isTranslating = value;
					OnPropertyChanged(nameof(IsTranslating));
				}
			}
		}

		private bool _ignoreChatMessages;
		public bool IgnoreChatMessages
		{
			get { return _ignoreChatMessages; }
			set
			{
				_ignoreChatMessages = value;
				OnPropertyChanged(nameof(IgnoreChatMessages));
			}
		}

		private bool _showOriginalText;
		public bool ShowOriginalText
		{
			get { return _showOriginalText; }
			set
			{
				_showOriginalText = value;
				OnPropertyChanged(nameof(ShowOriginalText));
			}
		}

		private bool _useFreeAPI;
		public bool UseFreeAPI
		{
			get { return _useFreeAPI; }
			set
			{
				_useFreeAPI = value;
				OnPropertyChanged(nameof(UseFreeAPI));
			}
		}

		public string TexttotranslateLabel
		{
			get { return $"Translate to {SelectedQTSourceLanguage.ToString()}"; }
		}

		public string TranslatedTextLabel
		{
			get { return $"Translate from {SelectedQTOutputLanguage.ToString()}"; }
		}


		private string _statusMessage;
		public string StatusMessage
		{
			get { return _statusMessage; }
			set
			{
				_statusMessage = value;
				OnPropertyChanged(nameof(StatusMessage));
			}
		}

		#endregion

		#region OnPropertyChanged Event Handlers

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string propertyName)
		{
			// Raise property changed so the UI will update in our application
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region Constructor

		public Main()
		{
			// Set test mode on/off
			testMode = false;

			// Set data context and bootstrap application
			InitializeComponent();

			// Set default values on startup
			_quickTranslateComboBoxItems = new ObservableCollection<TranslatedLine>();
			_ignoreChatComboBoxItems = new ObservableCollection<IgnoreChat>();
			_quickTranslateComboBoxItems = new ObservableCollection<TranslatedLine>();
			_fontColorComboBoxItems = new ObservableCollection<FontColor>();
			_fontSizeComboBoxItems = new ObservableCollection<FontSize>();
			_sourceLanguageComboBoxItems = new ObservableCollection<string>();
			_outputLanguageComboBoxItems = new ObservableCollection<string>();
			_quickTranslatefilteredItems = CollectionViewSource.GetDefaultView(QuickTranslateComboBoxItems);
			_sourceLanguagefilteredItems = CollectionViewSource.GetDefaultView(SourceLanguageComboBoxItems);
			_outputLanguagefilteredItems = CollectionViewSource.GetDefaultView(OutputLanguageComboBoxItems);
			IsTranslating = false;
			UseFreeAPI = false;
			IgnoreChatMessages = false;
			ShowOriginalText = false;
			StatusMessage = windowMessage;
			OutputFileName = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + logFilePath;
			SourceFolder = @"C:\Users\yourusername\AppData\Local\Dialsoft\Logs";
			CharacterName = "YourCharacterName";
			APIKey = "YourApiKey";
			APILimit = "450000";
			APICount = "0";

			// Add translated line types
			translatedLineTypes.Add(new TranslatedLineType() { Id = 1, Type = "Local Chat", Example = "08/10/2024 17:09:12-- {Himinbjörg}\":\" vendre" });
			translatedLineTypes.Add(new TranslatedLineType() { Id = 2, Type = "Chat Channel", Example = "08/10/2024 17:18:23-- [\"Main\"] \"Astaroth Asgard\": c mort trop dur pour moi tt seul lol" });
			translatedLineTypes.Add(new TranslatedLineType() { Id = 3, Type = "Server Chat", Example = "08/10/2024 17:19:12-- [Beyond The Kingdom] Il est fortement conseillé, avant d'aller voir Rackis, de faire la quête du diable (consultez {Kalastor}) si vous désirez plus tard suivre les quêtes de karma mauvais pour Beyond the Kingdom." });
			translatedLineTypes.Add(new TranslatedLineType() { Id = 4, Type = "Server Message", Example = "08/10/2024 17:59:27-- Le saviez-vous ? <> Il existe des { familiers} sur Neerya. Acquis en récompense lors d'une animation ou en les achetant dans un repaire de faction, ils vous suivront partout !" });

			// Load default values for comboboxes
			FontColorComboBoxItems.Add(new FontColor() { Color = "#11B5CD" });
			FontColorComboBoxItems.Add(new FontColor() { Color = "#11B5CD" });
			FontColorComboBoxItems.Add(new FontColor() { Color = "#FF214B" });
			FontColorComboBoxItems.Add(new FontColor() { Color = "#5A77D8" });
			FontColorComboBoxItems.Add(new FontColor() { Color = "#DCCA06" });
			FontColorComboBoxItems.Add(new FontColor() { Color = "#05C525" });
			FontColorComboBoxItems.Add(new FontColor() { Color = "#C70ACC" });
			FontColorComboBoxItems.Add(new FontColor() { Color = "#C0C0C0" });
			FontColorComboBoxItems.Add(new FontColor() { Color = "#E0640A" });

			// Add languges to comboboxes
			SourceLanguageComboBoxItems.Add("en");
			SourceLanguageComboBoxItems.Add("fr");
			SourceLanguageComboBoxItems.Add("af");
			SourceLanguageComboBoxItems.Add("sq");
			SourceLanguageComboBoxItems.Add("am");
			SourceLanguageComboBoxItems.Add("ar");
			SourceLanguageComboBoxItems.Add("hy");
			SourceLanguageComboBoxItems.Add("as");
			SourceLanguageComboBoxItems.Add("ay");
			SourceLanguageComboBoxItems.Add("az");
			SourceLanguageComboBoxItems.Add("bm");
			SourceLanguageComboBoxItems.Add("eu");
			SourceLanguageComboBoxItems.Add("be");
			SourceLanguageComboBoxItems.Add("bn");
			SourceLanguageComboBoxItems.Add("bho");
			SourceLanguageComboBoxItems.Add("bs");
			SourceLanguageComboBoxItems.Add("bg");
			SourceLanguageComboBoxItems.Add("ca");
			SourceLanguageComboBoxItems.Add("ceb");
			SourceLanguageComboBoxItems.Add("co");
			SourceLanguageComboBoxItems.Add("hr");
			SourceLanguageComboBoxItems.Add("cs");
			SourceLanguageComboBoxItems.Add("da");
			SourceLanguageComboBoxItems.Add("dv");
			SourceLanguageComboBoxItems.Add("doi");
			SourceLanguageComboBoxItems.Add("nl");
			SourceLanguageComboBoxItems.Add("eo");
			SourceLanguageComboBoxItems.Add("et");
			SourceLanguageComboBoxItems.Add("ee");
			SourceLanguageComboBoxItems.Add("fil");
			SourceLanguageComboBoxItems.Add("fi");
			SourceLanguageComboBoxItems.Add("fy");
			SourceLanguageComboBoxItems.Add("gl");
			SourceLanguageComboBoxItems.Add("ka");
			SourceLanguageComboBoxItems.Add("de");
			SourceLanguageComboBoxItems.Add("el");
			SourceLanguageComboBoxItems.Add("gn");
			SourceLanguageComboBoxItems.Add("gu");
			SourceLanguageComboBoxItems.Add("ht");
			SourceLanguageComboBoxItems.Add("ha");
			SourceLanguageComboBoxItems.Add("haw");
			SourceLanguageComboBoxItems.Add("hi");
			SourceLanguageComboBoxItems.Add("hmn");
			SourceLanguageComboBoxItems.Add("hu");
			SourceLanguageComboBoxItems.Add("is");
			SourceLanguageComboBoxItems.Add("ig");
			SourceLanguageComboBoxItems.Add("ilo");
			SourceLanguageComboBoxItems.Add("id");
			SourceLanguageComboBoxItems.Add("ga");
			SourceLanguageComboBoxItems.Add("it");
			SourceLanguageComboBoxItems.Add("ja");
			SourceLanguageComboBoxItems.Add("kn");
			SourceLanguageComboBoxItems.Add("kk");
			SourceLanguageComboBoxItems.Add("km");
			SourceLanguageComboBoxItems.Add("rw");
			SourceLanguageComboBoxItems.Add("gom");
			SourceLanguageComboBoxItems.Add("ko");
			SourceLanguageComboBoxItems.Add("kri");
			SourceLanguageComboBoxItems.Add("ku");
			SourceLanguageComboBoxItems.Add("ckb");
			SourceLanguageComboBoxItems.Add("ky");
			SourceLanguageComboBoxItems.Add("lo");
			SourceLanguageComboBoxItems.Add("la");
			SourceLanguageComboBoxItems.Add("lv");
			SourceLanguageComboBoxItems.Add("ln");
			SourceLanguageComboBoxItems.Add("lt");
			SourceLanguageComboBoxItems.Add("lg");
			SourceLanguageComboBoxItems.Add("lb");
			SourceLanguageComboBoxItems.Add("mk");
			SourceLanguageComboBoxItems.Add("ma");
			SourceLanguageComboBoxItems.Add("mg");
			SourceLanguageComboBoxItems.Add("ms");
			SourceLanguageComboBoxItems.Add("ml");
			SourceLanguageComboBoxItems.Add("mt");
			SourceLanguageComboBoxItems.Add("mi");
			SourceLanguageComboBoxItems.Add("mr");
			SourceLanguageComboBoxItems.Add("lus");
			SourceLanguageComboBoxItems.Add("mn");
			SourceLanguageComboBoxItems.Add("my");
			SourceLanguageComboBoxItems.Add("ne");
			SourceLanguageComboBoxItems.Add("no");
			SourceLanguageComboBoxItems.Add("ny");
			SourceLanguageComboBoxItems.Add("or");
			SourceLanguageComboBoxItems.Add("om");
			SourceLanguageComboBoxItems.Add("ps");
			SourceLanguageComboBoxItems.Add("fa");
			SourceLanguageComboBoxItems.Add("pl");
			SourceLanguageComboBoxItems.Add("pt");
			SourceLanguageComboBoxItems.Add("pa");
			SourceLanguageComboBoxItems.Add("qu");
			SourceLanguageComboBoxItems.Add("ro");
			SourceLanguageComboBoxItems.Add("ru");
			SourceLanguageComboBoxItems.Add("sm");
			SourceLanguageComboBoxItems.Add("sa");
			SourceLanguageComboBoxItems.Add("gd");
			SourceLanguageComboBoxItems.Add("nso");
			SourceLanguageComboBoxItems.Add("sr");
			SourceLanguageComboBoxItems.Add("st");
			SourceLanguageComboBoxItems.Add("sn");
			SourceLanguageComboBoxItems.Add("sd");
			SourceLanguageComboBoxItems.Add("si");
			SourceLanguageComboBoxItems.Add("sk");
			SourceLanguageComboBoxItems.Add("sl");
			SourceLanguageComboBoxItems.Add("so");
			SourceLanguageComboBoxItems.Add("es");
			SourceLanguageComboBoxItems.Add("su");
			SourceLanguageComboBoxItems.Add("sw");
			SourceLanguageComboBoxItems.Add("sv");
			SourceLanguageComboBoxItems.Add("tl");
			SourceLanguageComboBoxItems.Add("tg");
			SourceLanguageComboBoxItems.Add("ta");
			SourceLanguageComboBoxItems.Add("tt");
			SourceLanguageComboBoxItems.Add("te");
			SourceLanguageComboBoxItems.Add("th");
			SourceLanguageComboBoxItems.Add("ti");
			SourceLanguageComboBoxItems.Add("ts");
			SourceLanguageComboBoxItems.Add("tr");
			SourceLanguageComboBoxItems.Add("tk");
			SourceLanguageComboBoxItems.Add("ak");
			SourceLanguageComboBoxItems.Add("uk");
			SourceLanguageComboBoxItems.Add("ur");
			SourceLanguageComboBoxItems.Add("ug");
			SourceLanguageComboBoxItems.Add("uz");
			SourceLanguageComboBoxItems.Add("vi");
			SourceLanguageComboBoxItems.Add("cy");
			SourceLanguageComboBoxItems.Add("xh");
			SourceLanguageComboBoxItems.Add("yi");
			SourceLanguageComboBoxItems.Add("yo");
			SourceLanguageComboBoxItems.Add("zu");
			OutputLanguageComboBoxItems.Add("en");
			OutputLanguageComboBoxItems.Add("fr");
			OutputLanguageComboBoxItems.Add("af");
			OutputLanguageComboBoxItems.Add("sq");
			OutputLanguageComboBoxItems.Add("am");
			OutputLanguageComboBoxItems.Add("ar");
			OutputLanguageComboBoxItems.Add("hy");
			OutputLanguageComboBoxItems.Add("as");
			OutputLanguageComboBoxItems.Add("ay");
			OutputLanguageComboBoxItems.Add("az");
			OutputLanguageComboBoxItems.Add("bm");
			OutputLanguageComboBoxItems.Add("eu");
			OutputLanguageComboBoxItems.Add("be");
			OutputLanguageComboBoxItems.Add("bn");
			OutputLanguageComboBoxItems.Add("bho");
			OutputLanguageComboBoxItems.Add("bs");
			OutputLanguageComboBoxItems.Add("bg");
			OutputLanguageComboBoxItems.Add("ca");
			OutputLanguageComboBoxItems.Add("ceb");
			OutputLanguageComboBoxItems.Add("co");
			OutputLanguageComboBoxItems.Add("hr");
			OutputLanguageComboBoxItems.Add("cs");
			OutputLanguageComboBoxItems.Add("da");
			OutputLanguageComboBoxItems.Add("dv");
			OutputLanguageComboBoxItems.Add("doi");
			OutputLanguageComboBoxItems.Add("nl");
			OutputLanguageComboBoxItems.Add("eo");
			OutputLanguageComboBoxItems.Add("et");
			OutputLanguageComboBoxItems.Add("ee");
			OutputLanguageComboBoxItems.Add("fil");
			OutputLanguageComboBoxItems.Add("fi");
			OutputLanguageComboBoxItems.Add("fy");
			OutputLanguageComboBoxItems.Add("gl");
			OutputLanguageComboBoxItems.Add("ka");
			OutputLanguageComboBoxItems.Add("de");
			OutputLanguageComboBoxItems.Add("el");
			OutputLanguageComboBoxItems.Add("gn");
			OutputLanguageComboBoxItems.Add("gu");
			OutputLanguageComboBoxItems.Add("ht");
			OutputLanguageComboBoxItems.Add("ha");
			OutputLanguageComboBoxItems.Add("haw");
			OutputLanguageComboBoxItems.Add("hi");
			OutputLanguageComboBoxItems.Add("hmn");
			OutputLanguageComboBoxItems.Add("hu");
			OutputLanguageComboBoxItems.Add("is");
			OutputLanguageComboBoxItems.Add("ig");
			OutputLanguageComboBoxItems.Add("ilo");
			OutputLanguageComboBoxItems.Add("id");
			OutputLanguageComboBoxItems.Add("ga");
			OutputLanguageComboBoxItems.Add("it");
			OutputLanguageComboBoxItems.Add("ja");
			OutputLanguageComboBoxItems.Add("kn");
			OutputLanguageComboBoxItems.Add("kk");
			OutputLanguageComboBoxItems.Add("km");
			OutputLanguageComboBoxItems.Add("rw");
			OutputLanguageComboBoxItems.Add("gom");
			OutputLanguageComboBoxItems.Add("ko");
			OutputLanguageComboBoxItems.Add("kri");
			OutputLanguageComboBoxItems.Add("ku");
			OutputLanguageComboBoxItems.Add("ckb");
			OutputLanguageComboBoxItems.Add("ky");
			OutputLanguageComboBoxItems.Add("lo");
			OutputLanguageComboBoxItems.Add("la");
			OutputLanguageComboBoxItems.Add("lv");
			OutputLanguageComboBoxItems.Add("ln");
			OutputLanguageComboBoxItems.Add("lt");
			OutputLanguageComboBoxItems.Add("lg");
			OutputLanguageComboBoxItems.Add("lb");
			OutputLanguageComboBoxItems.Add("mk");
			OutputLanguageComboBoxItems.Add("ma");
			OutputLanguageComboBoxItems.Add("mg");
			OutputLanguageComboBoxItems.Add("ms");
			OutputLanguageComboBoxItems.Add("ml");
			OutputLanguageComboBoxItems.Add("mt");
			OutputLanguageComboBoxItems.Add("mi");
			OutputLanguageComboBoxItems.Add("mr");
			OutputLanguageComboBoxItems.Add("lus");
			OutputLanguageComboBoxItems.Add("mn");
			OutputLanguageComboBoxItems.Add("my");
			OutputLanguageComboBoxItems.Add("ne");
			OutputLanguageComboBoxItems.Add("no");
			OutputLanguageComboBoxItems.Add("ny");
			OutputLanguageComboBoxItems.Add("or");
			OutputLanguageComboBoxItems.Add("om");
			OutputLanguageComboBoxItems.Add("ps");
			OutputLanguageComboBoxItems.Add("fa");
			OutputLanguageComboBoxItems.Add("pl");
			OutputLanguageComboBoxItems.Add("pt");
			OutputLanguageComboBoxItems.Add("pa");
			OutputLanguageComboBoxItems.Add("qu");
			OutputLanguageComboBoxItems.Add("ro");
			OutputLanguageComboBoxItems.Add("ru");
			OutputLanguageComboBoxItems.Add("sm");
			OutputLanguageComboBoxItems.Add("sa");
			OutputLanguageComboBoxItems.Add("gd");
			OutputLanguageComboBoxItems.Add("nso");
			OutputLanguageComboBoxItems.Add("sr");
			OutputLanguageComboBoxItems.Add("st");
			OutputLanguageComboBoxItems.Add("sn");
			OutputLanguageComboBoxItems.Add("sd");
			OutputLanguageComboBoxItems.Add("si");
			OutputLanguageComboBoxItems.Add("sk");
			OutputLanguageComboBoxItems.Add("sl");
			OutputLanguageComboBoxItems.Add("so");
			OutputLanguageComboBoxItems.Add("es");
			OutputLanguageComboBoxItems.Add("su");
			OutputLanguageComboBoxItems.Add("sw");
			OutputLanguageComboBoxItems.Add("sv");
			OutputLanguageComboBoxItems.Add("tl");
			OutputLanguageComboBoxItems.Add("tg");
			OutputLanguageComboBoxItems.Add("ta");
			OutputLanguageComboBoxItems.Add("tt");
			OutputLanguageComboBoxItems.Add("te");
			OutputLanguageComboBoxItems.Add("th");
			OutputLanguageComboBoxItems.Add("ti");
			OutputLanguageComboBoxItems.Add("ts");
			OutputLanguageComboBoxItems.Add("tr");
			OutputLanguageComboBoxItems.Add("tk");
			OutputLanguageComboBoxItems.Add("ak");
			OutputLanguageComboBoxItems.Add("uk");
			OutputLanguageComboBoxItems.Add("ur");
			OutputLanguageComboBoxItems.Add("ug");
			OutputLanguageComboBoxItems.Add("uz");
			OutputLanguageComboBoxItems.Add("vi");
			OutputLanguageComboBoxItems.Add("cy");
			OutputLanguageComboBoxItems.Add("xh");
			OutputLanguageComboBoxItems.Add("yi");
			OutputLanguageComboBoxItems.Add("yo");
			OutputLanguageComboBoxItems.Add("zu");

			// Add font sizes 1-30 to combobox
			int counter = 0;
			while (counter < 30)
			{
				FontSizeComboBoxItems.Add(new FontSize() { Size = counter + 1 });
				counter++;
			}

			// Set default selected items for comboboxes if not selected already
			IgnoreChatComboBox.SelectedIndex = -1;
			QuickTranslateComboBox.SelectedIndex = -1;
			SelectedSourceLanguage = SourceLanguageComboBoxItems.FirstOrDefault(s => s == "fr");
			SelectedOutputLanguage = OutputLanguageComboBoxItems.FirstOrDefault(s => s == "en");
			SelectedQTSourceLanguage = SourceLanguageComboBoxItems.FirstOrDefault(s => s == "fr");
			SelectedQTOutputLanguage = OutputLanguageComboBoxItems.FirstOrDefault(s => s == "en");
			if (SelectedFontSize == null)
				SelectedFontSize = FontSizeComboBoxItems[13];
			if (SelectedFontColor == null)
				SelectedFontColor = FontColorComboBoxItems[4];

			// Data is set so load datacontext
			DataContext = this;

			// Load last saved window size
			LoadWindowSize();
		}

		#endregion

		#region Window Event Handlers

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			// We are finished loading so populate saved values from config file
			LoadConfig();

			// Load is done so set flag to true
			loadFinished = true;
		}

		private void LoadWindowSize()
		{
			// Load the window size from settings
			if (Properties.Settings.Default.WindowHeight > 0)
			{
				this.Height = Properties.Settings.Default.WindowHeight;
			}

			if (Properties.Settings.Default.WindowWidth > 0)
			{
				this.Width = Properties.Settings.Default.WindowWidth;
			}
		}

		private void SaveWindowSize()
		{
			// Save the current window size to settings
			Properties.Settings.Default.WindowHeight = this.Height;
			Properties.Settings.Default.WindowWidth = this.Width;
			Properties.Settings.Default.Save();
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			QuickTranslateComboBox.MinWidth = QuickTranslateStackPanel.ActualWidth;
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			SaveWindowSize();
			SaveConfig();
		}

		#endregion

		#region Quick Translate ComboBox Event Handlers

		private async void QuickTranslateComboBox_SelectionChanged(object sender, RoutedEventArgs e)
		{
			if (loadFinished && SelectedQuickTranslate != null)
			{
				System.Windows.Forms.Clipboard.SetText(SelectedQuickTranslate.TranslatedText);
				StatusMessage = $"Quick translate copied to clipboard...";
				QuickTranslateComboBox.SelectedIndex = -1;
				await Task.Delay(messageDelay);
				StatusMessage = windowMessage;
			}
		}

		private async void QuickTranslateComboBoxRemoveItem_Click(object sender, RoutedEventArgs e)
		{
			if (sender is System.Windows.Controls.Button button && button.DataContext is TranslatedLine item)
			{
				if (GetRichTextBoxText(ManualTranslationTextBoxOutput) != string.Empty)
				{
					StatusMessage = $"Quick translate removed...";
					_quickTranslateComboBoxItems.Remove(item);
					QuickTranslateComboBox.SelectedIndex = -1;
					bool isEnabled = true;
					if (IsRichTextBoxEmpty(ManualTranslationTextBoxOutput))
						isEnabled = false;
					else
					{
						foreach (TranslatedLine tl in QuickTranslatefilteredItems)
						{
							if (GetRichTextBoxText(ManualTranslationTextBoxOutput) == tl.TranslatedText && !IsRichTextBoxEmpty(ManualTranslationTextBoxOutput))
							{
								isEnabled = false;
								break;
							}
						}
					}
					AddQuickTranslateButton.IsEnabled = isEnabled;
				}
				SaveConfig();
				await Task.Delay(messageDelay);
				StatusMessage = windowMessage;
			}
		}

		private async void QTOutputLanguageComboBox_SelectionChanged(object sender, EventArgs e)
		{
			if (SelectedOutputLanguage != null && SelectedSourceLanguage != null)
			{
				_quickTranslatefilteredItems.Filter = item => ((TranslatedLine)item).OutputLanguage == SelectedQTOutputLanguage && ((TranslatedLine)item).SourceLanguage == SelectedQTSourceLanguage;
			}
			else
			{
				_quickTranslatefilteredItems.Filter = null;
			}
			if (loadFinished)
			{
				StatusMessage = $"Quick translate from: changed to ''{SelectedQTOutputLanguage}'' ...";
				await Task.Delay(messageDelay);
				StatusMessage = windowMessage;
				SaveConfig();
			}
		}

		private async void QTSourceLanguageComboBox_SelectionChanged(object sender, EventArgs e)
		{
			if (SelectedSourceLanguage != null && SelectedSourceLanguage != null)
			{
				_quickTranslatefilteredItems.Filter = item => ((TranslatedLine)item).OutputLanguage == SelectedQTOutputLanguage && ((TranslatedLine)item).SourceLanguage == SelectedQTSourceLanguage;
			}
			else
			{
				_quickTranslatefilteredItems.Filter = null;
			}
			if (loadFinished)
			{
				StatusMessage = $"Quick translate to: changed to ''{SelectedQTSourceLanguage}'' ...";
				await Task.Delay(messageDelay);
				StatusMessage = windowMessage;
				SaveConfig();
			}
		}

		#endregion

		#region Ignore Chat ComboBox Event Handlers

		private void IgnoreChatComboBox_SelectionChanged(object sender, RoutedEventArgs e)
		{
			IgnoreChatComboBox.SelectedIndex = -1;
		}

		private async void IgnoreChatComboBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (IgnoreChatComboBox.Text != null && IgnoreChatComboBox.Text != string.Empty)
			{
				bool ignoreChatAlreadyAdded = false;
				var enteredItem = IgnoreChatComboBox.Text;
				foreach (var item in _ignoreChatComboBoxItems)
				{
					if (item.ChatChannel == enteredItem)
					{
						ignoreChatAlreadyAdded = true;
						break;
					}
				}
				if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(IgnoreChatComboBox.Text) && !ignoreChatAlreadyAdded)
				{
					var ignoreChatChannel = new IgnoreChat();
					ignoreChatChannel.ChatChannel = IgnoreChatComboBox.Text;
					_ignoreChatComboBoxItems.Add(ignoreChatChannel);
					StatusMessage = $"Ignore chat channel ''[{IgnoreChatComboBox.Text}]'' added...";
					IgnoreChatComboBox.Text = string.Empty;
					await Task.Delay(messageDelay);
					StatusMessage = windowMessage;
					SaveConfig();
				}
				else if (e.Key == Key.Enter && ignoreChatAlreadyAdded)
				{
					StatusMessage = $"Ignore chat channel ''[{IgnoreChatComboBox.Text}]'' already added...";
					IgnoreChatComboBox.Text = string.Empty;
					await Task.Delay(messageDelay);
					StatusMessage = windowMessage;
				}
			}
		}

		private async void IgnoreChatComboBoxRemoveItem_Click(object sender, RoutedEventArgs e)
		{
			if (sender is System.Windows.Controls.Button button && button.DataContext is IgnoreChat item)
			{
				StatusMessage = $"Ignore chat channel ''[{item.ChatChannel}]'' removed...";
				_ignoreChatComboBoxItems.Remove(item);
				SaveConfig();
				await Task.Delay(messageDelay);
				StatusMessage = windowMessage;
			}
		}

		private void IgnoreChatComboBox_Loaded(object sender, RoutedEventArgs e)
		{
			var comboBox = sender as System.Windows.Controls.ComboBox;
			if (comboBox != null)
			{
				var textBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as System.Windows.Controls.TextBox;
				if (textBox != null)
				{
					textBox.KeyDown += IgnoreChatComboBox_KeyDown;
				}
			}
		}

		#endregion

		#region Other ComboxBox Event Handlers

		private async void OutputLanguageComboBox_SelectionChanged(object sender, EventArgs e)
		{
			if (loadFinished)
			{
				StatusMessage = $"Translate to: changed to ''{SelectedOutputLanguage}'' ...";
				await Task.Delay(messageDelay);
				StatusMessage = windowMessage;
				SaveConfig();
			}
		}

		private async void SourceLanguageComboBox_SelectionChanged(object sender, EventArgs e)
		{
			if (loadFinished)
			{
				StatusMessage = $"Translate from: changed to ''{SelectedSourceLanguage}'' ...";
				await Task.Delay(messageDelay);
				StatusMessage = windowMessage;
				SaveConfig();
			}
		}

		private async void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (SelectedFontSize != null)
			{
				if (loadFinished)
				{
					// Update formatting  
					var fontSize = SelectedFontSize.Size;
					SaveConfig();
					StatusMessage = $"Font size changed to ''{fontSize}'' ...";
					await Task.Delay(messageDelay);
					StatusMessage = windowMessage;
				}
			}
		}

		private async void FontColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (SelectedFontColor != null)
			{
				// Update formatting
				System.Windows.Media.Color selectedColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(SelectedFontColor.Color);
				if (loadFinished)
				{
					SaveConfig();
					StatusMessage = $"Font color changed to ''{SelectedFontColor.Color}'' ...";
					await Task.Delay(messageDelay);
					StatusMessage = windowMessage;
				}
			}
		}

		#endregion

		#region Button Click Event Handlers

		private async void Keyword_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Hyperlink hyperlink)
			{
				string clickedText = new TextRange(hyperlink.ContentStart, hyperlink.ContentEnd).Text;
				string originalKeyword = hyperlink.Tag as string;
				System.Windows.Clipboard.SetText(originalKeyword);
				StatusMessage = $"Keyword ''{originalKeyword}'' copied to clipboard...";
				await Task.Delay(messageDelay);
				StatusMessage = windowMessage;
			}
		}

		private async void ResetAPICountButton_Click(object sender, RoutedEventArgs e)
		{
			// Clear API count
			APICount = "0";
			StatusMessage = $"API Count Reset...";
			await Task.Delay(messageDelay);
			StatusMessage = windowMessage;
		}

		private void BrowseSourceFolder_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new FolderBrowserDialog();
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				SourceFolder = dialog.SelectedPath;
			}
		}

		private void BrowseOutputFile_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new SaveFileDialog();
			dialog.Filter = "TXT files (*.txt)|*.txt";
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				OutputFileName = dialog.FileName;
				string? directoryPath = System.IO.Path.GetDirectoryName(dialog.FileName);
				if (directoryPath != null && directoryPath != "" && !Directory.Exists(directoryPath))
				{
					Directory.CreateDirectory(directoryPath);
				}
				if (!File.Exists(dialog.FileName))
				{
					File.Create(dialog.FileName).Dispose();
				}
			}
		}

		private async void TranslateButton_Click(object sender, RoutedEventArgs e)
		{
			if (IsTranslating)
				await StopTranslation();
			else if (!IsTranslating)
				await StartTranslation();
		}

		private async Task StopTranslation()
		{
			var apiLimitReached = CheckAPILimit();
			if (!apiLimitReached)
			{
				if (IsTranslating)
				{
					// Stop translating
					IsTranslating = false;
					TranslateButton.Content = "Start Translation";
					BrowseTranslatedFileButton.IsEnabled = true;
					BrowseLogFileButton.IsEnabled = true;
					SourceFolderTextBox.IsEnabled = true;
					SourceLanguageComboBox.IsEnabled = true;
					OutputLanguageComboBox.IsEnabled = true;
					OutputFileNameTextBox.IsEnabled = true;
					ApiKeyTextBox.IsEnabled = true;
					CharacterNameTextBox.IsEnabled = true;
					windowMessage = originalWindowMessage;
					StatusMessage = windowMessage;

					// Stop the log file watcher
				    await StopLogFileWatcher();

					// Stop test mode simulation timers
					if (testMode)
					{
						foreach (DispatcherTimer timer in timers)
						{
							timer.Stop();
						}
						timers.Clear();
					}
				}
			}
		}

		private async Task StartTranslation()
		{
			var apiLimitReached = CheckAPILimit();
			if (!apiLimitReached)
			{
				if (!IsTranslating)
				{
					// Start translating 
					IsTranslating = true;
					TranslateButton.Content = "Stop Translating";
					BrowseTranslatedFileButton.IsEnabled = false;
					BrowseLogFileButton.IsEnabled = false;
					SourceFolderTextBox.IsEnabled = false;
					SourceLanguageComboBox.IsEnabled = false;
					OutputLanguageComboBox.IsEnabled = false;
					OutputFileNameTextBox.IsEnabled = false;
					ApiKeyTextBox.IsEnabled = false;
					CharacterNameTextBox.IsEnabled = false;
					if (UseFreeAPI)
						windowMessage = "T4C Log Translator - Auto translating logs locally...";
					else
						windowMessage = "T4C Log Translator - Auto translating logs via google...";


					// Ensure the translated log file exists, if it doesnt create it
					string? directoryPath = System.IO.Path.GetDirectoryName(OutputFileName);
					if (directoryPath != null && directoryPath != "" && !Directory.Exists(directoryPath))
					{
						Directory.CreateDirectory(directoryPath);
					}
					if (!File.Exists(OutputFileName))
					{
						File.Create(OutputFileName).Dispose();
					}

					// Start the log file watcher
					if (UseFreeAPI)
					{
						var ensureContainerIsRunningAsync = await EnsureContainerIsRunningAsync();
						if (ensureContainerIsRunningAsync)
						{
							await StartLogFolderWatcher();
						}
					}
					else
					{
						await StartLogFolderWatcher();
					}
					StatusMessage = windowMessage;
				}
			}
		}

		private async void SaveConfigButton_Click(object sender, RoutedEventArgs e)
		{
			StatusMessage = $"Configuration saved...";
			await Task.Delay(messageDelay);
			StatusMessage = windowMessage;
			SaveConfig();
		}

		private async void ManuallyTranslate_Click(object sender, RoutedEventArgs e)
		{
			// Enable or disable the quick translate button based on the translated text
			string sourceLang = SelectedQTOutputLanguage;
			string targetLang = SelectedQTSourceLanguage;
			string inputText = GetRichTextBoxText(ManualTranslationTextBoxInput);

			bool goodToTranslate = false;
			if (UseFreeAPI)
			{
				goodToTranslate = await EnsureContainerIsRunningAsync();
			} 
			else
			{
				goodToTranslate = true;
			}
			if (goodToTranslate)
			{
				TranslatedLine TranslatedLine = await TranslateText(inputText.ToString(), targetLang, sourceLang, false);
				if (TranslatedLine is null)
					return;
				SetRichTextBoxText(ManualTranslationTextBoxOutput, TranslatedLine.TranslatedText);
				if (GetRichTextBoxText(ManualTranslationTextBoxOutput) != string.Empty)
				{
					bool isEnabled = true;
					if (IsRichTextBoxEmpty(ManualTranslationTextBoxOutput))
						isEnabled = false;
					else
					{
						foreach (TranslatedLine item in QuickTranslatefilteredItems)
						{
							if (GetRichTextBoxText(ManualTranslationTextBoxOutput) == item.TranslatedText && !IsRichTextBoxEmpty(ManualTranslationTextBoxOutput))
							{
								isEnabled = false;
								break;
							}
						}
					}
					AddQuickTranslateButton.IsEnabled = isEnabled;
				}

				// Show copied message after translation
				System.Windows.Forms.Clipboard.SetText(TranslatedLine.TranslatedText);
				StatusMessage = $"Manually translated text copied to clipboard...";
				await Task.Delay(messageDelay);
				StatusMessage = windowMessage;
			}
		}

		private async void ClearLogButton_Click(object sender, RoutedEventArgs e)
		{
			translatedLines.Clear();
			hyperlinkXamlNames.Clear();
			OutputLogRichTextBox.Document.Blocks.Clear();
			lastTranslatedLineTimestamp = DateTime.MinValue;
			lastTranslatedLine = string.Empty;
			StatusMessage = $"Output log cleared...";
			await Task.Delay(messageDelay);
			StatusMessage = windowMessage;
		}
		private void ManualTranslationTextBoxInput_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (IsRichTextBoxEmpty(ManualTranslationTextBoxInput))
			{
				ManuallyTranslateButton.IsEnabled = false;
			}
			else
			{
				ManuallyTranslateButton.IsEnabled = true;
			}
		}

		private async void AddQuickTranslateButton_Click(object sender, RoutedEventArgs e)
		{
			var translatedLine = new TranslatedLine();
			translatedLine.OutputLanguage = SelectedQTOutputLanguage;
			translatedLine.SourceLanguage = SelectedQTSourceLanguage;
			translatedLine.OriginalText = GetRichTextBoxText(ManualTranslationTextBoxInput);
			translatedLine.TranslatedText = GetRichTextBoxText(ManualTranslationTextBoxOutput);

			bool alreadyExists = false;
			foreach (TranslatedLine item in QuickTranslateComboBox.Items)
			{
				if (item.TranslatedText == translatedLine.TranslatedText)
				{
					alreadyExists = true;
					break;
				}
			}
			if (!alreadyExists)
			{
				_quickTranslateComboBoxItems.Add(translatedLine);
				StatusMessage = $"Quick translate added...";
				AddQuickTranslateButton.IsEnabled = false;
				await Task.Delay(messageDelay);
				StatusMessage = windowMessage;
				QuickTranslateComboBox.SelectedItem = null;
				SaveConfig();
			}
			else
			{
				StatusMessage = $"Quick translate already added...";
				AddQuickTranslateButton.IsEnabled = false;
				await Task.Delay(messageDelay);
				StatusMessage = windowMessage;
			}
		}

		#endregion

		#region Checkbox Event Handlers

		private void IgnoreChatMessagesCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			if (IsLoaded)
			{
				if (IgnoreChatComboBoxItems.Count == 0)
				{
					IgnoreChatMessages = false;
				}

				SaveConfig();
			}
		}

		private void IgnoreChatMessagesCheckBox_UnChecked(object sender, RoutedEventArgs e)
		{
			if (IsLoaded)
			{
				if (IgnoreChatComboBoxItems.Count == 0)
				{
					IgnoreChatMessages = false;
				}

				SaveConfig();
			}
		}

		private void ShowOriginalTextCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			if (IsLoaded)
			{
				SaveConfig();
			}
		}



		private void ShowOriginalTextCheckBox_UnChecked(object sender, RoutedEventArgs e)
		{
			if (IsLoaded)
			{
				SaveConfig();
			}
		}

		private void UseFreeAPICheckBox_Checked(object sender, RoutedEventArgs e)
		{
			if (IsLoaded)
			{
				if (UseFreeAPI)
				{
					ApiKeyTextBox.IsEnabled = false;
					_sourceLanguagefilteredItems.Filter = item => (string)item == "sq" || (string)item == "ar" || (string)item == "az" || (string)item == "bn" || (string)item == "bg" || (string)item == "ca" || (string)item == "zh" || (string)item == "zt" || (string)item == "cs" || (string)item == "da" || (string)item == "nl" || (string)item == "en" || (string)item == "eo" || (string)item == "et" || (string)item == "fi" || (string)item == "fr" || (string)item == "de" || (string)item == "el" || (string)item == "he" || (string)item == "hi" || (string)item == "hu" || (string)item == "id" || (string)item == "ga" || (string)item == "it" || (string)item == "ja" || (string)item == "ko" || (string)item == "lv" || (string)item == "lt" || (string)item == "ms" || (string)item == "nb" || (string)item == "fa" || (string)item == "pl" || (string)item == "pt" || (string)item == "ro" || (string)item == "ru" || (string)item == "sk" || (string)item == "sl" || (string)item == "es" || (string)item == "sv" || (string)item == "tl" || (string)item == "th" || (string)item == "tr" || (string)item == "uk" || (string)item == "ur";
					_outputLanguagefilteredItems.Filter = item => (string)item == "sq" || (string)item == "ar" || (string)item == "az" || (string)item == "bn" || (string)item == "bg" || (string)item == "ca" || (string)item == "zh" || (string)item == "zt" || (string)item == "cs" || (string)item == "da" || (string)item == "nl" || (string)item == "en" || (string)item == "eo" || (string)item == "et" || (string)item == "fi" || (string)item == "fr" || (string)item == "de" || (string)item == "el" || (string)item == "he" || (string)item == "hi" || (string)item == "hu" || (string)item == "id" || (string)item == "ga" || (string)item == "it" || (string)item == "ja" || (string)item == "ko" || (string)item == "lv" || (string)item == "lt" || (string)item == "ms" || (string)item == "nb" || (string)item == "fa" || (string)item == "pl" || (string)item == "pt" || (string)item == "ro" || (string)item == "ru" || (string)item == "sk" || (string)item == "sl" || (string)item == "es" || (string)item == "sv" || (string)item == "tl" || (string)item == "th" || (string)item == "tr" || (string)item == "uk" || (string)item == "ur";
				}
				else
				{
					ApiKeyTextBox.IsEnabled = true;
					_sourceLanguagefilteredItems.Filter = null;
					_outputLanguagefilteredItems.Filter = null;
				}
				if (isTranslating)
				{
					windowMessage = "T4C Log Translator - Auto translating logs locally...";
					StatusMessage = windowMessage;
				}
				APICount = "0"; // Reset API count we are moving to offline
				SaveConfig();
			}
		}

		private void UseFreeAPICheckBox_UnChecked(object sender, RoutedEventArgs e)
		{
			if (IsLoaded)
			{
				if (UseFreeAPI)
				{
					ApiKeyTextBox.IsEnabled = false;
					_sourceLanguagefilteredItems.Filter = item => (string)item == "sq" || (string)item == "ar" || (string)item == "az" || (string)item == "bn" || (string)item == "bg" || (string)item == "ca" || (string)item == "zh" || (string)item == "zt" || (string)item == "cs" || (string)item == "da" || (string)item == "nl" || (string)item == "en" || (string)item == "eo" || (string)item == "et" || (string)item == "fi" || (string)item == "fr" || (string)item == "de" || (string)item == "el" || (string)item == "he" || (string)item == "hi" || (string)item == "hu" || (string)item == "id" || (string)item == "ga" || (string)item == "it" || (string)item == "ja" || (string)item == "ko" || (string)item == "lv" || (string)item == "lt" || (string)item == "ms" || (string)item == "nb" || (string)item == "fa" || (string)item == "pl" || (string)item == "pt" || (string)item == "ro" || (string)item == "ru" || (string)item == "sk" || (string)item == "sl" || (string)item == "es" || (string)item == "sv" || (string)item == "tl" || (string)item == "th" || (string)item == "tr" || (string)item == "uk" || (string)item == "ur";
					_outputLanguagefilteredItems.Filter = item => (string)item == "sq" || (string)item == "ar" || (string)item == "az" || (string)item == "bn" || (string)item == "bg" || (string)item == "ca" || (string)item == "zh" || (string)item == "zt" || (string)item == "cs" || (string)item == "da" || (string)item == "nl" || (string)item == "en" || (string)item == "eo" || (string)item == "et" || (string)item == "fi" || (string)item == "fr" || (string)item == "de" || (string)item == "el" || (string)item == "he" || (string)item == "hi" || (string)item == "hu" || (string)item == "id" || (string)item == "ga" || (string)item == "it" || (string)item == "ja" || (string)item == "ko" || (string)item == "lv" || (string)item == "lt" || (string)item == "ms" || (string)item == "nb" || (string)item == "fa" || (string)item == "pl" || (string)item == "pt" || (string)item == "ro" || (string)item == "ru" || (string)item == "sk" || (string)item == "sl" || (string)item == "es" || (string)item == "sv" || (string)item == "tl" || (string)item == "th" || (string)item == "tr" || (string)item == "uk" || (string)item == "ur";

				}
				else
				{
					ApiKeyTextBox.IsEnabled = true;
					_sourceLanguagefilteredItems.Filter = null;
					_outputLanguagefilteredItems.Filter = null;
				}
				if (isTranslating)
				{
					windowMessage = "T4C Log Translator - Auto translating logs via google...";
					StatusMessage = windowMessage;
				}
				APICount = "0"; // Reset API count we are moving to offline
				SaveConfig();
			}
		}

		#endregion

		#region Log Folder Change Methods

		private async Task StartLogFolderWatcher()
		{
			if (testMode)
			{
				string testLogFileFolder = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\TestLogs";
				DispatcherTimer timer = new DispatcherTimer();
				timers.Add(timer);
				timer.Interval = TimeSpan.FromSeconds(2); // Simulate a log entry every x seconds
				timer.Start();
				timer.Tick += async (sender, e) =>
				{
					CheckAPILimit();
					string randomLine = string.Empty;
					var files = Directory.GetFiles(testLogFileFolder, "*.txt");
					if (files.Length != 0)
					{
						Random random = new Random();
						string randomFile = files[random.Next(files.Length)];
						var lines = File.ReadAllLines(randomFile, enc1252);
						if (lines.Length != 0)
							randomLine = lines[random.Next(lines.Length)];
					}
					if (randomLine != string.Empty)
					{
						if (TryExtractTimestamp(randomLine, out DateTime timestamp))
						{
							// Process the line
							await TranslateLogOutputLines(randomLine, timestamp);
						}
					}
				};
			}
			else
			{
				await Dispatcher.InvokeAsync(async () =>
				{
					try
					{
						// Stop the existing file watcher (if any)
						if (logFileWatcher != null)
						{
							logFileWatcher.Dispose();
						}

						// Start file watcher for log file changes in the folder
						logFileWatcher = new FileSystemWatcher
						{
							Path = SourceFolder,
							Filter = "*.txt",
							EnableRaisingEvents = true,
							NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
						};
						logFileWatcher.Changed += async (sender, e) =>
						{

							// Get the last write time of the file
							DateTime lastWriteTime = File.GetLastWriteTime(e.FullPath);

							// Calculate the time difference between the last write time and the current time
							TimeSpan timeDifference = DateTime.Now - lastWriteTime;

							// Only work with files that are 1 minute old or newer
							if (timeDifference.TotalMinutes <= 1)
							{
								try
								{
									using (FileStream fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
									{
										fs.Seek(lastReadFilePosition, SeekOrigin.Begin);
										using (StreamReader reader = new StreamReader(fs, enc1252, true))
										{
											string line;
											while ((line = await reader.ReadLineAsync()) != null)
											{
												if (TryExtractTimestamp(line, out DateTime timestamp))
												{
													TimeSpan timeDiff = DateTime.Now - timestamp;
													if (timeDiff.TotalMinutes <= 1) // Only translate lines that are 1 minute old or newer
													{
														// Process the line
														await TranslateLogOutputLines(line, timestamp);
													}
												}
											}

											// Update last line read position
											lastReadFilePosition = fs.Position;
										}
									}
								}

								// Log file is locked by another process, lets try again. The game client can sometimes lock the log files and we cannot stop that so we can only retry and wait for it to unlock
								catch (IOException ioEx) when (ioEx.HResult == unchecked((int)0x80070020))
								{
									logFileWatcher.Dispose();
									await StartLogFolderWatcher();
								}
								catch (Exception ex)
								{
									MessageBoxResult result = System.Windows.MessageBox.Show(ex.Message, "Log Reader Error - Do you want to retry?", MessageBoxButton.YesNo, MessageBoxImage.Error);
									if (result == MessageBoxResult.Yes)
									{
										await StopTranslation();
										await StartTranslation();
									}
									if (result == MessageBoxResult.No)
									{
										await StopTranslation();
									}
								}
							}
						};

					}
					catch (Exception ex)
					{
						MessageBoxResult result = System.Windows.MessageBox.Show(ex.Message, "Log Reader Error - Do you want to retry?", MessageBoxButton.YesNo, MessageBoxImage.Error);
						if (result == MessageBoxResult.Yes)
						{
							await StopTranslation();
							await StartTranslation();
						}
						if (result == MessageBoxResult.No)
						{
							await StopTranslation();
						}
					}
				});
			}
		}

		private async Task StopLogFileWatcher()
		{
			if (logFileWatcher != null)
			{
				logFileWatcher.Dispose();
			}
		}

		#endregion

		#region Main Translation Methods

		private async Task TranslateLogOutputLines(string source, DateTime lineTimestamp)
		{
			try
			{
				// If the last line translated was the same as this one, skip translation
				var isUniqueLine = false;
				if (lineTimestamp != lastTranslatedLineTimestamp && source != lastTranslatedLine)
				{
					isUniqueLine = true;
				}

				// If the line is unique, start processing it
				if (isUniqueLine)
				{
					// Set last translated timestamp
					lastTranslatedLineTimestamp = lineTimestamp;
					lastTranslatedLine = source;

					// Start preparing the log output line for translation
					var sourceText = source;

					// Create new translated line object
					var newTranslatedLine = new TranslatedLine();
					newTranslatedLine.Keywords = new List<Keyword>();
					newTranslatedLine.WordsofInterest = new List<WordofInterest>();

					// Cleanup the messy stuff from the text
					sourceText = CleanupLogString(sourceText);

					// Copy the full log text to the object
					newTranslatedLine.FullLogText = sourceText;

					// Local Chat - If the 23rd character of the string is a {
					var checkLocalChat = sourceText[22];
					if (checkLocalChat == '{')
					{
						newTranslatedLine.TranslatedLineType = translatedLineTypes.FirstOrDefault(i => i.Id == 1);
						newTranslatedLine.Name = GetFirstWordBetweenCurlyBraces(sourceText);
						newTranslatedLine.OriginalText = StripBeforeColonAndTrimQuotes(sourceText);
					}

					// If the 23rd character of the string is a [ it can be a chat channel or a server chat
					var checkChatChannel = sourceText[22];
					if (checkChatChannel == '[')
					{
						var checkForPlayerName = GetFirstWordBetweenBrackets(sourceText);
						if (checkForPlayerName[0] == '"') // If the first letter in the [ is a " its a player name if not it is a server chat message
						{
							// Chat Channel
							newTranslatedLine.TranslatedLineType = translatedLineTypes.FirstOrDefault(i => i.Id == 2);
							newTranslatedLine.Name = GetFirstWordBetweenQuotesAfterBrackets(sourceText);
							newTranslatedLine.Channel = GetFirstWordBetweenBracketsAndQuotes(sourceText);
							newTranslatedLine.OriginalText = StripBeforeColonAndTrim(sourceText);
						}
						else
						{
							// Server Chat
							newTranslatedLine.TranslatedLineType = translatedLineTypes.FirstOrDefault(i => i.Id == 3);
							newTranslatedLine.Channel = GetFirstWordBetweenBrackets(sourceText);
							newTranslatedLine.OriginalText = StripBeforeBracketAndTrim(sourceText);
						}
					}

					// Server Message - If the 23rd character of the string is not a [ and not a {
					var checkServerMessage = sourceText[22];
					if (checkServerMessage != '[' && checkServerMessage != '{')
					{
						newTranslatedLine.TranslatedLineType = translatedLineTypes.FirstOrDefault(i => i.Id == 4);
						newTranslatedLine.OriginalText = StripBeforeDoubleDashAndTrim(sourceText);
					}

					// If line starts with You or Macro or Screenshot or "Mouse already in use" ignore it, otherwise add to the list
					string[] getWords = newTranslatedLine.OriginalText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (!getWords[0].Equals("You", StringComparison.OrdinalIgnoreCase) && !getWords[0].Equals("Macro", StringComparison.OrdinalIgnoreCase) && !getWords[0].Equals("Paged", StringComparison.OrdinalIgnoreCase) && !getWords[0].Equals("AutoMove", StringComparison.OrdinalIgnoreCase) && !getWords[0].Equals("Mouse", StringComparison.OrdinalIgnoreCase) && !getWords[0].Equals("Trade", StringComparison.OrdinalIgnoreCase) && !getWords[0].Equals("Status", StringComparison.OrdinalIgnoreCase) && !getWords[0].Equals("The", StringComparison.OrdinalIgnoreCase) && !getWords[0].Equals("Screenshot", StringComparison.OrdinalIgnoreCase))
					{
						string timeOnly = lineTimestamp.ToString("hh:mm:ss tt");
						string lineWithoutDate = sourceText.Replace(lineTimestamp.ToString("MM/dd/yyyy HH:mm:ss"), timeOnly);
						newTranslatedLine.SourceLanguage = SelectedSourceLanguage;
						newTranslatedLine.Time = timeOnly;
						newTranslatedLine.IsTranslated = false;
						newTranslatedLine.OriginalText = newTranslatedLine.OriginalText;
						translatedLines.Add(newTranslatedLine);
					}

					var linesToTranslate = translatedLines.ToList().Where(i => !i.IsTranslated);
					foreach (TranslatedLine line in linesToTranslate)
					{
						// If the line is a chat message and the channel is in the ignore list, skip translation
						if (line.Channel != null && IgnoreChatMessages)
						{
							foreach (IgnoreChat ignoreChat in _ignoreChatComboBoxItems)
							{
								if (line.Channel == ignoreChat.ChatChannel)
								{
									// Remove from the list, this is in the ignore list
									translatedLines.Remove(line);
									continue; // Exit this loop, we dont need to translate this line it is in the ignore list
								}
							}
						}

						// If the line is a chat message or a local message and the name is character name, skip translation
						if (line.Name != null && line.Name != "*" && CharacterName != "")
						{
							if (line.Name.ToLower() == CharacterName.ToLower())
							{
								// Remove from the list, this is the character name
								translatedLines.Remove(line);
								continue; // Exit this loop, we dont need to translate this line it is the character name
							}
						}

						// If there are more than 8 words, first check the language of the text
						var words = line.OriginalText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						if (line.OriginalText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length > 8)
						{
							var firstWords = string.Join(" ", words.Take(8));
							TranslatedLine checklanguage = await TranslateText(firstWords, SelectedOutputLanguage, SelectedSourceLanguage, true);
							if (checklanguage is null || checklanguage.TranslatedText == string.Empty)
							{
								// Remove from the list, there was an error in the translation API
								translatedLines.Remove(line);
								continue; // Exit this loop, we dont need to translate this line there was an error
							}
							if (SelectedOutputLanguage == checklanguage.Detectedlanguage)
							{
								// Remove from the list, this is already in our target language no need to translate
								translatedLines.Remove(line);
								continue; // Exit this loop, we dont need to translate this line it is already in our output language
							}
						}

						// Translate the line it passes all the checks and neeeds to go to the API
						var wordCount = words.Count();
						bool detectLanguage = false;

						// If there are 5 or more words, auto detect the language
						if (wordCount >= 5)
						{
							detectLanguage = true;
						}
						TranslatedLine translatedLine = await TranslateText(line.OriginalText, SelectedOutputLanguage, SelectedSourceLanguage, detectLanguage);
						if (translatedLine is null || translatedLine.TranslatedText == string.Empty)
						{
							// Remove from the list, there was an error in the translation API
							translatedLines.Remove(line);
							continue; // Exit this loop, we dont need to translate this line there was an error
						}
						if (SelectedOutputLanguage == translatedLine.Detectedlanguage)
						{
							// Remove from the list, this is already in our target language no need to translate
							translatedLines.Remove(line);
							continue; // Exit this loop, we dont need to translate this line it is already in our output language
						}
						line.OutputLanguage = translatedLine.OutputLanguage;
						line.SourceLanguage = translatedLine.SourceLanguage;
						line.TranslatedText = translatedLine.TranslatedText;

						// Add any text wrapped in "" to Keywords in a pair with the original and translated versions of the words
						List<string> originalKeywords = GetWordsInQuotes(line.OriginalText);
						List<string> translatedKeywords = GetWordsInQuotes(line.TranslatedText);
						int keywordCount = Math.Min(originalKeywords.Count, translatedKeywords.Count);
						for (int i = 0; i < keywordCount; i++)
						{
							line.Keywords.Add(new Keyword() { OriginalKeyword = originalKeywords[i], TranslatedKeyword = translatedKeywords[i], HyperlinkAdded = false });
						}

						// Add any text wrapped in {} to Words of Interest in a pair with the original and translated versions of the words
						List<string> originalWordsofInterest = GetWordsInCurlyBraces(line.OriginalText);
						List<string> translatedWordsofInterest = GetWordsInCurlyBraces(line.TranslatedText);
						int woiCount = Math.Min(originalWordsofInterest.Count, translatedWordsofInterest.Count);
						for (int i = 0; i < woiCount; i++)
						{
							line.WordsofInterest.Add(new WordofInterest() { OriginalWordofInterest = originalWordsofInterest[i], TranslatedWordofInterest = translatedWordsofInterest[i] });
						}
						line.IsTranslated = true;
					}

					// Refresh the output log
					var linesToProcess = translatedLines.Where(i => i.IsTranslated && !i.IsDisplayed);
					foreach (TranslatedLine line in linesToProcess)
					{
						// Copy the original and translated text to the XAML version
						line.OriginalTextXaml = line.OriginalText;
						line.TranslatedTextXaml = line.TranslatedText;

						// Log Output formatting and display
						await Dispatcher.InvokeAsync(() =>
						{
							// Translated text create wrapping paragraph
							var translatedParagraph = new Paragraph();
							translatedParagraph.Margin = new Thickness(0, 0, 0, 0);

							// Create textblock for the translated text
							var translatedTextBlock = new TextBlock();
							translatedTextBlock.SetBinding(TextBlock.FontSizeProperty, new System.Windows.Data.Binding("SelectedFontSize.Size"));
							translatedTextBlock.SetBinding(TextBlock.ForegroundProperty, new System.Windows.Data.Binding("SelectedFontColor.Color"));
							translatedTextBlock.FontFamily = new System.Windows.Media.FontFamily("Consolas");
							translatedTextBlock.TextWrapping = TextWrapping.WrapWithOverflow;
							translatedTextBlock.Inlines.Add(new Run($"{line.OutputLanguage.ToUpper()} {line.Time} "));

							// If the line is a Chat Channel or Server Chat create a hyperlink and show the channel name
							if (line.TranslatedLineType.Type == "Chat Channel" || line.TranslatedLineType.Type == "Server Chat")
							{
								// Create hyperlink and text for channel
								var channelHyperlink = new Hyperlink
								{
									Foreground = System.Windows.Media.Brushes.White,
									Tag = line.Channel,
									TextDecorations = null
								};
								if (line.TranslatedLineType.Type == "Server Chat")
								{
									channelHyperlink.Inlines.Add(new Run($"[{line.Channel}]"));
								}
								else
								{
									channelHyperlink.Inlines.Add(new Run($"[{line.Channel}] "));
								}
								channelHyperlink.Click += Keyword_Click;
								translatedTextBlock.Inlines.Add(channelHyperlink);
							}

							// If the line is a Local Chat or Chat Channel or Server Chat create a hyperlink and show the player name
							if (line.TranslatedLineType.Type == "Local Chat" || line.TranslatedLineType.Type == "Chat Channel" || line.TranslatedLineType.Type == "Server Chat")
							{
								var nameHyperlink = new Hyperlink
								{
									Foreground = System.Windows.Media.Brushes.White,
									Tag = line.Name,
									TextDecorations = null
								};
								nameHyperlink.Inlines.Add(new Run($"{line.Name}: "));

								nameHyperlink.Click += Keyword_Click;
								translatedTextBlock.Inlines.Add(nameHyperlink);
							}

							// If there are no keywords in the line just bring over the text
							if (line.Keywords.Count == 0 && line.WordsofInterest.Count == 0)
								translatedTextBlock.Inlines.Add(new Run(RemoveMatchedCurlyBrackets(RemoveMatchedDoubleQuotes(line.TranslatedTextXaml))));
							// There are keywords and/or words of interest so format them in the output log
							else
								FormatKeywords(translatedTextBlock, RemoveMatchedCurlyBrackets(RemoveMatchedDoubleQuotes(line.TranslatedTextXaml)), line.Keywords, line.WordsofInterest);

							// Update the paragraph with the textblock
							translatedParagraph.Inlines.Add(translatedTextBlock);

							// Original text create wrapping paragraph
							var originalParagraph = new Paragraph();
							originalParagraph.Margin = new Thickness(0, 0, 0, 0);

							// Create textblock for the original text
							var originalTextBlock = new TextBlock();
							originalTextBlock.SetBinding(TextBlock.FontSizeProperty, new System.Windows.Data.Binding("SelectedFontSize.Size"));
							originalTextBlock.SetBinding(TextBlock.ForegroundProperty, new System.Windows.Data.Binding("SelectedFontColorLight"));
							originalTextBlock.FontFamily = new System.Windows.Media.FontFamily("Consolas");
							originalTextBlock.TextWrapping = TextWrapping.WrapWithOverflow;
							originalTextBlock.Inlines.Add(new Run($"{line.SourceLanguage.ToUpper()} {line.Time} "));

							// If the line is a Chat Channel or Server Chat create a hyperlink and show the channel name
							if (line.TranslatedLineType.Type == "Chat Channel" || line.TranslatedLineType.Type == "Server Chat")
							{
								// Create hyperlink and text for channel
								var channelHyperlink = new Hyperlink
								{
									Foreground = System.Windows.Media.Brushes.White,
									Tag = line.Channel,
									TextDecorations = null
								};
								if (line.TranslatedLineType.Type == "Server Chat")
								{
									channelHyperlink.Inlines.Add(new Run($"[{line.Channel}]"));
								}
								else
								{
									channelHyperlink.Inlines.Add(new Run($"[{line.Channel}] "));
								}
								channelHyperlink.Click += Keyword_Click;
								originalTextBlock.Inlines.Add(channelHyperlink);
							}

							// If the line is a Local Chat or Chat Channel or Server Chat create a hyperlink and show the player name
							if (line.TranslatedLineType.Type == "Local Chat" || line.TranslatedLineType.Type == "Chat Channel" || line.TranslatedLineType.Type == "Server Chat")
							{
								var nameHyperlink = new Hyperlink
								{
									Foreground = System.Windows.Media.Brushes.White,
									Tag = line.Name,
									TextDecorations = null
								};
								nameHyperlink.Inlines.Add(new Run($"{line.Name}: "));

								nameHyperlink.Click += Keyword_Click;
								originalTextBlock.Inlines.Add(nameHyperlink);
							}

							// There are keywords and/or words of interest so format them in the output log
							if (line.Keywords.Count == 0 && line.WordsofInterest.Count == 0)
								originalTextBlock.Inlines.Add(new Run(RemoveMatchedCurlyBrackets(RemoveMatchedDoubleQuotes(line.OriginalTextXaml))));
							// There are keywords and/or words of interest so format them in the output log
							else
								FormatKeywords(originalTextBlock, RemoveMatchedCurlyBrackets(RemoveMatchedDoubleQuotes(line.OriginalTextXaml)), line.Keywords, line.WordsofInterest);

							// Update the paragraph with the textblock
							originalParagraph.Inlines.Add(originalTextBlock);

							// If output log is null add ShowOriginalText is false, show only original text to the top of the output log
							if (OutputLogRichTextBox.Document.Blocks.FirstBlock == null && !ShowOriginalText)
							{
								OutputLogRichTextBox.Document.Blocks.Add(translatedParagraph);
							}
							// If output log is null and ShowOriginalText is true, show both original and translated text to the top of the output log
							else if (OutputLogRichTextBox.Document.Blocks.FirstBlock == null && ShowOriginalText)
							{
								OutputLogRichTextBox.Document.Blocks.Add(translatedParagraph);
								OutputLogRichTextBox.Document.Blocks.Add(originalParagraph);
							}
							// If output log is not null, show both original and translated text to the top of the output log
							else if (OutputLogRichTextBox.Document.Blocks.FirstBlock != null && ShowOriginalText)
							{
								OutputLogRichTextBox.Document.Blocks.InsertBefore(OutputLogRichTextBox.Document.Blocks.FirstBlock, translatedParagraph);
								OutputLogRichTextBox.Document.Blocks.InsertAfter(translatedParagraph, originalParagraph);
								OutputLogRichTextBox.Document.Blocks.InsertAfter(originalParagraph, new Paragraph() { Margin = new Thickness(0, 0, 0, 0) }); // Spacer paragraph
							}
							// If output log is not null, show only translated paragraph to the top of the output log
							else if (OutputLogRichTextBox.Document.Blocks.FirstBlock != null && !ShowOriginalText)
							{
								OutputLogRichTextBox.Document.Blocks.InsertBefore(OutputLogRichTextBox.Document.Blocks.FirstBlock, translatedParagraph);
								OutputLogRichTextBox.Document.Blocks.InsertAfter(translatedParagraph, new Paragraph() { Margin = new Thickness(0, 0, 0, 0) }); // Spacer paragraph
							}
						});

						// Write the outputlog to the outputlog text file
						if (!line.IsDisplayed)
						{
							// Check if the outputlog file exists, if not create it
							if (!File.Exists(OutputFileName))
							{
								File.Create(OutputFileName).Dispose();
							}

							// Insert this line to the outputlog text file on the top
							string existingContent = File.Exists(OutputFileName) ? File.ReadAllText(OutputFileName) : string.Empty;

							// Build dynamic line based on the type of log line
							string newLine = string.Empty;
							switch (line.TranslatedLineType.Id)
							{
								case 1: // Local Chat
									{
										newLine = $"{line.OutputLanguage.ToUpper()}-{line.Time} {line.Name}: {line.TranslatedText}\n{line.SourceLanguage.ToUpper()}-{line.Time} {line.Name}: {line.OriginalText}";
										break;
									}
								case 2: // Chat Channel
									{
										newLine = $"{line.OutputLanguage.ToUpper()}-{line.Time} [{line.Channel}] {line.Name}: {line.TranslatedText}\n{line.SourceLanguage.ToUpper()}-{line.Time} [{line.Channel}] {line.Name}: {line.OriginalText}";
										break;

									}
								case 3: // Server Chat
									{
										newLine = $"{line.OutputLanguage.ToUpper()}-{line.Time} [{line.Channel}] {line.Name}: {line.TranslatedText}\n{line.SourceLanguage.ToUpper()}-{line.Time} [{line.Channel}] {line.Name}: {line.OriginalText}";
										break;
									}
								case 4: // Server Message
									{
										newLine = $"{line.OutputLanguage.ToUpper()}-{line.Time} {line.TranslatedText}\n{line.SourceLanguage.ToUpper()}-{line.Time} {line.OriginalText}";
										break;
									}
							}

							// Combine the new text with the existing content
							string updatedContent = newLine + Environment.NewLine + existingContent;

							// Write the combined content back to the log file
							File.WriteAllText(OutputFileName, updatedContent);
						}

						// Set is displayed flag so we dont process this line again
						line.IsDisplayed = true;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBoxResult result = System.Windows.MessageBox.Show($"TranslateLogOutputLines() Error: {ex.Message}", "TranslateLogOutputLines() Error - Do you want to retry?", MessageBoxButton.YesNo, MessageBoxImage.Error);
				if (result == MessageBoxResult.Yes)
				{
					await StopTranslation();
					await StartTranslation();
				}
				if (result == MessageBoxResult.No)
				{
					await StopTranslation();
				}
			}
		}

		private async Task<TranslatedLine> TranslateText(string originalText, string outputLanguage, string sourceLanguage, bool detectlanguage)
		{
			try
			{
				// Increase our API counter
				APICount = (int.Parse(APICount) + originalText.Length).ToString();

				// Strip out \ characters before we send to translate API
				originalText = originalText.Replace("\\", "");
				originalText = originalText.Trim();
				string apiKey = APIKey;

				if (!UseFreeAPI)
				{
					// Use google translate API to translate the text
					string url = $"https://translation.googleapis.com/language/translate/v2?key={apiKey}";
					using (HttpClient client = new HttpClient())
					{
						dynamic requestBody;
						if (detectlanguage)
						{
							requestBody = new
							{
								q = originalText,
								target = outputLanguage,
								format = "html"
							};
						}
						else
						{
							requestBody = new
							{
								q = originalText,
								source = sourceLanguage,
								target = outputLanguage,
								format = "html"
							};
						}
						var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.Default, "application/json");
						HttpResponseMessage response = await client.PostAsync(url, content);
						string jsonResponse = await response.Content.ReadAsStringAsync();
						var translatedLine = new TranslatedLine();
						if (!response.IsSuccessStatusCode)
						{
							await StopTranslation();
							translatedLine.SourceLanguage = sourceLanguage;
							translatedLine.OutputLanguage = outputLanguage;
							translatedLine.OriginalText = originalText.Trim();
							translatedLine.TranslatedText = string.Empty;
							MessageBoxResult result = System.Windows.MessageBox.Show($"API Error: {response.StatusCode} - {jsonResponse}", "Translation API Error - Do you want to retry?", MessageBoxButton.YesNo, MessageBoxImage.Error);
							if (result == MessageBoxResult.Yes)
							{
								await StartTranslation();
							}
							return null;
						}
						JObject json = JObject.Parse(jsonResponse);
						var responseJSON = json["data"]?["translations"]?[0]?["translatedText"]?.ToString() ?? "Translation failed";
						var detectedLanguage = json["data"]?["translations"]?[0]?["detectedSourceLanguage"]?.ToString();
						if (detectlanguage)
							translatedLine.SourceLanguage = detectedLanguage;
						else
							translatedLine.SourceLanguage = sourceLanguage;
						translatedLine.OutputLanguage = outputLanguage;
						translatedLine.Detectedlanguage = detectedLanguage;
						translatedLine.OriginalText = originalText.Trim();
						translatedLine.TranslatedText = CleanupLogString(System.Net.WebUtility.HtmlDecode(responseJSON.Trim()));

						// Return the object
						return translatedLine;
					}
				}
				else
				{
					// Use the free translation API option on libre translate docker container to translate the text
					string url = "http://localhost:5000/translate";
					using (HttpClient client = new HttpClient())
					{
						dynamic requestBody;
						if (detectlanguage)
						{
							requestBody = new
							{
								q = originalText,
								source = "auto",
								target = outputLanguage,
								format = "html",
								alternatives = 3,
								api_key = ""
							};
						}
						else
						{
							requestBody = new
							{
								q = originalText,
								source = sourceLanguage,
								target = outputLanguage,
								format = "html",
								alternatives = 3,
								api_key = ""
							};
						}
						var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.Default, "application/json");
						HttpResponseMessage response = await client.PostAsync(url, content);
						string jsonResponse = await response.Content.ReadAsStringAsync();
						var translatedLine = new TranslatedLine();
						if (!response.IsSuccessStatusCode)
						{
							await StopTranslation();
							translatedLine.SourceLanguage = sourceLanguage;
							translatedLine.OutputLanguage = outputLanguage;
							translatedLine.OriginalText = originalText.Trim();
							translatedLine.TranslatedText = string.Empty;
							MessageBoxResult result = System.Windows.MessageBox.Show($"API Error: {response.StatusCode} - {jsonResponse}", "Translation API Error - Do you want to retry?", MessageBoxButton.YesNo, MessageBoxImage.Error);
							if (result == MessageBoxResult.Yes)
							{
								await StartTranslation();
							}
							return null;
						}
						TranslationResponse translationResponse = JsonConvert.DeserializeObject<TranslationResponse>(jsonResponse);
						var detectedLanguage = translationResponse.DetectedLanguage?.Language;
						if (detectlanguage)
							translatedLine.SourceLanguage = detectedLanguage;
						else
							translatedLine.SourceLanguage = sourceLanguage;
						translatedLine.OutputLanguage = outputLanguage;
						translatedLine.Detectedlanguage = detectedLanguage;
						translatedLine.OriginalText = originalText.Trim();
						translatedLine.TranslatedText = CleanupLogString(System.Net.WebUtility.HtmlDecode(translationResponse.TranslatedText.Trim()));

						// Return the object
						return translatedLine;
					}
				}
			}
			catch (Exception ex)
			{
				await StopTranslation();
				MessageBoxResult result = System.Windows.MessageBox.Show($"API Error: {ex.Message} - {ex.InnerException?.Message}", "Translation API Error - Do you want to retry?", MessageBoxButton.YesNo, MessageBoxImage.Error);
				if (result == MessageBoxResult.Yes)
				{
					await StartTranslation();
				}
				return null;
			}
		}

		#endregion

		#region Config Methods

		private void SaveConfig()
		{
			// Check for null in combobox items
			var ignoreChatList = string.Empty;
			var quickTranslateList = string.Empty;
			if (JsonConvert.SerializeObject(IgnoreChatComboBoxItems) != null && JsonConvert.SerializeObject(IgnoreChatComboBoxItems) != string.Empty)
				ignoreChatList = JsonConvert.SerializeObject(IgnoreChatComboBoxItems);
			if (JsonConvert.SerializeObject(QuickTranslateComboBoxItems) != null && JsonConvert.SerializeObject(QuickTranslateComboBoxItems) != string.Empty)
				quickTranslateList = JsonConvert.SerializeObject(QuickTranslateComboBoxItems);

			// Create config object
			var config = new
			{
				CharacterName = CharacterName,
				SourceFolder = SourceFolder,
				SourceLanguage = SelectedSourceLanguage,
				OutputLanguage = SelectedOutputLanguage,
				qtSourceLanguage = SelectedQTSourceLanguage,
				qtOutputLanguage = SelectedQTOutputLanguage,
				OutputFileName = OutputFileName,
				APIKey = APIKey,
				FontSize = SelectedFontSize,
				FontColor = SelectedFontColor,
				IgnoreChat = IgnoreChatMessages,
				UseFreeAPIBool = UseFreeAPI,
				ShowOriginalTextBool = ShowOriginalText,
				TotalAPICount = APICount,
				APILimit = APILimit,
				IgnoreChatList = ignoreChatList,
				QuickTranslateList = quickTranslateList
			};
			try
			{
				File.WriteAllText(configFilePath, Newtonsoft.Json.JsonConvert.SerializeObject(config));
			}
			catch (Exception ex)
			{
				// The file is locked or inaccessible
			}
		}

		public void LoadConfig()
		{
			if (File.Exists(configFilePath))
			{
				// Set default values for comboboxes before we load from config
				SelectedSourceLanguage = SourceLanguageComboBoxItems.FirstOrDefault(s => s == "fr");
				SelectedOutputLanguage = OutputLanguageComboBoxItems.FirstOrDefault(s => s == "en");
				SelectedQTSourceLanguage = SourceLanguageComboBoxItems.FirstOrDefault(s => s == "fr");
				SelectedQTOutputLanguage = OutputLanguageComboBoxItems.FirstOrDefault(s => s == "en");

				// Load config values saved in our config.json file
				var config = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(configFilePath));
				CharacterName = config?.CharacterName ?? @"YourName";
				SourceFolder = config?.SourceFolder ?? @"C:\Users\yourusername\AppData\Local\Dialsoft\Logs";
				if (config.OutputLanguage != null)
					SelectedOutputLanguage = OutputLanguageComboBoxItems.FirstOrDefault(s => s == (string)config.OutputLanguage);
				else
					SelectedOutputLanguage = OutputLanguageComboBoxItems.FirstOrDefault(s => s == "en");

				if (config.SourceLanguage != null)
					SelectedSourceLanguage = SourceLanguageComboBoxItems.FirstOrDefault(s => s == (string)config.SourceLanguage);
				else
					SelectedSourceLanguage = SourceLanguageComboBoxItems.FirstOrDefault(s => s == "fr");

				// Quick translate combobox items
				if (config.qtOutputLanguage != null)
					SelectedQTOutputLanguage = OutputLanguageComboBoxItems.FirstOrDefault(s => s == (string)config.qtOutputLanguage);
				else
					SelectedQTOutputLanguage = OutputLanguageComboBoxItems.FirstOrDefault(s => s == "en");

				if (config.qtSourceLanguage != null)
					SelectedQTSourceLanguage = SourceLanguageComboBoxItems.FirstOrDefault(s => s == (string)config.qtSourceLanguage);
				else
					SelectedQTSourceLanguage = SourceLanguageComboBoxItems.FirstOrDefault(s => s == "fr");

				OutputFileName = config?.OutputFileName ?? System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + logFilePath;
				APIKey = config?.APIKey ?? string.Empty;
				IgnoreChatMessages = config?.IgnoreChat ?? false;
				ShowOriginalText = config?.ShowOriginalTextBool ?? false;
				UseFreeAPI = config?.UseFreeAPIBool ?? false;
				APILimit = config?.APILimit ?? "450000";
				APICount = config?.TotalAPICount ?? int.Parse("0");

				// Translate combobox items from JSON config file (database)
				var quickTranslateListJson = config?.QuickTranslateList?.ToString();
				var quickTranslateItems = !string.IsNullOrEmpty(quickTranslateListJson)
					? JsonConvert.DeserializeObject<ObservableCollection<TranslatedLine>>(quickTranslateListJson)
					: new ObservableCollection<TranslatedLine>();
				QuickTranslateComboBoxItems.Clear();
				if (quickTranslateItems != null)
				{
					foreach (var item in quickTranslateItems)
					{
						QuickTranslateComboBoxItems.Add(item);
					}
				}
				var ignoreChatListJson = config?.IgnoreChatList?.ToString();
				var ignoreChatListItems = !string.IsNullOrEmpty(ignoreChatListJson)
					? JsonConvert.DeserializeObject<ObservableCollection<IgnoreChat>>(ignoreChatListJson)
					: new ObservableCollection<IgnoreChat>();
				IgnoreChatComboBoxItems.Clear();
				if (ignoreChatListItems != null)
				{
					foreach (var item in ignoreChatListItems)
					{
						IgnoreChatComboBoxItems.Add(item);
					}
				}

				// Clear selected items from comboboxes
				IgnoreChatComboBox.SelectedIndex = -1;
				QuickTranslateComboBox.SelectedIndex = -1;

				// Set selected color combobox
				FontColor configColor = config?.FontColor != null ? config.FontColor.ToObject<FontColor>() : FontColorComboBoxItems[4];
				foreach (FontColor item in FontColorComboBoxItems)
				{
					if (item.Color == configColor.Color)
					{
						var selectedItem = FontColorComboBoxItems.AsQueryable().FirstOrDefault(i => i.Color == configColor.Color);
						SelectedFontColor = selectedItem;
						break;
					}
				}

				// Set selected font size combobox
				FontSize configSize = config?.FontSize != null ? config.FontSize.ToObject<FontSize>() : FontSizeComboBoxItems[13];
				foreach (FontSize item in FontSizeComboBoxItems)
				{
					if (item.Size == configSize.Size)
					{
						var selectedItem = FontSizeComboBoxItems.AsQueryable().FirstOrDefault(i => i.Size == configSize.Size);
						SelectedFontSize = selectedItem;
						break;
					}
				}
			}
		}

		#endregion

		#region Helper Methods

		public async Task <bool> EnsureContainerIsRunningAsync()
		{
			try
			{
				using (var client = new DockerClientConfiguration(new Uri(DockerUri)).CreateClient())
				{
					bool isDockerRunning = await IsDockerDesktopRunningAsync();
					if (isDockerRunning)
					{

						// Check if the container is already running
						var containers = await client.Containers.ListContainersAsync(new ContainersListParameters() { All = true });
						var container = containers.FirstOrDefault(c => c.Names.Contains("/" + ContainerName));

						if (container == null)
						{
							// Pull the image if it does not exist
							StatusMessage = $"This may take a while, docker container not found, attempting to install please wait...";
							await client.Images.CreateImageAsync(new ImagesCreateParameters { FromImage = ImageName, Tag = "latest" }, null, new Progress<JSONMessage>());
							await ShowFakeProgressBarAsync("This may take a while, docker container not found, attempting to install please wait...", 10);

							// Create and start the container
							await client.Containers.CreateContainerAsync(new CreateContainerParameters
							{
								Image = ImageName,
								Name = ContainerName,
								HostConfig = new HostConfig
								{
									PortBindings = new Dictionary<string, IList<PortBinding>>
						{
							{ "5000/tcp", new List<PortBinding> { new PortBinding { HostPort = "5000" } } }
						}
								}
							});

							StatusMessage = $"Container was installed, downloading language packs, this may take up to 10 minutes...";
							await client.Containers.StartContainerAsync(ContainerName, new ContainerStartParameters());
							await ShowFakeProgressBarAsync("Container was installed, downloading language packs, this may take up to 10 minutes...", 600); // 10 minutes delay to let language packs install and for service to start
						}
						else if (container.State != "running")
						{
							if (container.State == "paused")
							{
								// Unpause the container if it is not running and is paused
								StatusMessage = $"Container is paused, attempting to start libre translate docker container please wait...";
								await client.Containers.UnpauseContainerAsync(container.ID);
								await ShowFakeProgressBarAsync("Container is paused, attempting to start libre translate docker container please wait...", 20); // 20 seconds delay to let container start up
							} 
							else
							{
								// Start the container if it is not running
								StatusMessage = $"Container not running, attempting to start libre translate docker container please wait...";
								await client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
								await ShowFakeProgressBarAsync("Container not running, attempting to start libre translate docker container please wait..", 20); // 20 seconds delay to let container start up
							}
						}
						else
						{
							// Container already running
						}
						return true;
					}
					else
					{
						MessageBoxResult result = System.Windows.MessageBox.Show("Docker Desktop not found, are you sure it is installed and running?", "Docker Error - Do you want to retry?", MessageBoxButton.YesNo, MessageBoxImage.Error);
						if (result == MessageBoxResult.Yes)
						{
							await StopTranslation();
							await StartTranslation();
						}
						if (result == MessageBoxResult.No)
						{
							await StopTranslation();
						}
						return false;
					}
				} 
			}
			catch (Exception ex)
			{
				MessageBoxResult result = System.Windows.MessageBox.Show(ex.Message, "Libre Translate Docker Error - Do you want to retry?", MessageBoxButton.YesNo, MessageBoxImage.Error);
				if (result == MessageBoxResult.Yes)
				{
					await StopTranslation();
					await StartTranslation();
				}
				if (result == MessageBoxResult.No)
				{
					await StopTranslation();
				}
				return false;
			}

		}

		private void FormatKeywords(TextBlock textBlock, string text, List<Keyword> keywords, List<WordofInterest> wordsofInterest)
		{
			// Combine keywords and words of interest into a single list with their types
			var combinedList = new List<(string Text, string Tag, bool IsKeyword)>();
			foreach (var keyword in keywords)
			{
				combinedList.Add((keyword.OriginalKeyword, keyword.OriginalKeyword, true));
				combinedList.Add((keyword.TranslatedKeyword, keyword.OriginalKeyword, true));
			}
			foreach (var woi in wordsofInterest)
			{
				combinedList.Add((woi.OriginalWordofInterest, string.Empty, false));
				combinedList.Add((woi.TranslatedWordofInterest, string.Empty, false));
			}


			// Loop through the text and add hyperlinks for keywords and gray words of interest
			int inputLength = text.Length;
			for (int i = 0; i < inputLength; i++)
			{
				bool matchFound = false;
				foreach (var item in combinedList)
				{
					int substringLength = item.Text.Length;
					if (i + substringLength <= inputLength && text.Substring(i, substringLength) == item.Text)
					{
						// Create a new hyperlink
						if (item.IsKeyword)
						{
							// Hyperlink the keyword and add it to a new run after the previous text
							var hyperlink = new Hyperlink(new Run(item.Text))
							{
								Foreground = System.Windows.Media.Brushes.White,
								TextDecorations = null,
								FontWeight = FontWeights.Bold
							};
							hyperlink.Click += Keyword_Click;
							hyperlink.Tag = item.Tag;
							textBlock.Inlines.Add(hyperlink);
						}
						// Create new run for woi
						if (!item.IsKeyword)
						{
							// Make the word of interest gray and add it to a new run after the previous text
							var run = new Run(item.Text)
							{
								Foreground = System.Windows.Media.Brushes.Gray
							};
							textBlock.Inlines.Add(run);
						}

						// Move the index to the end of the matched substring
						i += substringLength - 1;
						matchFound = true;
						break;
					}
				}

				// No match found, add the current character as a Run
				if (!matchFound)
				{
					// Add run to textblock
					textBlock.Inlines.Add(new Run(text[i].ToString()));
				}
			}
		}

		private async Task<bool> IsDockerDesktopRunningAsync()
		{
			try
			{
				using (var client = new DockerClientConfiguration(new Uri(DockerUri)).CreateClient())
				{
					var containers = await client.Containers.ListContainersAsync(new ContainersListParameters());
					return containers != null;
				}
			}
			catch (Exception ex)
			{
				// Log or handle the exception as needed
				return false;
			}
		}

		private bool CheckAPILimit()
		{
			// Check if we are not using offline translation and have an API limit set before translating and stop translating if we hit the limit
			if (APILimit != "" && APICount != null)
			{
				if (!UseFreeAPI && int.Parse(APICount) >= int.Parse(APILimit))
				{
					if (testMode)
					{
						foreach (var timer in timers)
						{
							timer.Stop();
						}
						timers.Clear();
					}
					else
					{
						StopLogFileWatcher();
					}

					// Stop translating
					IsTranslating = false;
					TranslateButton.Content = "Start Translation";
					BrowseTranslatedFileButton.IsEnabled = true;
					BrowseLogFileButton.IsEnabled = true;
					SourceFolderTextBox.IsEnabled = true;
					SourceLanguageComboBox.IsEnabled = true;
					OutputLanguageComboBox.IsEnabled = true;
					OutputFileNameTextBox.IsEnabled = true;
					ApiKeyTextBox.IsEnabled = true;
					CharacterNameTextBox.IsEnabled = true;
					StatusMessage = windowMessage;
					System.Windows.MessageBox.Show($"You have an API call character limit set of {APILimit} and you have currently sent {FormatNumberWithCommas(APICount.ToString())} characters to the API. Translation cannot continue, please clear your API Count or increase your API limit. Please note google API translation is free for use up to 500,000 characters before charges apply.", "API Limit Warning", MessageBoxButton.OK, MessageBoxImage.Information);
					return true;
				}
			}
			return false;
		}

		private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			e.Handled = !IsTextAllowed(e.Text);
		}

		private static bool IsTextAllowed(string text)
		{
			return !new System.Text.RegularExpressions.Regex("[^0-9]+").IsMatch(text);
		}

		public string FormatNumberWithCommas(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				throw new ArgumentException("Input string cannot be null or empty.", nameof(input));
			}

			if (double.TryParse(input, out double number))
			{
				return number.ToString("N0", CultureInfo.InvariantCulture);
			}
			else
			{
				throw new FormatException("Input string is not a valid number.");
			}
		}

		public string CleanupLogString(string input)
		{
			// Code to convert "\":\"" to ":"
			if (input.Contains("\":\""))
			{
				input = input.Replace("\":\"", ":");
			}
			// Code to convert "}: \"" to "}: "
			if (input.Contains("}: \""))
			{
				input = input.Replace("}: \"", "}: ");
			}
			// Code to convert "}:\"" to "}: "
			if (input.Contains("}:\""))
			{
				input = input.Replace("}:\"", "}: ");
			}
			// Code to convert " to empty if it is at the end of the string
			if (input.EndsWith("''"))
			{
				input = input.Substring(0, input.Length - 1);
			}
			//Code to convert " to empty if it is at the end of the string
			if (input.EndsWith("\""))
			{
				input = input.Substring(0, input.Length - 1);
			}
			// Code to convert  ? to ? if it is at the end of the string
			if (input.EndsWith(" ?"))
			{
				input = input.Substring(0, input.Length - 2) + "?";
			}
			// Code to convert  ! to ! if it is at the end of the string
			if (input.EndsWith(" !"))
			{
				input = input.Substring(0, input.Length - 2) + "!";
			}
			// Code to convert }” to }
			if (input.Contains("}”"))
			{
				input = input.Replace("”", "}");
			}

			// Check for unmatched double quotes and remove them
			input = RemoveUnmatchedQuotes(input);

			// Remove escape characters
		    input = RemoveEscapeCharacters(input);

			// Remove single quotes not followed by a letter
			input = RemoveUnfollowedOrUnprecededSingleQuotes(input);

			// Remove single quotes next to curly braces
			input = RemoveSingleQuotesNextToCurlyBraces(input);

			// Trim whitespace
			input = input.Trim();

			// Return cleaned up string
			return input;
		}

		public string RemoveUnfollowedOrUnprecededSingleQuotes(string input)
		{
			// Regular expression to match a single quote not followed by or preceded by a letter, semicolon, or colon
			string pattern = @"(?<![\p{L};:])'(?![\p{L};:])";
			return Regex.Replace(input, pattern, "");
		}

		private string RemoveSingleQuotesNextToCurlyBraces(string input)
		{
			var cleanInput = input;
			for (int i = 0; i < input.Length; i++)
			{
				if (i+1 !< input.Length && input[i] == '\'' && input[i+1] == '{')
				{
					cleanInput = cleanInput.Remove(i, 1);
					continue;
				}
				if (i - 1 != -1 && input[i-1]  == '}' && input[i] == '\'')
				{
					cleanInput = cleanInput.Remove(i-1, 1);
					continue;
				}
			}
			return cleanInput;
		}

		public List<string> GetWordsInCurlyBraces(string input)
		{
			// Define the regular expression pattern to match text within curly braces.
			string pattern = @"\{([^{}]*)\}";

			// Use Regex to find all matches.
			MatchCollection matches = Regex.Matches(input, pattern);

			// Create a list to store the matched words.
			List<string> wordsInCurlyBraces = new List<string>();

			// Iterate through the matches and add them to the list.
			foreach (Match match in matches)
			{
				wordsInCurlyBraces.Add(match.Groups[1].Value.Trim());
			}

			return wordsInCurlyBraces;
		}

		public List<string> GetWordsInQuotes(string input)
		{
			// Define the regular expression pattern to match text within double quotes.
			string pattern = "\"([^\"\\\\]*(?:\\\\.[^\"\\\\]*)*)\"";

			// Use Regex to find all matches.
			MatchCollection matches = Regex.Matches(input, pattern);

			// Create a list to store the matched words.
			List<string> wordsInQuotes = new List<string>();

			// Iterate through the matches and add them to the list.
			foreach (Match match in matches)
			{
				wordsInQuotes.Add(match.Groups[1].Value.Trim());
			}

			return wordsInQuotes;
		}

		public string StripBeforeDoubleDashAndTrim(string input)
		{
			// Find the position of the first "--"
			int doubleDashIndex = input.IndexOf("--");

			if (doubleDashIndex != -1)
			{
				// Extract the part of the string after the "--"
				string result = input.Substring(doubleDashIndex + 2);

				// Trim whitespace from the start and end
				return result.Trim();
			}

			// If no "--" is found, return the trimmed input
			return input.Trim();
		}

		public string StripBeforeBracketAndTrim(string input)
		{
			// Find the position of the first "]"
			int bracketIndex = input.IndexOf(']');

			if (bracketIndex != -1)
			{
				// Extract the part of the string after the "]"
				string result = input.Substring(bracketIndex + 1);

				// Trim whitespace from the start and end
				return result.Trim();
			}

			// If no "]" is found, return the trimmed input
			return input.Trim();
		}

		public string StripBeforeColonAndTrim(string input)
		{
			int colonCount = 0;
			int colonIndex = -1;

			// Find the position of the third ":"
			for (int i = 0; i < input.Length; i++)
			{
				if (input[i] == ':')
				{
					colonCount++;
					if (colonCount == 3)
					{
						colonIndex = i;
					}
				}
			}

			if (colonIndex != -1)
			{
				// Extract the part of the string after the ":"
				string result = input.Substring(colonIndex + 1);

				// Trim whitespace from the start and end
				return result.Trim();
			}

			// If no ":" is found, return the trimmed input
			return input.Trim();
		}

		public string StripBeforeColonAndTrimQuotes(string input)
		{
			int colonCount = 0;
			int colonIndex = -1;

			// Find the position of the third ":"
			for (int i = 0; i < input.Length; i++)
			{
				if (input[i] == ':')
				{
					colonCount++;
					if (colonCount == 3)
					{
						colonIndex = i;
					}
				}
			}

			if (colonIndex != -1)
			{
				// Extract the part of the string after the ":"
				string result = input.Substring(colonIndex + 1);

				// Trim whitespace from the start and end
				result = result.Trim();

				// Ensure the result is wrapped in quotes
				if (result.StartsWith("\"") && result.EndsWith("\""))
				{
					// Remove the surrounding quotes
					result = result.Substring(1, result.Length - 2).Trim();
				}

				return result;
			}

			// If no ":" is found, return the trimmed input
			return input.Trim();
		}

		public string GetFirstWordBetweenBrackets(string input)
		{
			// Define the regular expression pattern to match text within [].
			string pattern = @"\[(.*?)\]";

			// Use Regex to find the first match.
			Match match = Regex.Match(input, pattern);

			if (match.Success)
			{
				// Get the matched text within the brackets.
				string textWithinBrackets = match.Groups[1].Value;
				return textWithinBrackets.Trim();
			}

			return null;
		}

		public string GetFirstWordBetweenBracketsAndQuotes(string input)
		{
			// Define the regular expression pattern to match text within [""].
			string pattern = @"\[""([^""]*)""\]";

			// Use Regex to find the first match.
			Match match = Regex.Match(input, pattern);

			if (match.Success)
			{
				// Get the matched text within the double quotes.
				string textWithinQuotes = match.Groups[1].Value;
				return textWithinQuotes.Trim();
			}

			return null;
		}

		public string GetFirstWordBetweenQuotesAfterBrackets(string input)
		{
			// Define the regular expression pattern to match text within []
			string bracketPattern = @"\[[^\]]*\]";
			Match bracketMatch = Regex.Match(input, bracketPattern);

			if (bracketMatch.Success)
			{
				// Get the index after the first match of []
				int startIndex = bracketMatch.Index + bracketMatch.Length;

				// Define the regular expression pattern to match text within "" after []
				string quotePattern = @"""([^""]*)""";
				Match quoteMatch = Regex.Match(input.Substring(startIndex), quotePattern);

				if (quoteMatch.Success)
				{
					// Get the matched text within the quotes
					string textWithinQuotes = quoteMatch.Groups[1].Value;
					return textWithinQuotes.Trim();
				}
			}

			return null;
		}

		public string GetFirstWordBetweenCurlyBraces(string input)
		{
			// Define the regular expression pattern to match text within {}
			string pattern = @"\{([^}]*)\}";

			// Use Regex to find the first match
			Match match = Regex.Match(input, pattern);

			if (match.Success)
			{
				// Get the matched text within the curly braces
				string textWithinBraces = match.Groups[1].Value;
				return textWithinBraces.Trim();
			}

			return null;
		}

		private bool TryExtractTimestamp(string line, out DateTime timestamp)
		{
			string[] parts = line.Split(new[] { "--" }, StringSplitOptions.None);
			if (parts.Length > 0)
			{
				string[] formats = { "MM/dd/yyyy HH:mm:ss" };
				string timestampPart = parts[0].Trim();

				foreach (var format in formats)
				{
					if (DateTime.TryParseExact(timestampPart, format, CultureInfo.CurrentCulture, DateTimeStyles.None, out timestamp))
					{
						return true;
					}
				}
			}
			timestamp = DateTime.Now;
			return false;
		}
		private bool IsRichTextBoxEmpty(System.Windows.Controls.RichTextBox richTextBox)
		{
			TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
			return string.IsNullOrWhiteSpace(textRange.Text);
		}

		string GetRichTextBoxText(System.Windows.Controls.RichTextBox richTextBox)
		{
			string returnString;
			TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
			returnString = textRange.Text.Replace("\r\n", string.Empty);
			return returnString.Trim();
		}

		public void SetRichTextBoxText(System.Windows.Controls.RichTextBox richTextBox, string text)
		{
			// Extract the current text and formatting
			TextRange originalTextRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
			string originalText = originalTextRange.Text;
			List<TextFormatting> formattingList = new List<TextFormatting>();
			TextPointer pointer = richTextBox.Document.ContentStart;

			while (pointer != null && pointer.CompareTo(richTextBox.Document.ContentEnd) < 0)
			{
				if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
				{
					TextRange range = new TextRange(pointer, pointer.GetPositionAtOffset(1, LogicalDirection.Forward));
					formattingList.Add(new TextFormatting
					{
						Text = range.Text,
						Foreground = range.GetPropertyValue(TextElement.ForegroundProperty) as System.Drawing.Brush,
						FontWeight = (FontWeight)range.GetPropertyValue(TextElement.FontWeightProperty),
						FontFamily = range.GetPropertyValue(TextElement.FontFamilyProperty) as System.Drawing.FontFamily
					});
				}
				pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
			}

			// Clear the existing content
			richTextBox.Document.Blocks.Clear();

			// Set the new text
			TextRange newTextRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
			newTextRange.Text = text;

			// Reapply the formatting
			foreach (var formatting in formattingList)
			{
				if (pointer == null) break;
				TextRange range = new TextRange(pointer, pointer.GetPositionAtOffset(formatting.Text.Length, LogicalDirection.Forward));
				if (range.IsEmpty) break;
				range.ApplyPropertyValue(TextElement.ForegroundProperty, formatting.Foreground);
				range.ApplyPropertyValue(TextElement.FontWeightProperty, formatting.FontWeight);
				range.ApplyPropertyValue(TextElement.FontFamilyProperty, formatting.FontFamily);
				pointer = pointer.GetPositionAtOffset(formatting.Text.Length, LogicalDirection.Forward);
			}
		}

		public string RemoveMatchedDoubleQuotes(string input)
		{
			// Regular expression to match text wrapped in double quotes
			string pattern = @"""([^""]*)""";

			// Replace matched double quotes with the text between them
			string result = Regex.Replace(input, pattern, "$1");

			return result.Trim();
		}

		public string RemoveUnmatchedQuotes(string input)
		{
			// Regular expression to find quotes
			var quotePattern = new Regex("\"");

			// Find all matches
			var matches = quotePattern.Matches(input);

			// If the number of quotes is even, return the input as is
			if (matches.Count % 2 == 0)
			{
				return input.Trim();
			}

			// If the number of quotes is odd, remove the last unmatched quote
			int lastQuoteIndex = input.LastIndexOf('\"');
			if (lastQuoteIndex != -1)
			{
				input = input.Remove(lastQuoteIndex, 1);
			}

			return input.Trim();
		}

		public string RemoveEscapeCharacters(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return input;
			}

			// Replace backslashes with an empty string.
			input = input.Replace("\\", string.Empty);

			// Define a regular expression pattern to match escape characters.
			string pattern = @"[\n\r\t]";

			// Replace escape characters with an empty string.
			string result = Regex.Replace(input, pattern, string.Empty);

			return result.Trim();
		}

		public string RemoveMatchedCurlyBrackets(string input)
		{
			// Regular expression to match text wrapped in double quotes
			string pattern = @"\{([^}]*)\}";

			// Replace matched double quotes with the text between them
			string result = Regex.Replace(input, pattern, "$1");

			return result.Trim();
		}

		public string ChangeColorBrightness(string passedColor, float correctionFactor)
		{
			var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(passedColor);

			float red = (float)color.R;
			float green = (float)color.G;
			float blue = (float)color.B;

			if (correctionFactor < 0)
			{
				correctionFactor = 1 + correctionFactor;
				red *= correctionFactor;
				green *= correctionFactor;
				blue *= correctionFactor;
			}
			else
			{
				red = (255 - red) * correctionFactor + red;
				green = (255 - green) * correctionFactor + green;
				blue = (255 - blue) * correctionFactor + blue;
			}
			var newColor = System.Windows.Media.Color.FromArgb(color.A, (byte)red, (byte)green, (byte)blue);
			return newColor.ToString();
		}

		private async Task ShowFakeProgressBarAsync(string message, int durationInSeconds)
		{
			var progressBarWindow = new DockerProgressBar
			{
				Owner = this,
				Message = message
			};

			progressBarWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			progressBarWindow.Show();

			for (int i = 0; i <= 100; i++)
			{
				progressBarWindow.SetProgress(i);
				await Task.Delay(durationInSeconds * 10); // Simulate progress
			}

			progressBarWindow.Close();
		}

		#endregion
	}
}