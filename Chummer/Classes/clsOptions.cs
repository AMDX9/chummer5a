/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using Microsoft.Win32;

// MRUChanged Event Handler.
public delegate void MRUChangedHandler();

namespace Chummer
{
	public enum ClipboardContentType
	{
		None = 0,
		Gear = 1,
		Commlink = 2,
		OperatingSystem = 3,
		Cyberware = 4,
		Bioware = 5,
		Armor = 6,
		Weapon = 7,
		Vehicle = 8,
		Lifestyle = 9,
	}

	public class SourcebookInfo
	{
		string _strCode = "";
		string _strPath = "";
		int _intOffset = 0;

		#region Properties
		public string Code
		{
			get
			{
				return _strCode;
			}
			set
			{
				_strCode = value;
			}
		}

		public string Path
		{
			get
			{
				return _strPath;
			}
			set
			{
				_strPath = value;
			}
		}

		public int Offset
		{
			get
			{
				return _intOffset;
			}
			set
			{
				_intOffset = value;
			}
		}
		#endregion
	}

	/// <summary>
	/// Global Options. A single instance class since Options are common for all characters, reduces execution time and memory usage.
	/// </summary>
	public sealed class GlobalOptions
	{
		static readonly GlobalOptions _objInstance = new GlobalOptions();
		static readonly CultureInfo _objCultureInfo = new CultureInfo("en-US");

		public event MRUChangedHandler MRUChanged;

		private frmMain _frmMainForm;

		private static bool _blnAutomaticUpdate = false;
		private static bool _blnLocalisedUpdatesOnly = false;
		private static bool _blnStartupFullscreen = false;
		private static bool _blnSingleDiceRoller = true;
		private static string _strLanguage = "en-us";
		private static string _strDefaultCharacterSheet = "Shadowrun 5";
		private static bool _blnDatesIncludeTime = true;
		private static bool _blnPrintToFileFirst = false;
		private static bool _lifeModuleEnabled;
		private static bool _blnMissionsOnly = false;

		// Omae Information.
		private static string _strOmaeUserName = "";
		private static string _strOmaePassword = "";
		private static bool _blnOmaeAutoLogin = false;

		private XmlDocument _objXmlClipboard = new XmlDocument();
		private ClipboardContentType _objClipboardContentType = new ClipboardContentType();

		public static GradeList CyberwareGrades = new GradeList();
		public static GradeList BiowareGrades = new GradeList();

		// PDF information.
		public static string _strPDFAppPath = "";
        public static string _strURLAppPath = "";
		public static List<SourcebookInfo> _lstSourcebookInfo = new List<SourcebookInfo>();
        public static bool _blnOpenPDFsAsURLs = false;
        public static bool _blnUseLogging = false;

		#region Constructor and Instance
		static GlobalOptions()
		{
			string settingsDirectoryPath = Path.Combine(Environment.CurrentDirectory, "settings");
            if (!Directory.Exists(settingsDirectoryPath))
				Directory.CreateDirectory(settingsDirectoryPath);

			// Automatic Update.
			try
			{
				_blnAutomaticUpdate = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("autoupdate").ToString());
			}
			catch
			{
			}

			try
			{
				_lifeModuleEnabled =
					Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("lifemodule").ToString());
			}
			catch 
			{
			}

            // Whether or not the app should only download localised files in the user's selected language.
			try
			{
				_blnLocalisedUpdatesOnly = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("localisedupdatesonly").ToString());
			}
			catch
			{
			}

            // Whether or not the app should use logging.
            try
            {
                _blnUseLogging = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("uselogging").ToString());
            }
            catch
            {
            }

            // Whether or not dates should include the time.
			try
			{
				_blnDatesIncludeTime = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("datesincludetime").ToString());
			}
			catch
			{
			}

			try
			{
				_blnMissionsOnly = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("missionsonly").ToString());
			}
			catch { }

			// Whether or not printouts should be sent to a file before loading them in the browser. This is a fix for getting printing to work properly on Linux using Wine.
			try
			{
				_blnPrintToFileFirst = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("printtofilefirst").ToString());
			}
			catch
			{
			}

			// Default character sheet.
			try
			{
				_strDefaultCharacterSheet = Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("defaultsheet").ToString();
			}
			catch
			{
			}

			// Omae Settings.
			// Username.
			try
			{
				_strOmaeUserName = Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("omaeusername").ToString();
			}
			catch
			{
			}
			// Password.
			try
			{
				_strOmaePassword = Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("omaepassword").ToString();
			}
			catch
			{
			}
			// AutoLogin.
			try
			{
				_blnOmaeAutoLogin = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("omaeautologin").ToString());
			}
			catch
			{
			}
			// Language.
			try
			{
				_strLanguage = Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("language").ToString();
				if (_strLanguage == "en-us2")
				{
					_strLanguage = "en-us";
				}
			}
			catch
			{
			}
			// Startup in Fullscreen mode.
			try
			{
				_blnStartupFullscreen = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("startupfullscreen").ToString());
			}
			catch
			{
			}
			// Single instace of the Dice Roller window.
			try
			{
				_blnSingleDiceRoller = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("singlediceroller").ToString());
			}
			catch
			{
			}

            // Open PDFs as URLs. For use with Chrome, Firefox, etc.
            try
            {
                _blnOpenPDFsAsURLs = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("openpdfsasurls").ToString());
            }
            catch
            {
            }

			// PDF application path.
			try
			{
				_strPDFAppPath = Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("pdfapppath").ToString();
			}
			catch
			{
			}

			// Retrieve the SourcebookInfo objects.
			XmlDocument objXmlDocument = XmlManager.Instance.Load("books.xml");
			XmlNodeList objXmlBookList = objXmlDocument.SelectNodes("/chummer/books/book");
			foreach (XmlNode objXmlBook in objXmlBookList)
			{
				try
				{
					SourcebookInfo objSource = new SourcebookInfo();
					string strTemp = Registry.CurrentUser.CreateSubKey("Software\\Chummer5\\Sourcebook").GetValue(objXmlBook["code"].InnerText).ToString();
					string[] strParts = strTemp.Split('|');
					objSource.Code = objXmlBook["code"].InnerText;
					objSource.Path = strParts[0];
					objSource.Offset = Convert.ToInt32(strParts[1]);

					_lstSourcebookInfo.Add(objSource);
				}
				catch
				{
				}
			}

			CyberwareGrades.LoadList(Improvement.ImprovementSource.Cyberware);
			BiowareGrades.LoadList(Improvement.ImprovementSource.Bioware);
		}

		

		/// <summary>
		/// Global instance of the GlobalOptions.
		/// </summary>
		public static GlobalOptions Instance
		{
			get
			{
				return _objInstance;
			}
		}
		#endregion

		#region Properties
		/// <summary>
		/// Whether or not Automatic Updates are enabled.
		/// </summary>
		public bool AutomaticUpdate
		{
			get
			{
				return _blnAutomaticUpdate;
			}
			set
			{
				_blnAutomaticUpdate = value;
			}
		}

		public bool LifeModuleEnabled
		{
			get { return _lifeModuleEnabled; }
			set { _lifeModuleEnabled = value; }
		}

        /// <summary>
		/// Whether or not the app should only download localised files in the user's selected language.
		/// </summary>
		public bool LocalisedUpdatesOnly
		{
			get
			{
				return _blnLocalisedUpdatesOnly;
			}
			set
			{
				_blnLocalisedUpdatesOnly = value;
			}
		}

        /// <summary>
        /// Whether or not the app should use logging.
        /// </summary>
        public bool UseLogging
        {
            get
            {
                return _blnUseLogging;
            }
            set
            {
                _blnUseLogging = value;
            }
        }

        /// <summary>
		/// Whether or not dates should include the time.
		/// </summary>
		public bool DatesIncludeTime
		{
			get
			{
				return _blnDatesIncludeTime;
			}
			set
			{
				_blnDatesIncludeTime = value;
			}
		}

		public bool MissionsOnly {
			get
			{
				return _blnMissionsOnly;
				
			}
			set
			{
				_blnMissionsOnly = value;
			}
		}
		

		/// <summary>
		/// Whether or not printouts should be sent to a file before loading them in the browser. This is a fix for getting printing to work properly on Linux using Wine.
		/// </summary>
		public bool PrintToFileFirst
		{
			get
			{
				return _blnPrintToFileFirst;
			}
			set
			{
				_blnPrintToFileFirst = value;
			}
		}

		/// <summary>
		/// Omae user name.
		/// </summary>
		public string OmaeUserName
		{
			get
			{
				return _strOmaeUserName;
			}
			set
			{
				_strOmaeUserName = value;
			}
		}

		/// <summary>
		/// Omae password (Base64 encoded).
		/// </summary>
		public string OmaePassword
		{
			get
			{
				return _strOmaePassword;
			}
			set
			{
				_strOmaePassword = value;
			}
		}

		/// <summary>
		/// Omae AutoLogin.
		/// </summary>
		public bool OmaeAutoLogin
		{
			get
			{
				return _blnOmaeAutoLogin;
			}
			set
			{
				_blnOmaeAutoLogin = value;
			}
		}

		/// <summary>
		/// Main application form.
		/// </summary>
		public frmMain MainForm
		{
			get
			{
				return _frmMainForm;
			}
			set
			{
				_frmMainForm = value;
			}
		}

		/// <summary>
		/// Language.
		/// </summary>
		public string Language
		{
			get
			{
				return _strLanguage;
			}
			set
			{
				_strLanguage = value;
			}
		}

		/// <summary>
		/// Whether or not the application should start in fullscreen mode.
		/// </summary>
		public bool StartupFullscreen
		{
			get
			{
				return _blnStartupFullscreen;
			}
			set
			{
				_blnStartupFullscreen = value;
			}
		}

		/// <summary>
		/// Whether or not only a single instance of the Dice Roller should be allowed.
		/// </summary>
		public bool SingleDiceRoller
		{
			get
			{
				return _blnSingleDiceRoller;
			}
			set
			{
				_blnSingleDiceRoller = value;
			}
		}

		/// <summary>
		/// CultureInfor for number localization.
		/// </summary>
		public CultureInfo CultureInfo
		{
			get
			{
				return _objCultureInfo;
			}
		}

		/// <summary>
		/// Clipboard.
		/// </summary>
		public XmlDocument Clipboard
		{
			get
			{
				return _objXmlClipboard;
			}
			set
			{
				_objXmlClipboard = value;
			}
		}

		/// <summary>
		/// Type of data that is currently stored in the clipboard.
		/// </summary>
		public ClipboardContentType ClipboardContentType
		{
			get
			{
				return _objClipboardContentType;
			}
			set
			{
				_objClipboardContentType = value;
			}
		}

		/// <summary>
		/// Default character sheet to use when printing.
		/// </summary>
		public string DefaultCharacterSheet
		{
			get
			{
				return _strDefaultCharacterSheet;
			}
			set
			{
				_strDefaultCharacterSheet = value;
			}
		}

		/// <summary>
		/// Path to the user's PDF application.
		/// </summary>
		public string PDFAppPath
		{
			get
			{
				return _strPDFAppPath;
			}
			set
			{
				_strPDFAppPath = value;
			}
		}

        /// <summary>
        /// Path to the user's PDF application.
        /// </summary>
        public string URLAppPath
        {
            get
            {
                return _strURLAppPath;
            }
            set
            {
                _strURLAppPath = value;
            }
        }
		/// <summary>
		/// List of SourcebookInfo.
		/// </summary>
		public List<SourcebookInfo> SourcebookInfo
		{
			get
			{
				return _lstSourcebookInfo;
			}
			set
			{
				_lstSourcebookInfo = value;
			}
		}
		#endregion

		#region MRU Methods
		/// <summary>
		/// Add a file to the most recently used characters.
		/// </summary>
		/// <param name="strFile">Name of the file to add.</param>
		public void AddToMRUList(string strFile)
		{
			List<string> strFiles = ReadMRUList();

			// Make sure the file does not already exist in the MRU list.
			if (strFiles.Contains(strFile))
				strFiles.Remove(strFile);

			// Make sure the file doesn't exist in the sticky MRU list.
			List<string> strStickyFiles = ReadStickyMRUList();
			if (strStickyFiles.Contains(strFile))
				return;

			strFiles.Insert(0, strFile);

			if (strFiles.Count > 10)
				strFiles.RemoveRange(10, strFiles.Count - 10);

			RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
			int i = 0;
			foreach (string strItem in strFiles)
			{
				i++;
				objRegistry.SetValue("mru" + i.ToString(), strItem);
			}
			MRUChanged();
		}

		/// <summary>
		/// Remove a file from the most recently used characters.
		/// </summary>
		/// <param name="strFile">Name of the file to remove.</param>
		public void RemoveFromMRUList(string strFile)
		{
			List<string> strFiles = ReadMRUList();

			foreach (string strItem in strFiles)
			{
				if (strItem == strFile)
				{
					strFiles.Remove(strItem);
					break;
				}
			}

			RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
			int i = 0;
			foreach (string strItem in strFiles)
			{
				i++;
				objRegistry.SetValue("mru" + i.ToString(), strItem);
			}
			if (strFiles.Count < 10)
			{
				for (i = strFiles.Count + 1; i <= 10; i++)
				{
					try
					{
						objRegistry.DeleteValue("mru" + i.ToString());
					}
					catch
					{
					}
				}
			}
			MRUChanged();
		}

		/// <summary>
		/// Retrieve the list of most recently used characters.
		/// </summary>
		public List<string> ReadMRUList()
		{
			RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
			List<string> lstFiles = new List<string>();

			for (int i = 1; i <= 10; i++)
			{
				if ((objRegistry.GetValue("mru" + i.ToString())) != null)
				{
					lstFiles.Add(objRegistry.GetValue("mru" + i.ToString()).ToString());
				}
			}

			return lstFiles;
		}

		/// <summary>
		/// Add a file to the sticky most recently used characters.
		/// </summary>
		/// <param name="strFile">Name of the file to add.</param>
		public void AddToStickyMRUList(string strFile)
		{
			List<string> strFiles = ReadStickyMRUList();

			// Make sure the file does not already exist in the MRU list.
			if (strFiles.Contains(strFile))
				strFiles.Remove(strFile);

			strFiles.Insert(0, strFile);

			if (strFiles.Count > 10)
				strFiles.RemoveRange(10, strFiles.Count - 10);

			RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
			int i = 0;
			foreach (string strItem in strFiles)
			{
				i++;
				objRegistry.SetValue("stickymru" + i.ToString(), strItem);
			}
			MRUChanged();
		}

		/// <summary>
		/// Remove a file from the sticky most recently used characters.
		/// </summary>
		/// <param name="strFile">Name of the file to remove.</param>
		public void RemoveFromStickyMRUList(string strFile)
		{
			List<string> strFiles = ReadStickyMRUList();

			foreach (string strItem in strFiles)
			{
				if (strItem == strFile)
				{
					strFiles.Remove(strItem);
					break;
				}
			}

			RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
			int i = 0;
			foreach (string strItem in strFiles)
			{
				i++;
				objRegistry.SetValue("stickymru" + i.ToString(), strItem);
			}
			if (strFiles.Count < 10)
			{
				for (i = strFiles.Count + 1; i <= 10; i++)
				{
					try
					{
						objRegistry.DeleteValue("stickymru" + i.ToString());
					}
					catch
					{
					}
				}
			}
			MRUChanged();
		}

		/// <summary>
		/// Retrieve the list of sticky most recently used characters.
		/// </summary>
		public List<string> ReadStickyMRUList()
		{
			RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
			List<string> lstFiles = new List<string>();

			for (int i = 1; i <= 10; i++)
			{
				if ((objRegistry.GetValue("stickymru" + i.ToString())) != null)
				{
					lstFiles.Add(objRegistry.GetValue("stickymru" + i.ToString()).ToString());
				}
			}

			return lstFiles;
		}
		#endregion
        /// <summary>
        /// Which method of opening PDFs to use. True = file://path.pdf#page=x
        /// </summary>
        public bool OpenPDFsAsURLs 
        { 
            get
            {
                return GlobalOptions._blnOpenPDFsAsURLs;
            }
            set
            {
                GlobalOptions._blnOpenPDFsAsURLs = value;
            } 
        }

    }

	public class CharacterOptions
	{
		private string _strFileName = "default.xml";
		private string _strName = "Default Settings";

		// Settings.
        private bool _blnAllow2ndMaxAttribute = false;
        private bool _blnAllowAttributePointsOnExceptional = false;
        private bool _blnAllowBiowareSuites = false;
        private bool _blnAllowCustomTransgenics = false;
        private bool _blnAllowCyberwareESSDiscounts = false;
        private bool _blnAllowEditPartOfBaseWeapon = false;
        private bool _blnAllowExceedAttributeBP = false;
        private bool _blnAllowHigherStackedFoci = false;
        private bool _blnAllowInitiationInCreateMode = false;
        private bool _blnAllowObsolescentUpgrade = false;
        private bool _blnAllowSkillDiceRolling = false;
        private bool _blnAllowSkillRegrouping = true;
        private bool _blnAlternateArmorEncumbrance = false;
        private bool _blnAlternateComplexFormCost = false;
        private bool _blnAlternateMatrixAttribute = false;
        private bool _blnAlternateMetatypeAttributeKarma = false;
        private bool _blnArmorDegradation = false;
        private bool _blnArmorSuitCapacity = false;
        private bool _blnAutomaticCopyProtection = true;
        private bool _blnAutomaticRegistration = true;
        private bool _blnBreakSkillGroupsInCreateMode = false;
        private bool _blnCalculateCommlinkResponse = true;
        private bool _blnCapSkillRating = false;
        private bool _blnConfirmDelete = true;
        private bool _blnConfirmKarmaExpense = true;
        private bool _blnCreateBackupOnCareer = false;
        private bool _blnCyberlegMovement = false;
        private bool _blnDontDoubleQualityPurchaseCost = false;
		private bool _blnDontDoubleQualityRefundCost = false;
		private bool _blnEnforceCapacity = true;
        private bool _blnEnforceSkillMaximumModifiedRating = false;
        private bool _blnErgonomicProgramsLimit = true;
        private bool _blnESSLossReducesMaximumOnly = false;
        private bool _blnExceedNegativeQualities = false;
        private bool _blnExceedNegativeQualitiesLimit = false;
        private bool _blnExceedPositiveQualities = false;
        private bool _blnExtendAnyDetectionSpell = false;
        private bool _blnFreeContactsMultiplierEnabled = false;
        private bool _blnFreeKarmaContacts = false;
        private bool _blnFreeKarmaKnowledge = false;
        private bool _blnFreeKnowledgeMultiplierEnabled = false;
        private bool _blnFreeSpiritPowerPointsMAG = false;
        private bool _blnIgnoreArmorEncumbrance = true;
        private bool _blnIgnoreArt = false;
        private bool _blnKnucksUseUnarmed = false;
        private bool _blnLicenseRestrictedItems = false;
        private bool _blnMaximumArmorModifications = false;
        private bool _blnMayBuyQualities = false;
        private bool _blnMetatypeCostsKarma = true;
        private bool _blnMoreLethalGameplay = false;
        private bool _blnMultiplyForbiddenCost = false;
        private bool _blnMultiplyRestrictedCost = false;
        private bool _blnNoSingleArmorEncumbrance = false;
        private bool _blnPrintArcanaAlternates = false;
        private bool _blnPrintExpenses = false;
        private bool _blnPrintLeadershipAlternates = false;
        private bool _blnPrintNotes = false;
        private bool _blnPrintSkillsWithZeroRating = true;
        private bool _blnRestrictRecoil = true;
        private bool _blnSkillDefaultingIncludesModifiers = false;
        private bool _blnSpecialAttributeKarmaLimit = false;
        private bool _blnSpecialKarmaCostBasedOnShownValue = false;
        private bool _blnSpiritForceBasedOnTotalMAG = false;
        private bool _blnStrengthAffectsRecoil = false;
        private bool _blnTechnomancerAllowAutosoft = false;
        private bool _blnUnrestrictedNuyen = false;
        private bool _blnUseCalculatedVehicleSensorRatings = false;
        private bool _blnUseContactPoints = false;
        private bool _blnUsePointsOnBrokenGroups = false;
        private int _intEssenceDecimals = 2;
        private int _intForbiddenCostMultiplier = 1;
        private int _intFreeContactsFlatNumber = 0;
        private int _intFreeContactsMultiplier = 3;
        private int _intFreeKnowledgeMultiplier = 2;
        private int _intLimbCount = 6;
        private int _intMetatypeCostMultiplier = 1;
        private int _intNuyenPerBP = 2000;
        private int _intRestrictedCostMultiplier = 1;
		private bool _automaticBackstory = true;

		private readonly XmlDocument _objBookDoc = new XmlDocument();
        private string _strBookXPath = "";
        private string _strExcludeLimbSlot = "";

        // BP variables.
        private int _intBPActiveSkill = 4;
        private int _intBPActiveSkillSpecialization = 2;
        private int _intBPAttributeMax = 15;
        private int _intBPComplexForm = 1;
        private int _intBPComplexFormOption = 1;
        private int _intBPContact = 1;
        private int _intBPFocus = 1;
        private int _intBPKnowledgeSkill = 2;
        private int _intBPMartialArt = 5;
        private int _intBPMartialArtManeuver = 2;
        private int _intBPSkillGroup = 10;
        private int _intBPSpell = 3;
        private int _intBPSpirit = 1;
        private int _intBPAttribute = 10;

        // Karma variables.
        private int _intKarmaAttribute = 5;
        private int _intKarmaCarryover = 7;
        private int _intKarmaComplexFormOption = 2;
        private int _intKarmaComplexFormSkillfot = 1;
        private int _intKarmaContact = 1;
        private int _intKarmaEnemy = 1;
        private int _intKarmaEnhancement = 2;
        private int _intKarmaImproveActiveSkill = 2;
        private int _intKarmaImproveComplexForm = 1;
        private int _intKarmaImproveKnowledgeSkill = 1;
        private int _intKarmaImproveSkillGroup = 5;
        private int _intKarmaInitiation = 3;
        private int _intKarmaJoinGroup = 5;
        private int _intKarmaLeaveGroup = 1;
        private int _intKarmaManeuver = 5;
        private int _intKarmaMetamagic = 15;
        private int _intKarmaNewActiveSkill = 2;
        private int _intKarmaNewComplexForm = 4;
        private int _intKarmaNewKnowledgeSkill = 1;
        private int _intKarmaNewSkillGroup = 5;
        private int _intKarmaNuyenPer = 2000;
        private int _intKarmaQuality = 1;
        private int _intKarmaSpecialization = 7;
        private int _intKarmaSpell = 5;
        private int _intKarmaSpirit = 2;

        // Karma Foci variables.
        private int _intKarmaAlchemicalFocus = 3;
		private int _intKarmaBanishingFocus = 2;
		private int _intKarmaBindingFocus = 2;
		private int _intKarmaCenteringFocus = 3;
		private int _intKarmaCounterspellingFocus = 2;
		private int _intKarmaDisenchantingFocus = 3;
		private int _intKarmaFlexibleSignatureFocus = 3;
		private int _intKarmaMaskingFocus = 3;
		private int _intKarmaPowerFocus = 6;
		private int _intKarmaQiFocus = 2;
        private int _intKarmaRitualSpellcastingFocus = 2;
        private int _intKarmaSpellcastingFocus = 2;
        private int _intKarmaSpellShapingFocus = 3;
        private int _intKarmaSummoningFocus = 2;
		private int _intKarmaSustainingFocus = 2;
		private int _intKarmaWeaponFocus = 3;

		// Default build settings.
		private string _strBuildMethod = "Karma";
		private int _intBuildPoints = 800;
		private int _intAvailability = 12;

		// Sourcebook list.
		private readonly List<string> _lstBooks = new List<string>();

		#region Initialization, Save, and Load Methods
		public CharacterOptions()
		{
			// Create the settings directory if it does not exist.
			string settingsDirectoryPath = Path.Combine(Environment.CurrentDirectory, "settings");
			if (!Directory.Exists(settingsDirectoryPath))
				Directory.CreateDirectory(settingsDirectoryPath);

			// If the default.xml settings file does not exist, attempt to read the settings from the Registry (old storage format), then save them to the default.xml file.
			string strFilePath = Path.Combine(settingsDirectoryPath, "default.xml");
			if (!File.Exists(strFilePath))
			{
				_strFileName = "default.xml";
				LoadFromRegistry();
				Save();
			}
			else
				Load("default.xml");
			// Load the language file.
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);

			// Load the book information.
			_objBookDoc = XmlManager.Instance.Load("books.xml");
		}

		/// <summary>
		/// Save the current settings to the settings file.
		/// </summary>
		public void Save()
		{
			string strFilePath = Path.Combine(Environment.CurrentDirectory, "settings", _strFileName);
			FileStream objStream = new FileStream(strFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.Unicode);
			objWriter.Formatting = Formatting.Indented;
			objWriter.Indentation = 1;
			objWriter.IndentChar = '\t';
			objWriter.WriteStartDocument();

			// <settings>
			objWriter.WriteStartElement("settings");

			// <name />
			objWriter.WriteElementString("name", _strName);
			// <confirmdelete />
			objWriter.WriteElementString("confirmdelete", _blnConfirmDelete.ToString());
            // <licenserestricted />
            objWriter.WriteElementString("licenserestricted", _blnLicenseRestrictedItems.ToString());
            // <confirmkarmaexpense />
			objWriter.WriteElementString("confirmkarmaexpense", _blnConfirmKarmaExpense.ToString());
			// <printzeroratingskills />
			objWriter.WriteElementString("printzeroratingskills", _blnPrintSkillsWithZeroRating.ToString());
			// <morelethalgameplay />
			objWriter.WriteElementString("morelethalgameplay", _blnMoreLethalGameplay.ToString());
			// <spiritforcebasedontotalmag />
			objWriter.WriteElementString("spiritforcebasedontotalmag", _blnSpiritForceBasedOnTotalMAG.ToString());
			// <skilldefaultingincludesmodifiers />
			objWriter.WriteElementString("skilldefaultingincludesmodifiers", _blnSkillDefaultingIncludesModifiers.ToString());
			// <enforceskillmaximummodifiedrating />
			objWriter.WriteElementString("enforceskillmaximummodifiedrating", _blnEnforceSkillMaximumModifiedRating.ToString());
			// <capskillrating />
			objWriter.WriteElementString("capskillrating", _blnCapSkillRating.ToString());
			// <printexpenses />
			objWriter.WriteElementString("printexpenses", _blnPrintExpenses.ToString());
			// <nuyenperbp />
			objWriter.WriteElementString("nuyenperbp", _intNuyenPerBP.ToString());
			// <knucksuseunarmed />
            objWriter.WriteElementString("knucksuseunarmed", _blnKnucksUseUnarmed.ToString());
            // <allowinitiationincreatemode />
            objWriter.WriteElementString("allowinitiationincreatemode", _blnAllowInitiationInCreateMode.ToString());
            // <usepointsonbrokengroups />
            objWriter.WriteElementString("usepointsonbrokengroups", _blnUsePointsOnBrokenGroups.ToString());
            // <dontdoublequalities />
            objWriter.WriteElementString("dontdoublequalities", _blnDontDoubleQualityPurchaseCost.ToString());
			// <dontdoublequalities />
			objWriter.WriteElementString("dontdoublequalityrefunds", _blnDontDoubleQualityRefundCost.ToString());
			// <ignoreart />
			objWriter.WriteElementString("ignoreart", _blnIgnoreArt.ToString());
            // <cyberlegmovement />
            objWriter.WriteElementString("cyberlegmovement", _blnCyberlegMovement.ToString());
            // <allow2ndmaxattribute />
            objWriter.WriteElementString("allow2ndmaxattribute", _blnAllow2ndMaxAttribute.ToString());
            // <allowattributepointsonexceptional />
            objWriter.WriteElementString("allowattributepointsonexceptional", _blnAllowAttributePointsOnExceptional.ToString());
            // <freekarmacontactsmultiplier />
            objWriter.WriteElementString("freekarmacontactsmultiplier", _intFreeContactsMultiplier.ToString());
            // <freekarmaknowledgemultiplier />
            objWriter.WriteElementString("freekarmaknowledgemultiplier", _intFreeKnowledgeMultiplier.ToString());
            // <freekarmaknowledgemultiplierenabled />
            objWriter.WriteElementString("freekarmaknowledgemultiplierenabled", _blnFreeKnowledgeMultiplierEnabled.ToString());
            // <freecontactsmultiplierenabled />
            objWriter.WriteElementString("freecontactsmultiplierenabled", _blnFreeContactsMultiplierEnabled.ToString());
			// <freecontactsflatnumber />
			objWriter.WriteElementString("freecontactsflatnumber", _intFreeContactsFlatNumber.ToString());
            // <freekarmaknowledge />
            objWriter.WriteElementString("freekarmacontacts", _blnFreeKarmaContacts.ToString());
            // <freekarmaknowledge />
            objWriter.WriteElementString("freekarmaknowledge", _blnFreeKarmaKnowledge.ToString());
			// <nosinglearmorencumbrance />
			objWriter.WriteElementString("nosinglearmorencumbrance", _blnNoSingleArmorEncumbrance.ToString());
			// <ignorearmorencumbrance />
			objWriter.WriteElementString("ignorearmorencumbrance", _blnIgnoreArmorEncumbrance.ToString());
			// <alternatearmorencumbrance />
			objWriter.WriteElementString("alternatearmorencumbrance", _blnAlternateArmorEncumbrance.ToString());
			// <esslossreducesmaximumonly />
			objWriter.WriteElementString("esslossreducesmaximumonly", _blnESSLossReducesMaximumOnly.ToString());
			// <allowskillregrouping />
			objWriter.WriteElementString("allowskillregrouping", _blnAllowSkillRegrouping.ToString());
			// <metatypecostskarma />
			objWriter.WriteElementString("metatypecostskarma", _blnMetatypeCostsKarma.ToString());
			// <metatypecostskarmamultiplier />
			objWriter.WriteElementString("metatypecostskarmamultiplier", _intMetatypeCostMultiplier.ToString());
			// <limbcount />
			objWriter.WriteElementString("limbcount", _intLimbCount.ToString());
			// <excludelimbslot />
			objWriter.WriteElementString("excludelimbslot", _strExcludeLimbSlot);
			// <allowcyberwareessdiscounts />
			objWriter.WriteElementString("allowcyberwareessdiscounts", _blnAllowCyberwareESSDiscounts.ToString());
			// <strengthaffectsrecoil />
			objWriter.WriteElementString("strengthaffectsrecoil", _blnStrengthAffectsRecoil.ToString());
			// <maximumarmormodifications />
			objWriter.WriteElementString("maximumarmormodifications", _blnMaximumArmorModifications.ToString());
			// <armorsuitcapacity />
			objWriter.WriteElementString("armorsuitcapacity", _blnArmorSuitCapacity.ToString());
			// <armordegredation />
			objWriter.WriteElementString("armordegredation", _blnArmorDegradation.ToString());
			// <automaticcopyprotection />
			objWriter.WriteElementString("automaticcopyprotection", _blnAutomaticCopyProtection.ToString());
			// <automaticregistration />
			objWriter.WriteElementString("automaticregistration", _blnAutomaticRegistration.ToString());
			// <ergonomicprogramlimit />
			objWriter.WriteElementString("ergonomicprogramlimit", _blnErgonomicProgramsLimit.ToString());
			// <specialkarmacostbasedonshownvalue />
			objWriter.WriteElementString("specialkarmacostbasedonshownvalue", _blnSpecialKarmaCostBasedOnShownValue.ToString());
			// <exceedpositivequalities />
			objWriter.WriteElementString("exceedpositivequalities", _blnExceedPositiveQualities.ToString());
			// <exceednegativequalities />
			objWriter.WriteElementString("exceednegativequalities", _blnExceedNegativeQualities.ToString());
			// <exceednegativequalitieslimit />
			objWriter.WriteElementString("exceednegativequalitieslimit", _blnExceedNegativeQualitiesLimit.ToString());
			// <usecalculatedvehiclesensorratings />
			objWriter.WriteElementString("usecalculatedvehiclesensorratings", _blnUseCalculatedVehicleSensorRatings.ToString());
			// <multiplyrestrictedcost />
			objWriter.WriteElementString("multiplyrestrictedcost", _blnMultiplyRestrictedCost.ToString());
			// <multiplyforbiddencost />
			objWriter.WriteElementString("multiplyforbiddencost", _blnMultiplyForbiddenCost.ToString());
			// <restrictedcostmultiplier />
			objWriter.WriteElementString("restrictedcostmultiplier", _intRestrictedCostMultiplier.ToString());
			// <forbiddencostmultiplier />
			objWriter.WriteElementString("forbiddencostmultiplier", _intForbiddenCostMultiplier.ToString());
			// <essencedecimals />
			objWriter.WriteElementString("essencedecimals", _intEssenceDecimals.ToString());
			// <enforcecapacity />
			objWriter.WriteElementString("enforcecapacity", _blnEnforceCapacity.ToString());
			// <restrictrecoil />
			objWriter.WriteElementString("restrictrecoil", _blnRestrictRecoil.ToString());
			// <allowexceedattributebp />
			objWriter.WriteElementString("allowexceedattributebp", _blnAllowExceedAttributeBP.ToString());
			// <unrestrictednuyen />
			objWriter.WriteElementString("unrestrictednuyen", _blnUnrestrictedNuyen.ToString());
			// <calculatecommlinkresponse />
			objWriter.WriteElementString("calculatecommlinkresponse", _blnCalculateCommlinkResponse.ToString());
			// <allowhigherstackedfoci />
			objWriter.WriteElementString("allowhigherstackedfoci", _blnAllowHigherStackedFoci.ToString());
			// <alternatecomplexformcost />
			objWriter.WriteElementString("alternatecomplexformcost", _blnAlternateComplexFormCost.ToString());
			// <alternatematrixattribute />
			objWriter.WriteElementString("alternatematrixattribute", _blnAlternateMatrixAttribute.ToString());
			// <alloweditpartofbaseweapon />
			objWriter.WriteElementString("alloweditpartofbaseweapon", _blnAllowEditPartOfBaseWeapon.ToString());
			// <allowcustomtransgenics />
			objWriter.WriteElementString("allowcustomtransgenics", _blnAllowCustomTransgenics.ToString());
            // <maybuyqualities />
            objWriter.WriteElementString("maybuyqualities", _blnMayBuyQualities.ToString());
            // <usecontactpoints />
            objWriter.WriteElementString("usecontactpoints", _blnUseContactPoints.ToString());
            // <breakskillgroupsincreatemode />
			objWriter.WriteElementString("breakskillgroupsincreatemode", _blnBreakSkillGroupsInCreateMode.ToString());
			// <extendanydetectionspell />
			objWriter.WriteElementString("extendanydetectionspell", _blnExtendAnyDetectionSpell.ToString());
			// <allowskilldicerolling />
			objWriter.WriteElementString("allowskilldicerolling", _blnAllowSkillDiceRolling.ToString());
			// <alternatemetatypeattributekarma />
			objWriter.WriteElementString("alternatemetatypeattributekarma", _blnAlternateMetatypeAttributeKarma.ToString());
			// <createbackuponcareer />
			objWriter.WriteElementString("createbackuponcareer", _blnCreateBackupOnCareer.ToString());
			// <printleadershipalternates />
			objWriter.WriteElementString("printleadershipalternates", _blnPrintLeadershipAlternates.ToString());
			// <printarcanaalternates />
			objWriter.WriteElementString("printarcanaalternates", _blnPrintArcanaAlternates.ToString());
			// <printnotes />
			objWriter.WriteElementString("printnotes", _blnPrintNotes.ToString());
			// <allowobsolescentupgrade />
			objWriter.WriteElementString("allowobsolescentupgrade", _blnAllowObsolescentUpgrade.ToString());
			// <allowbiowaresuites />
			objWriter.WriteElementString("allowbiowaresuites", _blnAllowBiowareSuites.ToString());
			// <freespiritpowerpointsmag />
			objWriter.WriteElementString("freespiritpowerpointsmag", _blnFreeSpiritPowerPointsMAG.ToString());
			// <specialattributekarmalimit />
			objWriter.WriteElementString("specialattributekarmalimit", _blnSpecialAttributeKarmaLimit.ToString());
			// <technomancerallowautosoft />
			objWriter.WriteElementString("technomancerallowautosoft", _blnTechnomancerAllowAutosoft.ToString());
			objWriter.WriteElementString("autobackstory", _automaticBackstory.ToString());

			// <bpcost>
			objWriter.WriteStartElement("bpcost");
			// <bpattribute />
			objWriter.WriteElementString("bpattribute", _intBPAttribute.ToString());
			// <bpattributemax />
			objWriter.WriteElementString("bpattributemax", _intBPAttributeMax.ToString());
			// <bpcontact />
			objWriter.WriteElementString("bpcontact", _intBPContact.ToString());
			// <bpmartialart />
			objWriter.WriteElementString("bpmartialart", _intBPMartialArt.ToString());
			// <bpmartialartmaneuver />
			objWriter.WriteElementString("bpmartialartmaneuver", _intBPMartialArtManeuver.ToString());
			// <bpskillgroup />
			objWriter.WriteElementString("bpskillgroup", _intBPSkillGroup.ToString());
			// <bpactiveskill />
			objWriter.WriteElementString("bpactiveskill", _intBPActiveSkill.ToString());
			// <bpactiveskillspecialization />
			objWriter.WriteElementString("bpactiveskillspecialization", _intBPActiveSkillSpecialization.ToString());
			// <bpknowledgeskill />
			objWriter.WriteElementString("bpknowledgeskill", _intBPKnowledgeSkill.ToString());
			// <bpspell />
			objWriter.WriteElementString("bpspell", _intBPSpell.ToString());
			// <bpfocus />
			objWriter.WriteElementString("bpfocus", _intBPFocus.ToString());
			// <bpspirit />
			objWriter.WriteElementString("bpspirit", _intBPSpirit.ToString());
			// <bpcomplexform />
			objWriter.WriteElementString("bpcomplexform", _intBPComplexForm.ToString());
			// <bpcomplexformoption />
			objWriter.WriteElementString("bpcomplexformoption", _intBPComplexFormOption.ToString());
			// </bpcost>
			objWriter.WriteEndElement();

			// <karmacost>
			objWriter.WriteStartElement("karmacost");
			// <karmaattribute />
			objWriter.WriteElementString("karmaattribute", _intKarmaAttribute.ToString());
			// <karmaquality />
			objWriter.WriteElementString("karmaquality", _intKarmaQuality.ToString());
			// <karmaspecialization />
			objWriter.WriteElementString("karmaspecialization", _intKarmaSpecialization.ToString());
			// <karmanewknowledgeskill />
			objWriter.WriteElementString("karmanewknowledgeskill", _intKarmaNewKnowledgeSkill.ToString());
			// <karmanewactiveskill />
			objWriter.WriteElementString("karmanewactiveskill", _intKarmaNewActiveSkill.ToString());
			// <karmanewskillgroup />
			objWriter.WriteElementString("karmanewskillgroup", _intKarmaNewSkillGroup.ToString());
			// <karmaimproveknowledgeskill />
			objWriter.WriteElementString("karmaimproveknowledgeskill", _intKarmaImproveKnowledgeSkill.ToString());
			// <karmaimproveactiveskill />
			objWriter.WriteElementString("karmaimproveactiveskill", _intKarmaImproveActiveSkill.ToString());
			// <karmaimproveskillgroup />
			objWriter.WriteElementString("karmaimproveskillgroup", _intKarmaImproveSkillGroup.ToString());
			// <karmaspell />
			objWriter.WriteElementString("karmaspell", _intKarmaSpell.ToString());
            // <karmaenhancement />
            objWriter.WriteElementString("karmaenhancement", _intKarmaEnhancement.ToString());
            // <karmanewcomplexform />
			objWriter.WriteElementString("karmanewcomplexform", _intKarmaNewComplexForm.ToString());
			// <karmaimprovecomplexform />
			objWriter.WriteElementString("karmaimprovecomplexform", _intKarmaImproveComplexForm.ToString());
			// <karmanuyenper />
			objWriter.WriteElementString("karmanuyenper", _intKarmaNuyenPer.ToString());
			// <karmacontact />
			objWriter.WriteElementString("karmacontact", _intKarmaContact.ToString());
            // <karmaenemy />
            objWriter.WriteElementString("karmaenemy", _intKarmaEnemy.ToString());
            // <karmacarryover />
			objWriter.WriteElementString("karmacarryover", _intKarmaCarryover.ToString());
			// <karmaspirit />
			objWriter.WriteElementString("karmaspirit", _intKarmaSpirit.ToString());
			// <karmamaneuver />
			objWriter.WriteElementString("karmamaneuver", _intKarmaManeuver.ToString());
			// <karmainitiation />
			objWriter.WriteElementString("karmainitiation", _intKarmaInitiation.ToString());
			// <karmametamagic />
			objWriter.WriteElementString("karmametamagic", _intKarmaMetamagic.ToString());
			// <karmacomplexformoption />
			objWriter.WriteElementString("karmacomplexformoption", _intKarmaComplexFormOption.ToString());
			// <karmacomplexformskillsoft />
			objWriter.WriteElementString("karmacomplexformskillsoft", _intKarmaComplexFormSkillfot.ToString());
			// <karmajoingroup />
			objWriter.WriteElementString("karmajoingroup", _intKarmaJoinGroup.ToString());
			// <karmaleavegroup />
			objWriter.WriteElementString("karmaleavegroup", _intKarmaLeaveGroup.ToString());
            // <karmaalchemicalfocus />
			objWriter.WriteElementString("karmaalchemicalfocus", _intKarmaAlchemicalFocus.ToString());
			// <karmabanishingfocus />
			objWriter.WriteElementString("karmabanishingfocus", _intKarmaBanishingFocus.ToString());
			// <karmabindingfocus />
			objWriter.WriteElementString("karmabindingfocus", _intKarmaBindingFocus.ToString());
			// <karmacenteringfocus />
			objWriter.WriteElementString("karmacenteringfocus", _intKarmaCenteringFocus.ToString());
			// <karmacounterspellingfocus />
			objWriter.WriteElementString("karmacounterspellingfocus", _intKarmaCounterspellingFocus.ToString());
            // <karmadisenchantingfocus />
			objWriter.WriteElementString("karmadisenchantingfocus", _intKarmaDisenchantingFocus.ToString());
            // <karmaflexiblesignaturefocus />
			objWriter.WriteElementString("karmaflexiblesignaturefocus", _intKarmaFlexibleSignatureFocus.ToString());
			// <karmamaskingfocus />
			objWriter.WriteElementString("karmamaskingfocus", _intKarmaMaskingFocus.ToString());
			// <karmapowerfocus />
			objWriter.WriteElementString("karmapowerfocus", _intKarmaPowerFocus.ToString());
            // <karmaqifocus />
            objWriter.WriteElementString("karmaqifocus", _intKarmaQiFocus.ToString());
            // <karmaritualspellcastingfocus />
            objWriter.WriteElementString("karmaritualspellcastingfocus", _intKarmaRitualSpellcastingFocus.ToString());
			// <karmaspellcastingfocus />
			objWriter.WriteElementString("karmaspellcastingfocus", _intKarmaSpellcastingFocus.ToString());
            // <karmaspellshapingfocus />
            objWriter.WriteElementString("karmaspellshapingfocus", _intKarmaSpellShapingFocus.ToString());
            // <karmasummoningfocus />
			objWriter.WriteElementString("karmasummoningfocus", _intKarmaSummoningFocus.ToString());
			// <karmasustainingfocus />
			objWriter.WriteElementString("karmasustainingfocus", _intKarmaSustainingFocus.ToString());
			// <karmaweaponfocus />
			objWriter.WriteElementString("karmaweaponfocus", _intKarmaWeaponFocus.ToString());
			// </karmacost>
			objWriter.WriteEndElement();

			// <books>
			objWriter.WriteStartElement("books");
			foreach (string strBook in _lstBooks)
				objWriter.WriteElementString("book", strBook);
			// </books>
			objWriter.WriteEndElement();

			// <defaultbuild>
			objWriter.WriteStartElement("defaultbuild");
			// <buildmethod />
			objWriter.WriteElementString("buildmethod", _strBuildMethod);
			// <buildpoints />
			objWriter.WriteElementString("buildpoints", _intBuildPoints.ToString());
			// <availability />
			objWriter.WriteElementString("availability", _intAvailability.ToString());
			// </defaultbuild>
			objWriter.WriteEndElement();

			// </settings>
			objWriter.WriteEndElement();

			objWriter.WriteEndDocument();
			objWriter.Close();
			objStream.Close();
		}

		/// <summary>
		/// Load the settings from the settings file.
		/// </summary>
		/// <param name="strFileName">Settings file to load from.</param>
		public bool Load(string strFileName)
		{
			_strFileName = strFileName;
			string strFilePath = Path.Combine(Environment.CurrentDirectory, "settings", _strFileName);
			XmlDocument objXmlDocument = new XmlDocument();
			try
			{
				// Make sure the settings file exists. If not, ask the user if they would like to use the default settings file instead. A character cannot be loaded without a settings file.
				if (File.Exists(strFilePath))
					objXmlDocument.Load(strFilePath);
				else
				{
					if (MessageBox.Show(LanguageManager.Instance.GetString("Message_CharacterOptions_CannotLoadSetting").Replace("{0}", _strFileName), LanguageManager.Instance.GetString("MessageTitle_CharacterOptions_CannotLoadSetting"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
					{
						MessageBox.Show(LanguageManager.Instance.GetString("Message_CharacterOptions_CannotLoadCharacter"), LanguageManager.Instance.GetString("MessageText_CharacterOptions_CannotLoadCharacter"), MessageBoxButtons.OK, MessageBoxIcon.Error);
						return false;
					}
					else
					{
						_strFileName = "default.xml";
						objXmlDocument.Load(strFilePath);
					}
				}
			}
			catch
			{
				MessageBox.Show(LanguageManager.Instance.GetString("Message_CharacterOptions_CannotLoadCharacter"), LanguageManager.Instance.GetString("MessageText_CharacterOptions_CannotLoadCharacter"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			XmlNode objXmlNode = objXmlDocument.SelectSingleNode("//settings");
			// Setting name.
			_strName = objXmlDocument.SelectSingleNode("/settings/name").InnerText;
			// Confirm delete.
			objXmlNode.TryGetField("confirmdelete", out _blnConfirmDelete);
			// License Restricted items.
			objXmlNode.TryGetField("licenserestricted", out _blnLicenseRestrictedItems);
			// Confirm Karama ExpenseTryGetField
			objXmlNode.TryGetField("confirmkarmaexpense", out _blnConfirmKarmaExpense);
			// Print all Active Skills with a total value greater than 0 (as opposed to only printing those with a Rating higher than 0).
			objXmlNode.TryGetField("printzeroratingskills", out _blnPrintSkillsWithZeroRating);
			// More Lethal Gameplay.
			objXmlNode.TryGetField("morelethalgameplay", out _blnMoreLethalGameplay);
			// Spirit Force Based on Total MAG.
			objXmlNode.TryGetField("spiritforcebasedontotalmag", out _blnSpiritForceBasedOnTotalMAG);
			// Skill Defaulting Includes Modifers.
			objXmlNode.TryGetField("skilldefaultingincludesmodifiers", out _blnSkillDefaultingIncludesModifiers);
			// Enforce Skill Maximum Modified Rating.
			objXmlNode.TryGetField("enforceskillmaximummodifiedrating", out _blnEnforceSkillMaximumModifiedRating);
			// Cap Skill Rating.
			_blnCapSkillRating = objXmlNode.TryGetField("capskillrating", out _blnCapSkillRating);
			// Print Expenses.
			_blnPrintExpenses = objXmlNode.TryGetField("printexpenses", out _blnPrintExpenses);
			// Nuyen per Build Point
			objXmlNode.TryGetField("nuyenperbp", out _intNuyenPerBP);
			// Knucks use Unarmed
			objXmlNode.TryGetField("knucksuseunarmed", out _blnKnucksUseUnarmed);
			// Allow Initiation in Create Mode
			objXmlNode.TryGetField("allowinitiationincreatemode", out _blnAllowInitiationInCreateMode);
			// Use Points on Broken Groups
			objXmlNode.TryGetField("usepointsonbrokengroups", out _blnUsePointsOnBrokenGroups);
			// Don't Double the Cost of purchasing Qualities in Career Mode
			objXmlNode.TryGetField("dontdoublequalities", out _blnDontDoubleQualityPurchaseCost);
			// Don't Double the Cost of removing Qualities in Career Mode
			objXmlNode.TryGetField("dontdoublequalityrefunds", out _blnDontDoubleQualityRefundCost);
			// Ignore Art Requirements from Street Grimoire
			objXmlNode.TryGetField("ignoreart", out _blnIgnoreArt);
			// Use Cyberleg Stats for Movement
			objXmlNode.TryGetField("cyberlegmovement", out _blnCyberlegMovement);
			// Allow a 2nd Max Attribute
			objXmlNode.TryGetField("allow2ndmaxattribute", out _blnAllow2ndMaxAttribute);
			// Allow using Attribute Points with Exceptional Attribute
			objXmlNode.TryGetField("allowattributepointsonexceptional", out _blnAllowAttributePointsOnExceptional);
			// Free Contacts Multiplier
			objXmlNode.TryGetField("freekarmacontactsmultiplier", out _intFreeContactsMultiplier);
			// Free Contacts Multiplier Enabled
			objXmlNode.TryGetField("freecontactsmultiplierenabled", out _blnFreeContactsMultiplierEnabled);
			// Free Knowledge Multiplier Enabled
			objXmlNode.TryGetField("freekarmaknowledgemultiplierenabled", out _blnFreeKnowledgeMultiplierEnabled);
			objXmlNode.TryGetField("freekarmacontactsmultiplier", out _intFreeContactsMultiplier);
			objXmlNode.TryGetField("freekarmacontacts", out _blnFreeKarmaContacts);
			// Karma Free Knowledge
			objXmlNode.TryGetField("freekarmaknowledge", out _blnFreeKarmaKnowledge);
			// Free Contacts Multiplier
			objXmlNode.TryGetField("freekarmaknowledgemultiplier", out _intFreeKnowledgeMultiplier);
			// No Single Armor Encumbrance
			objXmlNode.TryGetField("nosinglearmorencumbrance", out _blnNoSingleArmorEncumbrance);
			// Ignore Armor Encumbrance
			objXmlNode.TryGetField("ignorearmorencumbrance", out _blnIgnoreArmorEncumbrance);
			// Alternate Armor Encumbrance (BOD+STR)
			objXmlNode.TryGetField("alternatearmorencumbrance", out _blnAlternateArmorEncumbrance);
			// Essence Loss Reduces Maximum Only.
			objXmlNode.TryGetField("esslossreducesmaximumonly", out _blnESSLossReducesMaximumOnly);
			// Allow Skill Regrouping.
			objXmlNode.TryGetField("allowskillregrouping", out _blnAllowSkillRegrouping);
			// Metatype Costs Karma.
			objXmlNode.TryGetField("metatypecostskarma", out _blnMetatypeCostsKarma);
			// Metatype Costs Karma Multiplier.
			objXmlNode.TryGetField("metatypecostskarmamultiplier", out _intMetatypeCostMultiplier);
			// Limb Count.
			objXmlNode.TryGetField("limbcount", out _intLimbCount);
			// Exclude Limb Slot.
			objXmlNode.TryGetField("excludelimbslot", out _strExcludeLimbSlot);
			// Allow Cyberware Essence Cost Discounts.
			objXmlNode.TryGetField("allowcyberwareessdiscounts", out _blnAllowCyberwareESSDiscounts);
			// Strength Affects Recoil.
			objXmlNode.TryGetField("strengthaffectsrecoil", out _blnStrengthAffectsRecoil);
			// Use Maximum Armor Modifications.
			objXmlNode.TryGetField("maximumarmormodifications", out _blnMaximumArmorModifications);
			// Use Armor Suit Capacity.
			objXmlNode.TryGetField("armorsuitcapacity", out _blnArmorSuitCapacity);
			// Allow Armor Degredation.
			objXmlNode.TryGetField("armordegredation", out _blnArmorDegradation);
			// Automatically add Copy Protection Program Option.
			objXmlNode.TryGetField("automaticcopyprotection", out _blnAutomaticCopyProtection);
			// Automatically add Registration Program Option.
			objXmlNode.TryGetField("automaticregistration", out _blnAutomaticRegistration);
			// Whether or not option for Ergonomic Programs affecting a Commlink's effective Response is enabled.
			objXmlNode.TryGetField("ergonomicprogramlimit", out _blnErgonomicProgramsLimit);
			// Whether or not Karma costs for increasing Special Attributes is based on the shown value instead of actual value.
			objXmlNode.TryGetField("specialkarmacostbasedonshownvalue", out _blnSpecialKarmaCostBasedOnShownValue);
			// Allow more than 35 BP in Positive Qualities.
			objXmlNode.TryGetField("exceedpositivequalities", out _blnExceedPositiveQualities);
			// Allow more than 35 BP in Negative Qualities.
			objXmlNode.TryGetField("exceednegativequalities", out _blnExceedNegativeQualities);
			// Character can still only receive 35 BP from Negative Qualities (though they can still add as many as they'd like).
			objXmlNode.TryGetField("exceednegativequalitieslimit", out _blnExceedNegativeQualitiesLimit);
			// Whether or not calculated Vehicle Sensor Ratings should be used.
			objXmlNode.TryGetField("usecalculatedvehiclesensorratings", out _blnUseCalculatedVehicleSensorRatings);
			// Whether or not Restricted items have their cost multiplied.
			objXmlNode.TryGetField("multiplyrestrictedcost", out _blnMultiplyRestrictedCost);
			// Whether or not Forbidden items have their cost multiplied.
			objXmlNode.TryGetField("multiplyforbiddencost", out _blnMultiplyForbiddenCost);
			// Restricted cost multiplier.
			objXmlNode.TryGetField("restrictedcostmultiplier", out _intRestrictedCostMultiplier);
			// Forbidden cost multiplier.
			objXmlNode.TryGetField("forbiddencostmultiplier", out _intForbiddenCostMultiplier);
			// Number of decimal places to round to when calculating Essence.
			objXmlNode.TryGetField("essencedecimals", out _intEssenceDecimals);
			// Whether or not Capacity limits should be enforced.
			objXmlNode.TryGetField("enforcecapacity", out _blnEnforceCapacity);
			// Whether or not Recoil modifiers are restricted (AR 148).
			objXmlNode.TryGetField("restrictrecoil", out _blnRestrictRecoil);
			// Whether or not characters can exceed putting 50% of their points into Attributes.
			objXmlNode.TryGetField("allowexceedattributebp", out _blnAllowExceedAttributeBP);
			// Whether or not character are not restricted to the number of points they can invest in Nuyen.
			objXmlNode.TryGetField("unrestrictednuyen", out _blnUnrestrictedNuyen);
			// Whether or not a Commlink's Response should be calculated based on the number of programms it has running.
			objXmlNode.TryGetField("calculatecommlinkresponse", out _blnCalculateCommlinkResponse);
			// Whether or not Stacked Foci can go a combined Force higher than 6.
			objXmlNode.TryGetField("allowhigherstackedfoci", out _blnAllowHigherStackedFoci);
			// Whether or not Complex Forms are treated as Spell for BP/Karma costs.
			objXmlNode.TryGetField("alternatecomplexformcost", out _blnAlternateComplexFormCost);
			// Whether or not LOG is used in place of Program Ratings for Matrix Tests.
			objXmlNode.TryGetField("alternatematrixattribute", out _blnAlternateMatrixAttribute);
			// Whether or not the user can change the status of a Weapon Mod or Accessory being part of the base Weapon.
			objXmlNode.TryGetField("alloweditpartofbaseweapon", out _blnAllowEditPartOfBaseWeapon);
			// Whether or not the user can mark any piece of Bioware as being Transgenic.
			objXmlNode.TryGetField("allowcustomtransgenics", out _blnAllowCustomTransgenics);
			// Whether or not the user may buy qualities.
            objXmlNode.TryGetField("maybuyqualities", out _blnMayBuyQualities);
            // Whether or not contact points are used instead of fixed contacts.
            objXmlNode.TryGetField("usecontactpoints", out _blnUseContactPoints);
            // Whether or not the user can break Skill Groups while in Create Mode.
			objXmlNode.TryGetField("breakskillgroupsincreatemode", out _blnBreakSkillGroupsInCreateMode);
			// Whether or not any Detection Spell can be taken as Extended range version.
			objXmlNode.TryGetField("extendanydetectionspell", out _blnExtendAnyDetectionSpell);
			// Whether or not dice rolling id allowed for Skills.
			objXmlNode.TryGetField("allowskilldicerolling", out _blnAllowSkillDiceRolling);
			// House rule: Treat the Metatype Attribute Minimum as 1 for the purpose of calculating Karma costs.
			objXmlNode.TryGetField("alternatemetatypeattributekarma", out _blnAlternateMetatypeAttributeKarma);
			// Whether or not a backup copy of the character should be created before they are placed into Career Mode.
			objXmlNode.TryGetField("createbackuponcareer", out _blnCreateBackupOnCareer);
			// Whether or not the alternate uses for the Leadership Skill should be printed.
			objXmlNode.TryGetField("printleadershipalternates", out _blnPrintLeadershipAlternates);
			// Whether or not the alternate uses for the Arcana Skill should be printed.
			objXmlNode.TryGetField("printarcanaalternates", out _blnPrintArcanaAlternates);
			// Whether or not Notes should be printed.
			objXmlNode.TryGetField("printnotes", out _blnPrintNotes);
			// Whether or not Obsolescent can be removed/upgrade in the same manner as Obsolete.
			objXmlNode.TryGetField("allowobsolescentupgrade", out _blnAllowObsolescentUpgrade);
			// Whether or not Bioware Suites can be created and added.
			objXmlNode.TryGetField("allowbiowaresuites", out _blnAllowBiowareSuites);
			// House rule: Free Spirits calculate their Power Points based on their MAG instead of EDG.
			objXmlNode.TryGetField("freespiritpowerpointsmag", out _blnFreeSpiritPowerPointsMAG);
			// House rule: Whether or not Special Attributes count towards the maximum 50% Karma allowed for Attributes during karma gen.
			objXmlNode.TryGetField("specialattributekarmalimit", out _blnSpecialAttributeKarmaLimit);
			// House rule: Whether or not Technomancers can select Autosofts as Complex Forms.
			objXmlNode.TryGetField("technomancerallowautosoft", out _blnTechnomancerAllowAutosoft);

			objXmlNode.TryGetField("autobackstory", out _automaticBackstory);

			objXmlNode = objXmlDocument.SelectSingleNode("//settings/bpcost");
			// Attempt to populate the BP vlaues.
			objXmlNode.TryGetField("bpattribute", out _intBPAttribute);
			objXmlNode.TryGetField("bpattributemax", out _intBPAttributeMax);
			objXmlNode.TryGetField("bpcontact", out _intBPContact);
			objXmlNode.TryGetField("bpmartialart", out _intBPMartialArt);
			objXmlNode.TryGetField("bpmartialartmaneuver", out _intBPMartialArtManeuver);
			objXmlNode.TryGetField("bpskillgroup", out _intBPSkillGroup);
			objXmlNode.TryGetField("bpactiveskill", out _intBPActiveSkill);
			objXmlNode.TryGetField("bpactiveskillspecialization", out _intBPActiveSkillSpecialization);
			objXmlNode.TryGetField("bpknowledgeskill", out _intBPKnowledgeSkill);
			objXmlNode.TryGetField("bpspell", out _intBPSpell);
			objXmlNode.TryGetField("bpfocus", out _intBPFocus);
			objXmlNode.TryGetField("bpspirit", out _intBPSpirit);
			objXmlNode.TryGetField("bpcomplexform", out _intBPComplexForm);
			objXmlNode.TryGetField("bpcomplexformoption", out _intBPComplexFormOption);

			objXmlNode = objXmlDocument.SelectSingleNode("//settings/karmacost");
			// Attempt to populate the Karma values.
			objXmlNode.TryGetField("karmaattribute", out _intKarmaAttribute);
			objXmlNode.TryGetField("karmaquality", out _intKarmaQuality);
			objXmlNode.TryGetField("karmaspecialization", out _intKarmaSpecialization);
			objXmlNode.TryGetField("karmanewknowledgeskill", out _intKarmaNewKnowledgeSkill);
			objXmlNode.TryGetField("karmanewactiveskill", out _intKarmaNewActiveSkill);
			objXmlNode.TryGetField("karmanewskillgroup", out _intKarmaNewSkillGroup);
			objXmlNode.TryGetField("karmaimproveknowledgeskill", out _intKarmaImproveKnowledgeSkill);
			objXmlNode.TryGetField("karmaimproveactiveskill", out _intKarmaImproveActiveSkill);
			objXmlNode.TryGetField("karmaimproveskillgroup", out _intKarmaImproveSkillGroup);
			objXmlNode.TryGetField("karmaspell", out _intKarmaSpell);
			objXmlNode.TryGetField("karmanewcomplexform", out _intKarmaNewComplexForm);
			objXmlNode.TryGetField("karmaimprovecomplexform", out _intKarmaImproveComplexForm);
			objXmlNode.TryGetField("karmanuyenper", out _intKarmaNuyenPer);
			objXmlNode.TryGetField("karmacontact", out _intKarmaContact);
			objXmlNode.TryGetField("karmaenemy", out _intKarmaEnemy);
			objXmlNode.TryGetField("karmacarryover", out _intKarmaCarryover);
			objXmlNode.TryGetField("karmaspirit", out _intKarmaSpirit);
			objXmlNode.TryGetField("karmamaneuver", out _intKarmaManeuver);
			objXmlNode.TryGetField("karmainitiation", out _intKarmaInitiation);
			objXmlNode.TryGetField("karmametamagic", out _intKarmaMetamagic);
			objXmlNode.TryGetField("karmacomplexformoption", out _intKarmaComplexFormOption);
			objXmlNode.TryGetField("karmajoingroup", out _intKarmaJoinGroup);
			objXmlNode.TryGetField("karmaleavegroup", out _intKarmaLeaveGroup);
			objXmlNode.TryGetField("karmacomplexformskillsoft", out _intKarmaComplexFormSkillfot);
            objXmlNode.TryGetField("karmaenhancement", out _intKarmaEnhancement);
			
			// Attempt to load the Karma costs for Foci.
			objXmlNode.TryGetField("karmaalchemicalfocus", out _intKarmaAlchemicalFocus);
			objXmlNode.TryGetField("karmabanishingfocus", out _intKarmaBanishingFocus);
			objXmlNode.TryGetField("karmabindingfocus", out _intKarmaBindingFocus);
			objXmlNode.TryGetField("karmacenteringfocus", out _intKarmaCenteringFocus);
			objXmlNode.TryGetField("karmacounterspellingfocus", out _intKarmaCounterspellingFocus);
			objXmlNode.TryGetField("karmadisenchantingfocus", out _intKarmaDisenchantingFocus);
			objXmlNode.TryGetField("karmaflexiblesignaturefocus", out _intKarmaFlexibleSignatureFocus);
			objXmlNode.TryGetField("karmamaskingfocus", out _intKarmaMaskingFocus);
			objXmlNode.TryGetField("karmapowerfocus", out _intKarmaPowerFocus);
			objXmlNode.TryGetField("karmaqifocus", out _intKarmaQiFocus);
			objXmlNode.TryGetField("karmaritualspellcastingfocus", out _intKarmaRitualSpellcastingFocus);
            objXmlNode.TryGetField("karmaspellcastingfocus", out _intKarmaSpellcastingFocus);
            objXmlNode.TryGetField("karmaspellshapingfocus", out _intKarmaSpellShapingFocus);
            objXmlNode.TryGetField("karmasummoningfocus", out _intKarmaSummoningFocus);
			objXmlNode.TryGetField("karmasustainingfocus", out _intKarmaSustainingFocus);
			objXmlNode.TryGetField("karmaweaponfocus", out _intKarmaWeaponFocus);


			// Load Books.
			_lstBooks.Clear();
			foreach (XmlNode objXmlBook in objXmlDocument.SelectNodes("/settings/books/book"))
				_lstBooks.Add(objXmlBook.InnerText);

			// Load default build settings.
			objXmlNode = objXmlDocument.SelectSingleNode("//settings/defaultbuild");
			objXmlNode.TryGetField("buildmethod", out _strBuildMethod);
			objXmlNode.TryGetField("buildpoints", out _intBuildPoints);
			objXmlNode.TryGetField("availability", out _intAvailability);

			return true;
		}
		#endregion

		#region Properties and Methods
		/// <summary>
		/// Load the Options from the Registry (which will subsequently be converted to the XML Settings File format). Registry keys are deleted once they are read since they will no longer be used.
		/// </summary>
		private void LoadFromRegistry()
		{
			// Confirm delete.
			try
			{
				_blnConfirmDelete = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("confirmdelete").ToString());
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("confirmdelete");
			}
			catch
			{
			}

			// Confirm Karama Expense.
			try
			{
				_blnConfirmKarmaExpense = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("confirmkarmaexpense").ToString());
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("confirmkarmaexpense");
			}
			catch
			{
			}

			// Print all Active Skills with a total value greater than 0 (as opposed to only printing those with a Rating higher than 0).
			try
			{
				_blnPrintSkillsWithZeroRating = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("printzeroratingskills").ToString());
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("printzeroratingskills");
			}
			catch
			{
			}

			// More Lethal Gameplay.
			try
			{
				_blnMoreLethalGameplay = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("morelethalgameplay").ToString());
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("morelethalgameplay");
			}
			catch
			{
			}

			// Spirit Force Based on Total MAG.
			try
			{
				_blnSpiritForceBasedOnTotalMAG = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("spiritforcebasedontotalmag").ToString());
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("spiritforcebasedontotalmag");
			}
			catch
			{
			}

			// Skill Defaulting Includes Modifers.
			try
			{
				_blnSkillDefaultingIncludesModifiers = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("skilldefaultingincludesmodifiers").ToString());
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("skilldefaultingincludesmodifiers");
			}
			catch
			{
			}

			// Enforce Skill Maximum Modified Rating.
			try
			{
				_blnEnforceSkillMaximumModifiedRating = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("enforceskillmaximummodifiedrating").ToString());
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("enforceskillmaximummodifiedrating");
			}
			catch
			{
			}

			// Cap Skill Rating.
			try
			{
				_blnCapSkillRating = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("capskillrating").ToString());
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("capskillrating");
			}
			catch
			{
			}

			// Print Expenses.
			try
			{
				_blnPrintExpenses = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("printexpenses").ToString());
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("printexpenses");
			}
			catch
			{
			}

			// Nuyen per Build Point
			try
			{
				_intNuyenPerBP = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("nuyenperbp").ToString());
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("nuyenperbp");
			}
			catch
			{
			}

            // Free Contacts Multiplier Enabled
			try
			{
                _blnFreeContactsMultiplierEnabled = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("freekarmacontactsmultiplierenabled").ToString());
                Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("freekarmacontactsmultiplierenabled");
			}
			catch
			{
			}

			// Free Contacts Multiplier Value
			try
			{
				_intFreeContactsMultiplier = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("freekarmacontactsmultiplier").ToString());
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("freekarmacontactsmultiplier");
			}
			catch
			{
            }

            // Free Knowledge Multiplier Enabled
            try
            {
                _blnFreeKnowledgeMultiplierEnabled = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("freekarmaknowledgemultiplierenabled").ToString());
                Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("freekarmaknowledgemultiplierenabled");
            }
            catch
            {
            }
            // Free Knowledge Multiplier Value
            try
            {
                _intFreeKnowledgeMultiplier = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("freekarmaknowledgemultiplier").ToString());
                Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("freekarmaknowledgemultiplier");
            }
            catch
            {
            }

            // Karma Free Knowledge Multiplier Enabled
            try
            {
                _blnFreeKnowledgeMultiplierEnabled = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("freeknowledgemultiplierenabled").ToString());
                Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("freeknowledgemultiplierenabled");
            }
            catch
            {
            }
            // Karma Free Knowledge
            try
            {
                _blnFreeKarmaContacts = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("freekarmacontacts").ToString());
                Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("freekarmacontacts");
            }
            catch
            {
            }
            // Karma Free Knowledge
            try
			{
				_blnFreeKarmaKnowledge = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("freekarmaknowledge").ToString());
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("freekarmaknowledge");
			}
			catch
			{
			}

			// No Single Armor Encumbrance
			try
			{
				_blnNoSingleArmorEncumbrance = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("nosinglearmorencumbrance").ToString());
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("nosinglearmorencumbrance");
			}
			catch
			{
			}

			// Essence Loss Reduces Maximum Only.
			try
			{
				_blnESSLossReducesMaximumOnly = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("esslossreducesmaximumonly").ToString());
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("esslossreducesmaximumonly");
			}
			catch
			{
			}

			// Allow Skill Regrouping.
			try
			{
				_blnAllowSkillRegrouping = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("allowskillregrouping").ToString());
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("allowskillregrouping");
			}
			catch
			{
			}

			// Attempt to populate the Karma values.
			try
			{
				_intKarmaAttribute = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmaattribute").ToString());
				_intKarmaQuality = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmaquality").ToString());
				_intKarmaSpecialization = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmaspecialization").ToString());
				_intKarmaNewKnowledgeSkill = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmanewknowledgeskill").ToString());
				_intKarmaNewActiveSkill = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmanewactiveskill").ToString());
				_intKarmaNewSkillGroup = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmanewskillgroup").ToString());
				_intKarmaImproveKnowledgeSkill = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmaimproveknowledgeskill").ToString());
				_intKarmaImproveActiveSkill = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmaimproveactiveskill").ToString());
				_intKarmaImproveSkillGroup = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmaimproveskillgroup").ToString());
				_intKarmaSpell = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmaspell").ToString());
                _intKarmaEnhancement = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmaenhancement").ToString());
                _intKarmaNewComplexForm = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmanewcomplexform").ToString());
				_intKarmaImproveComplexForm = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmaimprovecomplexform").ToString());
				_intKarmaNuyenPer = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmanuyenper").ToString());
				_intKarmaContact = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmacontact").ToString());
                _intKarmaEnemy = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmaenemy").ToString());
				_intKarmaCarryover = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmacarryover").ToString());
				_intKarmaSpirit = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmaspirit").ToString());
				_intKarmaManeuver = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmamaneuver").ToString());
				_intKarmaInitiation = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmainitiation").ToString());
				_intKarmaMetamagic = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmametamagic").ToString());
				_intKarmaComplexFormOption = Convert.ToInt32(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("karmacomplexformoption").ToString());
				// Delete the Registry keys ones the values have been retrieve since they will no longer be used.
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("karmaattribute");
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("karmaquality");
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("karmaspecialization");
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("karmanewknowledgeskill");
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("karmanewactiveskill");
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("karmanewskillgroup");
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("karmaimproveknowledgeskill");
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("karmaimproveactiveskill");
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("karmaimproveskillgroup");
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("karmaspell");
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("karmanewcomplexform");
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("karmaimprovecomplexform");
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("karmanuyenper");
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("karmacontact");
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("karmacarryover");
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("karmaspirit");
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("karmamaneuver");
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("karmainitiation");
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("karmametamagic");
				Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("karmacomplexformoption");
			}
			catch
			{
			}

			// Retrieve the sourcebooks that are in the Registry.
			string strBookList = "";
			try
			{
				strBookList = Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("books").ToString();
			}
			catch
			{
				// We were unable to get the Registry key which means the book options have not been saved yet, so create the default values.
				strBookList = "Shadowrun 5th Edition";
				RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
				objRegistry.SetValue("books", strBookList);
			}
			string[] strBooks = strBookList.Split(',');

			XmlDocument objXmlDocument = new XmlDocument();
			objXmlDocument = XmlManager.Instance.Load("books.xml");

			foreach (string strBookCode in strBooks)
			{
				XmlNode objXmlBook = objXmlDocument.SelectSingleNode("/chummer/books/book[name = \"" + strBookCode + "\"]");
				try
				{
					_lstBooks.Add(objXmlBook["code"].InnerText);
				}
				catch
				{
				}
			}

			// Delete the Registry keys ones the values have been retrieve since they will no longer be used.
			Registry.CurrentUser.CreateSubKey("Software\\Chummer5").DeleteValue("books");
		}

		/// <summary>
		/// Convert a book code into the full name.
		/// </summary>
		/// <param name="strCode">Book code to convert.</param>
		public string BookFromCode(string strCode)
		{
			string strReturn = "";
			XmlNode objXmlBook = _objBookDoc.SelectSingleNode("/chummer/books/book[code = \"" + strCode + "\"]");
			try
			{
				strReturn = objXmlBook["name"].InnerText;
			}
			catch
			{
			}
			return strReturn;
		}

		/// <summary>
		/// Book code (using the translated version if applicable).
		/// </summary>
		/// <param name="strCode">Book code to search for.</param>
		public string LanguageBookShort(string strCode)
		{
			if (strCode == "")
				return "";

			string strReturn = "";
			XmlNode objXmlBook = _objBookDoc.SelectSingleNode("/chummer/books/book[code = \"" + strCode + "\"]");
			try
			{
				if (objXmlBook["altcode"] != null)
					strReturn = objXmlBook["altcode"].InnerText;
				else
					strReturn = strCode;
			}
			catch
			{
			}
			return strReturn;
		}

		/// <summary>
		/// Determine the book's original code by using the alternate code.
		/// </summary>
		/// <param name="strCode">Alternate code to look for.</param>
		public string BookFromAltCode(string strCode)
		{
			if (strCode == "")
				return "";

			XmlNode objXmlBook = _objBookDoc.SelectSingleNode("/chummer/books/book[altcode = \"" + strCode + "\"]");
			if (objXmlBook == null)
				return strCode;
			else
				return objXmlBook["code"].InnerText;
		}

		/// <summary>
		/// Book name (using the translated version if applicable).
		/// </summary>
		/// <param name="strCode">Book code to search for.</param>
		public string LanguageBookLong(string strCode)
		{
			if (strCode == "")
				return "";

			string strReturn = "";
			XmlNode objXmlBook = _objBookDoc.SelectSingleNode("/chummer/books/book[code = \"" + strCode + "\"]");
			try
			{
				if (objXmlBook["translate"] != null)
					strReturn = objXmlBook["translate"].InnerText;
				else
					strReturn = objXmlBook["name"].InnerText;
			}
			catch
			{
			}
			return strReturn;
		}

		/// <summary>
		/// Determine whether or not a given book is in use.
		/// </summary>
		/// <param name="strCode">Book code to search for.</param>
		public bool BookEnabled(string strCode)
		{
			bool blnReturn = false;
			foreach (string strBook in _lstBooks)
			{
				if (strBook == strCode)
				{
					blnReturn = true;
					break;
				}
			}
			return blnReturn;
		}

		/// <summary>
		/// XPath query used to filter items based on the user's selected source books.
		/// </summary>
		public string BookXPath()
		{
			if (_strBookXPath != "")
				return _strBookXPath;

			string strPath = "(";

			foreach (string strBook in _lstBooks)
			{
				if (strBook != "")
					strPath += "source = \"" + strBook + "\" or ";
			}
			strPath = strPath.Substring(0, strPath.Length - 4) + ")";

			if (GlobalOptions.Instance.MissionsOnly)
			{
				strPath += " and not(nomission)";
			}

			_strBookXPath = strPath;
			
			return strPath;
		}

		public List<string> BookLinq()
		{
			return _lstBooks;
		}

		/// <summary>
		/// Whether or not all Active Skills with a total score higher than 0 should be printed.
		/// </summary>
		public bool PrintSkillsWithZeroRating
		{
			get
			{
				return _blnPrintSkillsWithZeroRating;
			}
			set
			{
				_blnPrintSkillsWithZeroRating = value;
			}
		}

		/// <summary>
		/// Whether or not the More Lethal Gameplay optional rule is enabled.
		/// </summary>
		public bool MoreLethalGameplay
		{
			get
			{
				return _blnMoreLethalGameplay;
			}
			set
			{
				_blnMoreLethalGameplay = value;
			}
		}

        /// <summary>
        /// Whether or not to require licensing restricted items.
        /// </summary>
        public bool LicenseRestricted
        {
            get
            {
                return _blnLicenseRestrictedItems;
            }
            set
            {
                _blnLicenseRestrictedItems = value;
            }
        }

        /// <summary>
		/// Whether or not a Spirit's Maximum Force is based on the character's total MAG.
		/// </summary>
		public bool SpiritForceBasedOnTotalMAG
		{
			get
			{
				return _blnSpiritForceBasedOnTotalMAG;
			}
			set
			{
				_blnSpiritForceBasedOnTotalMAG = value;
			}
		}

		/// <summary>
		/// Whether or not Defaulting on a Skill should include any Modifiers.
		/// </summary>
		public bool SkillDefaultingIncludesModifiers
		{
			get
			{
				return _blnSkillDefaultingIncludesModifiers;
			}
			set
			{
				_blnSkillDefaultingIncludesModifiers = value;
			}
		}

		/// <summary>
		/// Whether or not the maximum Skill rating modifiers are set.
		/// </summary>
		public bool EnforceMaximumSkillRatingModifier
		{
			get
			{
				return _blnEnforceSkillMaximumModifiedRating;
			}
			set
			{
				_blnEnforceSkillMaximumModifiedRating = value;
			}
		}

		/// <summary>
		/// Whether or not total Skill ratings are capped at 20 or 2 x natural Attribute + Rating, whichever is higher.
		/// </summary>
		public bool CapSkillRating
		{
			get
			{
				return _blnCapSkillRating;
			}
			set
			{
				_blnCapSkillRating = value;
			}
		}

		/// <summary>
		/// Whether or not the Karma and Nueyn Expenses should be printed on the character sheet.
		/// </summary>
		public bool PrintExpenses
		{
			get
			{
				return _blnPrintExpenses;
			}
			set
			{
				_blnPrintExpenses = value;
			}
		}

		/// <summary>
		/// Amount of Nuyen gained per BP spent.
		/// </summary>
		public int NuyenPerBP
		{
			get
			{
				return _intNuyenPerBP;
			}
			set
			{
				_intNuyenPerBP = value;
			}
		}

		/// <summary>
		/// Whether or not characters in Karma build mode receive free Contacts equal to CHA * 2.
		/// </summary>
		public bool KnucksUseUnarmed
		{
			get
			{
                return _blnKnucksUseUnarmed;
			}
			set
			{
                _blnKnucksUseUnarmed = value;
			}
		}

        /// <summary>
        /// Whether or not characters may use Initiation/Submersion in Create mode.
        /// </summary>
        public bool AllowInitiationInCreateMode
        {
            get
            {
                return _blnAllowInitiationInCreateMode;
            }
            set
            {
                _blnAllowInitiationInCreateMode = value;
            }
        }

        /// <summary>
        /// Whether or not characters can spend skill points on broken groups.
        /// </summary>
        public bool UsePointsOnBrokenGroups
        {
            get
            {
                return _blnUsePointsOnBrokenGroups;
            }
            set
            {
                _blnUsePointsOnBrokenGroups = value;
            }
        }

        /// <summary>
        /// Whether or not characters in Career Mode should pay double for qualities.
        /// </summary>
        public bool DontDoubleQualityPurchases
        {
            get
            {
                return _blnDontDoubleQualityPurchaseCost;
            }
            set
            {
				_blnDontDoubleQualityPurchaseCost = value;
            }
		}

		/// <summary>
		/// Whether or not characters in Career Mode should pay double for removing Negative Qualities.
		/// </summary>
		public bool DontDoubleQualityRefunds
		{
			get
			{
				return _blnDontDoubleQualityRefundCost;
			}
			set
			{
				_blnDontDoubleQualityRefundCost = value;
			}
		}

		/// <summary>
		/// Whether or not to ignore the art requirements from street grimoire.
		/// </summary>
		public bool IgnoreArt
        {
            get
            {
                return _blnIgnoreArt;
            }
            set
            {
                _blnIgnoreArt = value;
            }
        }

        /// <summary>
        /// Whether or not to use stats from Cyberlegs when calculating movement rates
        /// </summary>
        public bool CyberlegMovement
        {
            get
            {
                return _blnCyberlegMovement;
            }
            set
            {
                _blnCyberlegMovement = value;
            }
        }

        /// <summary>
        /// Whether or not to allow a 2nd max attribute with Exceptional Attribute
        /// </summary>
        public bool Allow2ndMaxAttribute
        {
            get
            {
                return _blnAllow2ndMaxAttribute;
            }
            set
            {
                _blnAllow2ndMaxAttribute = value;
            }
        }

        /// <summary>
        /// Whether or not to allow using Attribute points on the bonus point from Exceptional Attribute
        /// </summary>
        public bool AllowAttributePointsOnExceptional
        {
            get
            {
                return _blnAllowAttributePointsOnExceptional;
            }
            set
            {
                _blnAllowAttributePointsOnExceptional = value;
            }
        }

        /// <summary>
		/// The CHA multiplier to be used with the Free Contacts Option.
		/// </summary>
		public int FreeContactsMultiplier
		{
			get
			{
				return _intFreeContactsMultiplier;
			}
			set
			{
				_intFreeContactsMultiplier = value;
			}
		}

        /// <summary>
        /// Whether or not characters get a flat number of BP for free Contacts.
        /// </summary>
        public bool FreeContactsMultiplierEnabled
        {
            get
            {
                return _blnFreeContactsMultiplierEnabled;
            }
            set
            {
                _blnFreeContactsMultiplierEnabled = value;
            }
        }


        /// <summary>
        /// Whether or not characters in Karma build mode receive free Knowledge Skills in the same manner as Priority characters.
        /// </summary>
        public bool FreeKarmaContacts
        {
            get
            {
                return _blnFreeKarmaContacts;
            }
            set
            {
                _blnFreeKarmaContacts = value;
            }
        }

        /// <summary>
        /// Whether or not characters in Karma build mode receive free Knowledge Skills in the same manner as Priority characters.
        /// </summary>
        public bool FreeKarmaKnowledge
		{
			get
			{
				return _blnFreeKarmaKnowledge;
			}
			set
			{
				_blnFreeKarmaKnowledge = value;
			}
		}

        /// <summary>
        /// Whether or not the multiplier for Free Knowledge points are used.
        /// </summary>
        public bool FreeKnowledgeMultiplierEnabled
        {
            get
            {
                return _blnFreeKnowledgeMultiplierEnabled;
            }
            set
            {
                _blnFreeKnowledgeMultiplierEnabled = value;
            }
        }

        /// <summary>
        /// The INT+LOG multiplier to be used with the Free Knowledge Option.
        /// </summary>
        public int FreeKnowledgeMultiplier
        {
            get
            {
                return _intFreeKnowledgeMultiplier;
            }
            set
            {
                _intFreeKnowledgeMultiplier = value;
            }
        }

		/// <summary>
		/// Optional Rule: Whether or not Armor Encumbrance is ignored if only a single piece of Armor is worn.
		/// </summary>
		public bool NoSingleArmorEncumbrance
		{
			get
			{
				return _blnNoSingleArmorEncumbrance;
			}
			set
			{
				_blnNoSingleArmorEncumbrance = value;
			}
		}

		/// <summary>
		/// House Rule: Ignore Armor Encumbrance entirely.
		/// </summary>
		public bool IgnoreArmorEncumbrance
		{
			get
			{
				return _blnIgnoreArmorEncumbrance;
			}
			set
			{
				_blnIgnoreArmorEncumbrance = value;
			}
		}

		/// <summary>
		/// House Rule: Alternate Armor Encumbrance (BOD+STR) instead of (BOD*2).
		/// </summary>
		public bool AlternateArmorEncumbrance
		{
			get
			{
				return _blnAlternateArmorEncumbrance;
			}
			set
			{
				_blnAlternateArmorEncumbrance = value;
			}
		}

		/// <summary>
		/// Whether or not Essence loss only reduces MAG/RES maximum value, not the current value.
		/// </summary>
		public bool ESSLossReducesMaximumOnly
		{
			get
			{
				return _blnESSLossReducesMaximumOnly;
			}
			set
			{
				_blnESSLossReducesMaximumOnly = value;
			}
		}

		/// <summary>
		/// Whether or not characters are allowed to put points into a Skill Group again once it is broken and all Ratings are the same.
		/// </summary>
		public bool AllowSkillRegrouping
		{
			get
			{
				return _blnAllowSkillRegrouping;
			}
			set
			{
				_blnAllowSkillRegrouping = value;
			}
		}

		/// <summary>
		/// Whether or not confirmation messages are shown when deleting an object.
		/// </summary>
		public bool ConfirmDelete
		{
			get
			{
				return _blnConfirmDelete;
			}
			set
			{
				_blnConfirmDelete = value;
			}
		}

		/// <summary>
		/// Wehther or not confirmation messages are shown for Karma Expenses.
		/// </summary>
		public bool ConfirmKarmaExpense
		{
			get
			{
				return _blnConfirmKarmaExpense;
			}
			set
			{
				_blnConfirmKarmaExpense = value;
			}
		}

		/// <summary>
		/// Sourcebooks.
		/// </summary>
		public List<string> Books
		{
			get
			{
				return _lstBooks;
			}
		}

		/// <summary>
		/// Setting name.
		/// </summary>
		public string Name
		{
			get
			{
				return _strName;
			}
			set
			{
				_strName = value;
			}
		}

		/// <summary>
		/// Whether or not Metatypes cost Karma equal to their BP when creating a character with Karma.
		/// </summary>
		public bool MetatypeCostsKarma
		{
			get
			{
				return _blnMetatypeCostsKarma;
			}
			set
			{
				_blnMetatypeCostsKarma = value;
			}
		}

		/// <summary>
		/// Mutiplier for Metatype Karma Costs when converting from BP to Karma.
		/// </summary>
		public int MetatypeCostsKarmaMultiplier
		{
			get
			{
				return _intMetatypeCostMultiplier;
			}
			set
			{
				_intMetatypeCostMultiplier = value;
			}
		}

		/// <summary>
		/// Number of Limbs a standard character has.
		/// </summary>
		public int LimbCount
		{
			get
			{
				return _intLimbCount;
			}
			set
			{
				_intLimbCount = value;
			}
		}

		/// <summary>
		/// Exclude a particular Limb Slot from count towards the Limb Count.
		/// </summary>
		public string ExcludeLimbSlot
		{
			get
			{
				return _strExcludeLimbSlot;
			}
			set
			{
				_strExcludeLimbSlot = value;
			}
		}

		/// <summary>
		/// Allow Cyberware Essence cost discounts.
		/// </summary>
		public bool AllowCyberwareESSDiscounts
		{
			get
			{
				return _blnAllowCyberwareESSDiscounts;
			}
			set
			{
				_blnAllowCyberwareESSDiscounts = value;
			}
		}

		/// <summary>
		/// Whether or not a character's Strength affects Weapon Recoil.
		/// </summary>
		public bool StrengthAffectsRecoil
		{
			get
			{
				return _blnStrengthAffectsRecoil;
			}
			set
			{
				_blnStrengthAffectsRecoil = value;
			}
		}

		/// <summary>
		/// Whether or not Maximum Armor Modifications is in use.
		/// </summary>
		public bool MaximumArmorModifications
		{
			get
			{
				return _blnMaximumArmorModifications;
			}
			set
			{
				_blnMaximumArmorModifications = value;
			}
		}

		/// <summary>
		/// Whether or not Armor Suit Capacity is in use.
		/// </summary>
		public bool ArmorSuitCapacity
		{
			get
			{
				return _blnArmorSuitCapacity;
			}
			set
			{
				_blnArmorSuitCapacity = value;
			}
		}

		/// <summary>
		/// Whether or not Armor Degredation is allowed.
		/// </summary>
		public bool ArmorDegradation
		{
			get
			{
				return _blnArmorDegradation;
			}
			set
			{
				_blnArmorDegradation = value;
			}
		}

		/// <summary>
		/// Whether or not the Copy Protection Program Option should automatically be added.
		/// </summary>
		public bool AutomaticCopyProtection
		{
			get
			{
				return _blnAutomaticCopyProtection;
			}
			set
			{
				_blnAutomaticCopyProtection = value;
			}
		}

		/// <summary>
		/// Whether or not the Registration Program Option should automatically be added.
		/// </summary>
		public bool AutomaticRegistration
		{
			get
			{
				return _blnAutomaticRegistration;
			}
			set
			{
				_blnAutomaticRegistration = value;
			}
		}

		/// <summary>
		/// Whether or not option for Ergonomic Programs affecting a Commlink's effective Response is enabled.
		/// </summary>
		public bool ErgonomicProgramLimit
		{
			get
			{
				return _blnErgonomicProgramsLimit;
			}
			set
			{
				_blnErgonomicProgramsLimit = value;
			}
		}

		/// <summary>
		/// Whether or not the Karma cost for increasing Special Attributes is based on the shown value instead of actual value.
		/// </summary>
		public bool SpecialKarmaCostBasedOnShownValue
		{
			get
			{
				return _blnSpecialKarmaCostBasedOnShownValue;
			}
			set
			{
				_blnSpecialKarmaCostBasedOnShownValue = value;
			}
		}

		/// <summary>
		/// Whether or not characters can have more than 25 BP in Positive Qualities.
		/// </summary>
		public bool ExceedPositiveQualities
		{
			get
			{
				return _blnExceedPositiveQualities;
			}
			set
			{
				_blnExceedPositiveQualities = value;
			}
		}

		/// <summary>
		/// Whether or not characters can have more than 25 BP in Negative Qualities.
		/// </summary>
		public bool ExceedNegativeQualities
		{
			get
			{
				return _blnExceedNegativeQualities;
			}
			set
			{
				_blnExceedNegativeQualities = value;
			}
		}

		/// <summary>
		/// If true, the character will not receive additional BP from Negative Qualities past the initial 25
		/// </summary>
		public bool ExceedNegativeQualitiesLimit
		{
			get
			{
				return _blnExceedNegativeQualitiesLimit;
			}
			set
			{
				_blnExceedNegativeQualitiesLimit = value;
			}
		}

		/// <summary>
		/// Whether or not Vehicles should use the average Rating of all of their Sensors instead of the Sensor Rating given to the Vehicle itself.
		/// </summary>
		public bool UseCalculatedVehicleSensorRatings
		{
			get
			{
				return _blnUseCalculatedVehicleSensorRatings;
			}
			set
			{
				_blnUseCalculatedVehicleSensorRatings = value;
			}
		}

		/// <summary>
		/// Whether or not Restricted items have their cost multiplied.
		/// </summary>
		public bool MultiplyRestrictedCost
		{
			get
			{
				return _blnMultiplyRestrictedCost;
			}
			set
			{
				_blnMultiplyRestrictedCost = value;
			}
		}

		/// <summary>
		/// Whether or not Forbidden items have their cost multiplied.
		/// </summary>
		public bool MultiplyForbiddenCost
		{
			get
			{
				return _blnMultiplyForbiddenCost;
			}
			set
			{
				_blnMultiplyForbiddenCost = value;
			}
		}

		/// <summary>
		/// Cost multiplier for Restricted items.
		/// </summary>
		public int RestrictedCostMultiplier
		{
			get
			{
				return _intRestrictedCostMultiplier;
			}
			set
			{
				_intRestrictedCostMultiplier = value;
			}
		}

		/// <summary>
		/// Cost multiplier for Forbidden items.
		/// </summary>
		public int ForbiddenCostMultiplier
		{
			get
			{
				return _intForbiddenCostMultiplier;
			}
			set
			{
				_intForbiddenCostMultiplier = value;
			}
		}

		/// <summary>
		/// Number of decimal places to round to when calculating Essence.
		/// </summary>
		public int EssenceDecimals
		{
			get
			{
				return _intEssenceDecimals;
			}
			set
			{
				_intEssenceDecimals = value;
			}
		}

		/// <summary>
		/// Whether or not Capacity limits should be enforced.
		/// </summary>
		public bool EnforceCapacity
		{
			get
			{
				return _blnEnforceCapacity;
			}
			set
			{
				_blnEnforceCapacity = value;
			}
		}

		/// <summary>
		/// Whether or not Recoil modifiers are restricted (AR 148).
		/// </summary>
		public bool RestrictRecoil
		{
			get
			{
				return _blnRestrictRecoil;
			}
			set
			{
				_blnRestrictRecoil = value;
			}
		}

		/// <summary>
		/// Whether or not characters can exceed putting 50% of their points into Attributes.
		/// </summary>
		public bool AllowExceedAttributeBP
		{
			get
			{
				return _blnAllowExceedAttributeBP;
			}
			set
			{
				_blnAllowExceedAttributeBP = value;
			}
		}

		/// <summary>
		/// Whether or not characters are unresicted in the number of points they can invest in Nuyen.
		/// </summary>
		public bool UnrestrictedNuyen
		{
			get
			{
				return _blnUnrestrictedNuyen;
			}
			set
			{
				_blnUnrestrictedNuyen = value;
			}
		}

		/// <summary>
		/// Whether or not a Commlink's Response should be calculated based on the number of programs running on it.
		/// </summary>
		public bool CalculateCommlinkResponse
		{
			get
			{
				return _blnCalculateCommlinkResponse;
			}
			set
			{
				_blnCalculateCommlinkResponse = value;
			}
		}

		/// <summary>
		/// Whether or not Stacked Foci can have a combined Force higher than 6.
		/// </summary>
		public bool AllowHigherStackedFoci
		{
			get
			{
				return _blnAllowHigherStackedFoci;
			}
			set
			{
				_blnAllowHigherStackedFoci = value;
			}
		}

		/// <summary>
		/// Whether or not Coplex Forms have the same BP/Karma cost as Spells.
		/// </summary>
		public bool AlternateComplexFormCost
		{
			get
			{
				return _blnAlternateComplexFormCost;
			}
			set
			{
				_blnAlternateComplexFormCost = value;
			}
		}

		/// <summary>
		/// Whether or not LOG is used in place of Program Ratings for Matrix Tests.
		/// </summary>
		public bool AlternateMatrixAttribute
		{
			get
			{
				return _blnAlternateMatrixAttribute;
			}
			set
			{
				_blnAlternateMatrixAttribute = value;
			}
		}

		/// <summary>
		/// Whether or not the user can change the Part of Base Weapon flag for a Weapon Accessory or Mod.
		/// </summary>
		public bool AllowEditPartOfBaseWeapon
		{
			get
			{
				return _blnAllowEditPartOfBaseWeapon;
			}
			set
			{
				_blnAllowEditPartOfBaseWeapon = value;
			}
		}

		/// <summary>
		/// Whether or not the user can mark any piece of Bioware as being Transgenic.
		/// </summary>
		public bool AllowCustomTransgenics
		{
			get
			{
				return _blnAllowCustomTransgenics;
			}
			set
			{
				_blnAllowCustomTransgenics = value;
			}
		}

        /// <summary>
        /// Whether or not the user can buy qualities.
        /// </summary>
        public bool MayBuyQualities
        {
            get
            {
                return _blnMayBuyQualities;
            }
            set
            {
                _blnMayBuyQualities = value;
            }
        }

        /// <summary>
        /// Whether or not to use contact points instead of fixed contacts.
        /// </summary>
        public bool UseContactPoints
        {
            get
            {
                return _blnUseContactPoints;
            }
            set
            {
                _blnUseContactPoints = value;
            }
        }

        /// <summary>
		/// Whether or not the user is allowed to break Skill Groups while in Create Mode.
		/// </summary>
		public bool BreakSkillGroupsInCreateMode
		{
			get
			{
				return _blnBreakSkillGroupsInCreateMode;
			}
			set
			{
				_blnBreakSkillGroupsInCreateMode = value;
			}
		}

		/// <summary>
		/// Whether or not any Detection Spell can be taken as Extended range version.
		/// </summary>
		public bool ExtendAnyDetectionSpell
		{
			get
			{
				return _blnExtendAnyDetectionSpell;
			}
			set
			{
				_blnExtendAnyDetectionSpell = value;
			}
		}

		/// <summary>
		/// Whether or not dice rolling is allowed for Skills.
		/// </summary>
		public bool AllowSkillDiceRolling
		{
			get
			{
				return _blnAllowSkillDiceRolling;
			}
			set
			{
				_blnAllowSkillDiceRolling = value;
			}
		}

		/// <summary>
		/// House rule: Treat the Metatype Attribute Minimum as 1 for the purpose of calculating Karma costs.
		/// </summary>
		public bool AlternateMetatypeAttributeKarma
		{
			get
			{
				return _blnAlternateMetatypeAttributeKarma;
			}
			set
			{
				_blnAlternateMetatypeAttributeKarma = value;
			}
		}

		/// <summary>
		/// Whether or not a backup copy of the character should be created before they are placed into Career Mode.
		/// </summary>
		public bool CreateBackupOnCareer
		{
			get
			{
				return _blnCreateBackupOnCareer;
			}
			set
			{
				_blnCreateBackupOnCareer = value;
			}
		}

		/// <summary>
		/// Whether or not the alternate uses for the Leadership Skill should be printed.
		/// </summary>
		public bool PrintLeadershipAlternates
		{
			get
			{
				return _blnPrintLeadershipAlternates;
			}
			set
			{
				_blnPrintLeadershipAlternates = value;
			}
		}

		/// <summary>
		/// Whether or not a backup copy of the character should be created before they are placed into Career Mode.
		/// </summary>
		public bool PrintArcanaAlternates
		{
			get
			{
				return _blnPrintArcanaAlternates;
			}
			set
			{
				_blnPrintArcanaAlternates = value;
			}
		}

		/// <summary>
		/// Whether or not Notes should be printed.
		/// </summary>
		public bool PrintNotes
		{
			get
			{
				return _blnPrintNotes;
			}
			set
			{
				_blnPrintNotes = value;
			}
		}

		/// <summary>
		/// Whether or not Obsolescent can be removed/upgraded in the same way as Obsolete.
		/// </summary>
		public bool AllowObsolescentUpgrade
		{
			get
			{
				return _blnAllowObsolescentUpgrade;
			}
			set
			{
				_blnAllowObsolescentUpgrade = value;
			}
		}

		/// <summary>
		/// Whether or not Bioware Suites can be added and created.
		/// </summary>
		public bool AllowBiowareSuites
		{
			get
			{
				return _blnAllowBiowareSuites;
			}
			set
			{
				_blnAllowBiowareSuites = value;
			}
		}

		/// <summary>
		/// House rule: Free Spirits calculate their Power Points based on their MAG instead of EDG.
		/// </summary>
		public bool FreeSpiritPowerPointsMAG
		{
			get
			{
				return _blnFreeSpiritPowerPointsMAG;
			}
			set
			{
				_blnFreeSpiritPowerPointsMAG = value;
			}
		}

		/// <summary>
		/// House rule: Whether or not Special Attributes count towards the max 50% karma spent on Attributes.
		/// </summary>
		public bool SpecialAttributeKarmaLimit
		{
			get
			{
				return _blnSpecialAttributeKarmaLimit;
			}
			set
			{
				_blnSpecialAttributeKarmaLimit = value;
			}
		}

		/// <summary>
		/// Whether or not Technomancers can select Autosofts as Complex Forms.
		/// </summary>
		public bool TechnomancerAllowAutosoft
		{
			get
			{
				return _blnTechnomancerAllowAutosoft;
			}
			set
			{
				_blnTechnomancerAllowAutosoft = value;
			}
		}
		#endregion

		#region BP
		/// <summary>
		/// BP cost for each Attribute = this value.
		/// </summary>
		public int BPAttribute
		{
			get
			{
				return _intBPAttribute;
			}
			set
			{
				_intBPAttribute = value;
			}
		}

		/// <summary>
		/// BP cost to raise an Attribute to its Metatype Maximum = this value.
		/// </summary>
		public int BPAttributeMax
		{
			get
			{
				return _intBPAttributeMax;
			}
			set
			{
				_intBPAttributeMax = value;
			}
		}

		/// <summary>
		/// BP cost for each Loyalty, Connection, and Group point = this value.
		/// </summary>
		public int BPContact
		{
			get
			{
				return _intBPContact;
			}
			set
			{
				_intBPContact = value;
			}
		}

		/// <summary>
		/// BP cost for each Martial Arts Rating = this value.
		/// </summary>
		public int BPMartialArt
		{
			get
			{
				return _intBPMartialArt;
			}
			set
			{
				_intBPMartialArt = value;
			}
		}

		/// <summary>
		/// BP cost for each Martial Art Maneuver = this value.
		/// </summary>
		public int BPMartialArtManeuver
		{
			get
			{
				return _intBPMartialArtManeuver;
			}
			set
			{
				_intBPMartialArtManeuver = value;
			}
		}

		/// <summary>
		/// BP cost for each Skill Group Rating = this value.
		/// </summary>
		public int BPSkillGroup
		{
			get
			{
				return _intBPSkillGroup;
			}
			set
			{
				_intBPSkillGroup = value;
			}
		}

		/// <summary>
		/// BP cost for each Active Skill Rating = this value.
		/// </summary>
		public int BPActiveSkill
		{
			get
			{
				return _intBPActiveSkill;
			}
			set
			{
				_intBPActiveSkill = value;
			}
		}

		/// <summary>
		/// BP cost for each Active Skill Specialization = this value.
		/// </summary>
		public int BPActiveSkillSpecialization
		{
			get
			{
				return _intBPActiveSkillSpecialization;
			}
			set
			{
				_intBPActiveSkillSpecialization = value;
			}
		}

		/// <summary>
		/// BP cost for each Knowledge Skill Rating = this value.
		/// </summary>
		public int BPKnowledgeSkill
		{
			get
			{
				return _intBPKnowledgeSkill;
			}
			set
			{
				_intBPKnowledgeSkill = value;
			}
		}

		/// <summary>
		/// BP cost for each Spell = this value.
		/// </summary>
		public int BPSpell
		{
			get
			{
				return _intBPSpell;
			}
			set
			{
				_intBPSpell = value;
			}
		}

		/// <summary>
		/// BP cost for each Rating of Foci.
		/// </summary>
		public int BPFocus
		{
			get
			{
				return _intBPFocus;
			}
			set
			{
				_intBPFocus = value;
			}
		}

		/// <summary>
		/// BP cost for each service a Sprit owes = this value.
		/// </summary>
		public int BPSpirit
		{
			get
			{
				return _intBPSpirit;
			}
			set
			{
				_intBPSpirit = value;
			}
		}

		/// <summary>
		/// BP cost for each Complex Form Rating = this value.
		/// </summary>
		public int BPComplexForm
		{
			get
			{
				return _intBPComplexForm;
			}
			set
			{
				_intBPComplexForm = value;
			}
		}

		/// <summary>
		/// BP cost for each Complex Form Option Rating = this value.
		/// </summary>
		public int BPComplexFormOption
		{
			get
			{
				return _intBPComplexFormOption;
			}
			set
			{
				_intBPComplexFormOption = value;
			}
		}
		#endregion

		#region Karma
		/// <summary>
		/// Karma cost to improve an Attribute = New Rating X this value.
		/// </summary>
		public int KarmaAttribute
		{
			get
			{
				return _intKarmaAttribute;
			}
			set
			{
				_intKarmaAttribute = value;
			}
		}

		/// <summary>
		/// Karma cost to purchase a Quality = BP Cost x this value.
		/// </summary>
		public int KarmaQuality
		{
			get
			{
				return _intKarmaQuality;
			}
			set
			{
				_intKarmaQuality = value;
			}
		}

		/// <summary>
		/// Karma cost to purchase a Specialization = this value.
		/// </summary>
		public int KarmaSpecialization
		{
			get
			{
				return _intKarmaSpecialization;
			}
			set
			{
				_intKarmaSpecialization = value;
			}
		}

		/// <summary>
		/// Karma cost to purchase a new Knowledge Skill = this value.
		/// </summary>
		public int KarmaNewKnowledgeSkill
		{
			get
			{
				return _intKarmaNewKnowledgeSkill;
			}
			set
			{
				_intKarmaNewKnowledgeSkill = value;
			}
		}

		/// <summary>
		/// Karma cost to purchase a new Active Skill = this value.
		/// </summary>
		public int KarmaNewActiveSkill
		{
			get
			{
				return _intKarmaNewActiveSkill;
			}
			set
			{
				_intKarmaNewActiveSkill = value;
			}
		}

		/// <summary>
		/// Karma cost to purchase a new Skill Group = this value.
		/// </summary>
		public int KarmaNewSkillGroup
		{
			get
			{
				return _intKarmaNewSkillGroup;
			}
			set
			{
				_intKarmaNewSkillGroup = value;
			}
		}

		/// <summary>
		/// Karma cost to improve a Knowledge Skill = New Rating x this value.
		/// </summary>
		public int KarmaImproveKnowledgeSkill
		{
			get
			{
				return _intKarmaImproveKnowledgeSkill;
			}
			set
			{
				_intKarmaImproveKnowledgeSkill = value;
			}
		}

		/// <summary>
		/// Karma cost to improve an Active Skill = New Rating x this value.
		/// </summary>
		public int KarmaImproveActiveSkill
		{
			get
			{
				return _intKarmaImproveActiveSkill;
			}
			set
			{
				_intKarmaImproveActiveSkill = value;
			}
		}

		/// <summary>
		/// Karma cost to improve a Skill Group = New Rating x this value.
		/// </summary>
		public int KarmaImproveSkillGroup
		{
			get
			{
				return _intKarmaImproveSkillGroup;
			}
			set
			{
				_intKarmaImproveSkillGroup = value;
			}
		}

		/// <summary>
		/// Karma cost for each Spell = this value.
		/// </summary>
		public int KarmaSpell
		{
			get
			{
				return _intKarmaSpell;
			}
			set
			{
				_intKarmaSpell = value;
			}
		}

        /// <summary>
        /// Karma cost for each Enhancement = this value.
        /// </summary>
        public int KarmaEnhancement
        {
            get
            {
                return _intKarmaEnhancement;
            }
            set
            {
                _intKarmaEnhancement = value;
            }
        }

        /// <summary>
		/// Karma cost for a new Complex Form = this value.
		/// </summary>
		public int KarmaNewComplexForm
		{
			get
			{
				return _intKarmaNewComplexForm;
			}
			set
			{
				_intKarmaNewComplexForm = value;
			}
		}

		/// <summary>
		/// Karma cost to improve a Complex Form = New Rating x this value.
		/// </summary>
		public int KarmaImproveComplexForm
		{
			get
			{
				return _intKarmaImproveComplexForm;
			}
			set
			{
				_intKarmaImproveComplexForm = value;
			}
		}

		/// <summary>
		/// Karma cost for Complex Form Options = Rating x this value.
		/// </summary>
		public int KarmaComplexFormOption
		{
			get
			{
				return _intKarmaComplexFormOption;
			}
			set
			{
				_intKarmaComplexFormOption = value;
			}
		}

		/// <summary>
		/// Karma cost for Complex Form Skillsofts = Rating x this value.
		/// </summary>
		public int KarmaComplexFormSkillsoft
		{
			get
			{
				return _intKarmaComplexFormSkillfot;
			}
			set
			{
				_intKarmaComplexFormSkillfot = value;
			}
		}

		/// <summary>
		/// Amount of Nueyn objtained per Karma point.
		/// </summary>
		public int KarmaNuyenPer
		{
			get
			{
				return _intKarmaNuyenPer;
			}
			set
			{
				_intKarmaNuyenPer = value;
			}
		}

		/// <summary>
		/// Karma cost for a Contact = (Connection + Loyalty) x this value.
		/// </summary>
		public int KarmaContact
		{
			get
			{
				return _intKarmaContact;
			}
			set
			{
				_intKarmaContact = value;
			}
		}

        /// <summary>
        /// Karma cost for an Enemy = (Connection + Loyalty) x this value.
        /// </summary>
        public int KarmaEnemy
        {
            get
            {
                return _intKarmaEnemy;
            }
            set
            {
                _intKarmaEnemy = value;
            }
        }

		/// <summary>
		/// Maximum amount of remaining Karma that is carried over to the character once they are created.
		/// </summary>
		public int KarmaCarryover
		{
			get
			{
				return _intKarmaCarryover;
			}
			set
			{
				_intKarmaCarryover = value;
			}
		}

		/// <summary>
		/// Karma cost for a Spirit = this value.
		/// </summary>
		public int KarmaSpirit
		{
			get
			{
				return _intKarmaSpirit;
			}
			set
			{
				_intKarmaSpirit = value;
			}
		}

		/// <summary>
		/// Karma cost for a Combat Maneuver = this value.
		/// </summary>
		public int KarmaManeuver
		{
			get
			{
				return _intKarmaManeuver;
			}
			set
			{
				_intKarmaManeuver = value;
			}
		}

		/// <summary>
		/// Karma cost for a Initiation = 10 + (New Rating x this value).
		/// </summary>
		public int KarmaInitiation
		{
			get
			{
				return _intKarmaInitiation;
			}
			set
			{
				_intKarmaInitiation = value;
			}
		}

		/// <summary>
		/// Karma cost for a Metamagic = this value.
		/// </summary>
		public int KarmaMetamagic
		{
			get
			{
				return _intKarmaMetamagic;
			}
			set
			{
				_intKarmaMetamagic = value;
			}
		}

		/// <summary>
		/// Karma cost to join a Group = this value.
		/// </summary>
		public int KarmaJoinGroup
		{
			get
			{
				return _intKarmaJoinGroup;
			}
			set
			{
				_intKarmaJoinGroup = value;
			}
		}

		/// <summary>
		/// Karma cost to leave a Group = this value.
		/// </summary>
		public int KarmaLeaveGroup
		{
			get
			{
				return _intKarmaLeaveGroup;
			}
			set
			{
				_intKarmaLeaveGroup = value;
			}
		}

		/// <summary>
		/// Karma cost for Alchemical Foci.
		/// </summary>
		public int KarmaAlchemicalFocus
		{
			get
			{
				return _intKarmaAlchemicalFocus;
			}
			set
			{
                _intKarmaAlchemicalFocus = value;
			}
		}

		/// <summary>
		/// Karma cost for Banishing Foci.
		/// </summary>
		public int KarmaBanishingFocus
		{
			get
			{
				return _intKarmaBanishingFocus;
			}
			set
			{
				_intKarmaBanishingFocus = value;
			}
		}

		/// <summary>
		/// Karma cost for Binding Foci.
		/// </summary>
		public int KarmaBindingFocus
		{
			get
			{
				return _intKarmaBindingFocus;
			}
			set
			{
				_intKarmaBindingFocus = value;
			}
		}

		/// <summary>
		/// Karma cost for Centering Foci.
		/// </summary>
		public int KarmaCenteringFocus
		{
			get
			{
				return _intKarmaCenteringFocus;
			}
			set
			{
				_intKarmaCenteringFocus = value;
			}
		}

		/// <summary>
		/// Karma cost for Counterspelling Foci.
		/// </summary>
		public int KarmaCounterspellingFocus
		{
			get
			{
				return _intKarmaCounterspellingFocus;
			}
			set
			{
				_intKarmaCounterspellingFocus = value;
			}
		}

		/// <summary>
		/// Karma cost for Disenchanting Foci.
		/// </summary>
		public int KarmaDisenchantingFocus
		{
			get
			{
				return _intKarmaDisenchantingFocus;
			}
			set
			{
                _intKarmaDisenchantingFocus = value;
			}
		}

		/// <summary>
		/// Karma cost for Flexible Signature Foci.
		/// </summary>
		public int KarmaFlexibleSignatureFocus
		{
			get
			{
				return _intKarmaFlexibleSignatureFocus;
			}
			set
			{
                _intKarmaFlexibleSignatureFocus = value;
			}
		}

		/// <summary>
		/// Karma cost for Masking Foci.
		/// </summary>
		public int KarmaMaskingFocus
		{
			get
			{
				return _intKarmaMaskingFocus;
			}
			set
			{
				_intKarmaMaskingFocus = value;
			}
		}

		/// <summary>
		/// Karma cost for Power Foci.
		/// </summary>
		public int KarmaPowerFocus
		{
			get
			{
				return _intKarmaPowerFocus;
			}
			set
			{
				_intKarmaPowerFocus = value;
			}
		}

		/// <summary>
		/// Karma cost for Qi Foci.
		/// </summary>
		public int KarmaQiFocus
		{
			get
			{
				return _intKarmaQiFocus;
			}
			set
			{
                _intKarmaQiFocus = value;
			}
		}

		/// <summary>
		/// Karma cost for Ritual Spellcasting Foci.
		/// </summary>
		public int KarmaRitualSpellcastingFocus
		{
			get
			{
				return _intKarmaRitualSpellcastingFocus;
			}
			set
			{
                _intKarmaRitualSpellcastingFocus = value;
			}
		}

        /// <summary>
        /// Karma cost for Spellcasting Foci.
        /// </summary>
        public int KarmaSpellcastingFocus
        {
            get
            {
                return _intKarmaSpellcastingFocus;
            }
            set
            {
                _intKarmaSpellcastingFocus = value;
            }
        }

        /// <summary>
        /// Karma cost for Spell Shaping Foci.
        /// </summary>
        public int KarmaSpellShapingFocus
        {
            get
            {
                return _intKarmaSpellShapingFocus;
            }
            set
            {
                _intKarmaSpellShapingFocus = value;
            }
        }

        /// <summary>
		/// Karma cost for Summoning Foci.
		/// </summary>
		public int KarmaSummoningFocus
		{
			get
			{
				return _intKarmaSummoningFocus;
			}
			set
			{
				_intKarmaSummoningFocus = value;
			}
		}

		/// <summary>
		/// Karma cost for Sustaining Foci.
		/// </summary>
		public int KarmaSustainingFocus
		{
			get
			{
				return _intKarmaSustainingFocus;
			}
			set
			{
				_intKarmaSustainingFocus = value;
			}
		}

		/// <summary>
		/// Karma cost for Weapon Foci.
		/// </summary>
		public int KarmaWeaponFocus
		{
			get
			{
				return _intKarmaWeaponFocus;
			}
			set
			{
				_intKarmaWeaponFocus = value;
			}
		}
		#endregion

		#region Default Build
		/// <summary>
		/// Default build method.
		/// </summary>
		public string BuildMethod
		{
			get
			{
				return _strBuildMethod;
			}
			set
			{
				_strBuildMethod = value;
			}
		}

		/// <summary>
		/// Default number of build points.
		/// </summary>
		public int BuildPoints
		{
			get
			{
				return _intBuildPoints;
			}
			set
			{
				_intBuildPoints = value;
			}
		}

		/// <summary>
		/// Default Availability.
		/// </summary>
		public int Availability
		{
			get
			{
				return _intAvailability;
			}
			set
			{
				_intAvailability = value;
			}
		}


		public bool AutomaticBackstory
		{
			get { return _automaticBackstory; }
			internal set { _automaticBackstory = value; }
		}

		#endregion

	}
}