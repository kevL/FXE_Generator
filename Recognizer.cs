using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

using Microsoft.Win32;

using SpeechLib;


namespace lipsync_editor
{
	/// <summary>
	/// An object that contains information about SAPI Recognizers.
	/// </summary>
	sealed class Recognizer
	{
		#region properties
		/// <summary>
		/// The registry token.
		/// </summary>
		internal ISpeechObjectToken Tok
		{ get; private set; }

		/// <summary>
		/// The parsed id. Eg, "MS-1033-80-DESK"
		/// @note 'Tok.Id' is the full registry path:
		/// "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Speech\Recognizers\Tokens\MS-1033-80-DESK"
		/// </summary>
		internal string Id
		{ get; private set; }

		/// <summary>
		/// The descriptive label. This appears in the Recognizers combobox.
		/// Eg. "Microsoft Speech Recognizer 8.0 for Windows (English - US)"
		/// </summary>
		string Label
		{ get; set; }

		/// <summary>
		/// The languages supported.
		/// @note "Languages" appears in the subkey "Attributes" in the registry.
		/// </summary>
		internal string Langids
		{ get; private set; }
		#endregion properties


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		/// <param name="tok"></param>
		internal Recognizer(ISpeechObjectToken tok)
		{
			logfile.Log("Recognizer() cTor");

			Tok = tok;

			Id = Tok.Id;
			int pos = Id.LastIndexOf('\\');
			Id = Id.Substring(pos + 1);
			logfile.Log(". Id= " + Id);

			Label = Tok.GetDescription();
			logfile.Log(". Label= " + Label);

			Langids = "n/a";

			string keyid = @"SOFTWARE\Microsoft\Speech\Recognizers\Tokens\"
						 + Id
						 + @"\Attributes";

			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyid))
			{
				if (key != null)
				{
					Object o = key.GetValue("Language");
					if (o != null)
					{
						string val = o as String;

						Langids = String.Empty;

						string[] langids = val.Split(';');
						foreach (var langid in langids)
						{
							if (Langids != String.Empty) Langids += "  ";
							Langids += Int32.Parse(langid, NumberStyles.HexNumber).ToString();
						}
					}
				}
			}
			logfile.Log(". Langids= " + Langids);
		}
		#endregion cTor


		#region methods (override)
		/// <summary>
		/// Used by the Recognizers combobox to list the speech-recognition
		/// engines that user has available in his/her windoz ControlPanel under
		/// Speech Recognition.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Label;
		}
		#endregion methods (override)
	}


	/// <summary>
	/// A static object that determines the user's available SpeechRecognizers.
	/// </summary>
	static class SpeechRecognizerLister
	{
		/// <summary>
		/// Adds the user's installed SAPI-compliant SpeechRecognizers (speech
		/// recognition engines) to a specified combobox.
		/// </summary>
		/// <param name="co"></param>
		/// <returns>true if a Recognizer was found and added to the list;
		/// false if user is SoL</returns>
		internal static bool AddSpeechRecognizers(ComboBox co)
		{
			logfile.Log();
			logfile.Log("SpeechRecognizerLister.AddSpeechRecognizers()");

			logfile.Log(". create (SpInprocRecognizer)_recognizer");
			// NOTE: This is your SAPI5.4 SpeechRecognizer (aka SpeechRecognitionEngine) interface.
			// good luck!
			var sr_default = new SpInprocRecognizer();
			logfile.Log(". (SpInprocRecognizer)_recognizer CREATED");

			if (sr_default != null)
			{
				logfile.Log();
				logfile.Log("Recognizer.Id= "               + sr_default.Recognizer.Id);
				logfile.Log("Recognizer.GetDescription()= " + sr_default.Recognizer.GetDescription());
				logfile.Log();

				ISpeechObjectTokens toks = sr_default.GetRecognizers();
				foreach (ISpeechObjectToken tok in toks)
				{
					logfile.Log(". installed.Id= "               + tok.Id);
					logfile.Log(". installed.GetDescription()= " + tok.GetDescription());
				}
				logfile.Log();


				int  id_default = -1;
				bool id_default_found = false;

				var recognizers = new List<Recognizer>();
				foreach (ISpeechObjectToken tok in toks)
				{
					if (tok.GetDescription().Contains("Microsoft Speech Recognizer")) // 8.0+ TODO: other SAPI-compliant speech engines incl/ 3rd-party
					{
						recognizers.Add(new Recognizer(tok));

						if (!id_default_found)
						{
							++id_default;

							if (tok.Id == sr_default.Recognizer.Id)
								id_default_found = true;
						}
					}
				}

				if (recognizers.Count != 0)
				{
					co.DataSource = recognizers;
					co.SelectedIndex = id_default;

					return true;
				}
			}

			logfile.Log(". RECOGNIZER NOT FOUND");
			return false;
		}
	}



/*	sealed class LanguageId
	{
		string Label
		{ get; set; }

		internal int Id
		{ get; private set; }

		internal LanguageId(string label, int id)
		{
			Label = label;
			Id    = id;
		}

		public override string ToString()
		{
			return Label;
		}
	}

	static class LanguageLister
	{
		internal static void AddLanguages(ComboBox co)
		{
//			System.Globalization.CultureInfo[] cultures = System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.AllCultures);
//			foreach (System.Globalization.CultureInfo culture in cultures)
//			{
//				logfile.Log(culture.DisplayName); // holy f8ck what use is that exactly
//			}

			var languages = new List<LanguageId>();

			// msoLanguageIDNone			0	No language specified
//			list.Add(new LanguageId("None - 0000", 0));

			// msoLanguageIDEnglishUS		1033	The English US language
			languages.Add(new LanguageId("English US - 1033", 1033));

			// msoLanguageIDEnglishUK		2057	The English UK language
			languages.Add(new LanguageId("English UK - 2057", 2057));
			// msoLanguageIDEnglishCanadian	4105	The English Canadian language
			languages.Add(new LanguageId("English Canadian - 4105", 4105));

			// msoLanguageIDCzech			1029	The Czech language
			languages.Add(new LanguageId("Czech - 1029", 1029));
			// msoLanguageIDDanish			1030	The Danish language
			languages.Add(new LanguageId("Danish - 1030", 1030));
			// msoLanguageIDFinnish			1035	The Finnish language
			languages.Add(new LanguageId("Finnish - 1035", 1035));
			// msoLanguageIDFrench			1036	The French language
			languages.Add(new LanguageId("French - 1036", 1036));
			// msoLanguageIDGerman			1031	The German language
			languages.Add(new LanguageId("German - 1031", 1031));
			// msoLanguageIDHungarian		1038	The Hungarian language
			languages.Add(new LanguageId("Hungarian - 1038", 1038));
			// msoLanguageIDIcelandic		1039	The Icelandic language
			languages.Add(new LanguageId("Icelandic - 1039", 1039));
			// msoLanguageIDItalian			1040	The Italian language
			languages.Add(new LanguageId("Italian - 1040", 1040));
			// msoLanguageIDMexicanSpanish	2058	The Mexican Spanish language
			languages.Add(new LanguageId("Mexican Spanish - 2058", 2058));
			// msoLanguageIDPolish			1045	The Polish language
			languages.Add(new LanguageId("Polish - 1045", 1045));
			// msoLanguageIDPortuguese		2070	The Portuguese language
			languages.Add(new LanguageId("Portuguese - 2070", 2070));
			// msoLanguageIDRomanian		1048	The Romanian language
			languages.Add(new LanguageId("Romanian - 1048", 1048));
			// msoLanguageIDRussian			1049	The Russian language
			languages.Add(new LanguageId("Russian - 1049", 1049));
			// msoLanguageIDSpanish			1034	The Spanish language
			languages.Add(new LanguageId("Spanish - 1034", 1034));
			// msoLanguageIDSwedish			1053	The Swedish language
			languages.Add(new LanguageId("Swedish - 1053", 1053));
			// msoLanguageIDZulu			1077	The Zulu language
			languages.Add(new LanguageId("Zulu - 1077", 1077));

			co.DataSource = languages;
			co.SelectedIndex = 0;
		}
	} */
}

/*
msoLanguageIDAfrikaans 	1078 	The Afrikaans language
msoLanguageIDAlbanian 	1052 	The Albanian language
msoLanguageIDAmharic 	1118 	The Amharic language
msoLanguageIDArabic 	1025 	The Arabic language
msoLanguageIDArabicAlgeria 	5121 	The Arabic Algeria language
msoLanguageIDArabicBahrain 	15361 	The Arabic Bahrain language
msoLanguageIDArabicEgypt 	3073 	The Arabic Egypt language
msoLanguageIDArabicIraq 	2049 	The Arabic Iraq language
msoLanguageIDArabicJordan 	11265 	The Arabic Jordan language
msoLanguageIDArabicKuwait 	13313 	The Arabic Kuwait language
msoLanguageIDArabicLebanon 	12289 	The Arabic Lebanon language
msoLanguageIDArabicLibya 	4097 	The Arabic Libya language
msoLanguageIDArabicMorocco 	6145 	The Arabic Morocco language
msoLanguageIDArabicOman 	8193 	The Arabic Oman language
msoLanguageIDArabicQatar 	16385 	The Arabic Qatar language
msoLanguageIDArabicSyria 	10241 	The Arabic Syria language
msoLanguageIDArabicTunisia 	7169 	The Arabic Tunisia language
msoLanguageIDArabicUAE 	14337 	The Arabic UAE language
msoLanguageIDArabicYemen 	9217 	The Arabic Yemen language
msoLanguageIDArmenian 	1067 	The Armenian language
msoLanguageIDAssamese 	1101 	The Assamese language
msoLanguageIDAzeriCyrillic 	2092 	The Azerbaijani Cyrillic language
msoLanguageIDAzeriLatin 	1068 	The Azerbaijani Latin language
msoLanguageIDBasque 	1069 	Basque (Basque)
msoLanguageIDBelgianDutch 	2067 	The Belgian Dutch language
msoLanguageIDBelgianFrench 	2060 	The Belgian French language
msoLanguageIDBengali 	1093 	The Bengali language
msoLanguageIDBosnian 	4122 	The Bosnian language
msoLanguageIDBosnianBosniaHerzegovinaCyrillic 	8218 	The Bosnian Bosnia Herzegovina Cyrillic language
msoLanguageIDBosnianBosniaHerzegovinaLatin 	5146 	The Bosnian Bosnia Herzegovina Latin language
msoLanguageIDBrazilianPortuguese 	1046 	The Portuguese (Brazil) language
msoLanguageIDBulgarian 	1026 	The Bulgarian language
msoLanguageIDBurmese 	1109 	The Burmese language
msoLanguageIDByelorussian 	1059 	The Belarusian language
msoLanguageIDCatalan 	1027 	The Catalan language
msoLanguageIDCherokee 	1116 	The Cherokee language
msoLanguageIDChineseHongKongSAR 	3076 	The Chinese Hong Kong SAR language
msoLanguageIDChineseMacaoSAR 	5124 	The Chinese Macao SAR language
msoLanguageIDChineseSingapore 	4100 	The Chinese Singapore language
msoLanguageIDCroatian 	1050 	The Croatian language
msoLanguageIDDivehi 	1125 	The Divehi language
msoLanguageIDDutch 	1043 	The Dutch language
msoLanguageIDEdo 	1126 	The Edo language
msoLanguageIDEnglishAUS 	3081 	The English AUS language
msoLanguageIDEnglishBelize 	10249 	The English Belize language
msoLanguageIDEnglishCaribbean 	9225 	The English Caribbean language
msoLanguageIDEnglishIndonesia 	14345 	The English Indonesia language
msoLanguageIDEnglishIreland 	6153 	The English Ireland language
msoLanguageIDEnglishJamaica 	8201 	The English Jamaica language
msoLanguageIDEnglishNewZealand 	5129 	The English NewZealand language
msoLanguageIDEnglishPhilippines 	13321 	The English Philippines language
msoLanguageIDEnglishSouthAfrica 	7177 	The English South Africa language
msoLanguageIDEnglishTrinidadTobago 	11273 	The English Trinidad Tobago language
msoLanguageIDEnglishZimbabwe 	12297 	The English Zimbabwe language
msoLanguageIDEstonian 	1061 	The Estonian language
msoLanguageIDFaeroese 	1080 	The Faeroese language
msoLanguageIDFarsi 	1065 	The Farsi language
msoLanguageIDFilipino 	1124 	The Filipino language
msoLanguageIDFrenchCameroon 	11276 	The French Cameroon language
msoLanguageIDFrenchCanadian 	3084 	The French Canadian language
msoLanguageIDFrenchCotedIvoire 	12300 	The French Coted Ivoire language
msoLanguageIDFrenchHaiti 	15372 	The French Haiti language
msoLanguageIDFrenchLuxembourg 	5132 	The French Luxembourg language
msoLanguageIDFrenchMali 	13324 	The French Mali language
msoLanguageIDFrenchMonaco 	6156 	The French Monaco language
msoLanguageIDFrenchMorocco 	14348 	The French Morocco language
msoLanguageIDFrenchReunion 	8204 	The French Reunion language
msoLanguageIDFrenchSenegal 	10252 	The French Senegal language
msoLanguageIDFrenchWestIndies 	7180 	The French West Indies language
msoLanguageIDFranchCongoDRC 	9228 	The French Congo DRC language
msoLanguageIDFrisianNetherlands 	1122 	The Frisian Netherlands language
msoLanguageIDFulfulde 	1127 	The Fulfulde language
msoLanguageIDGaelicIreland 	2108 	The Irish (Ireland) language
msoLanguageIDGaelicScotland 	1084 	The Scottish Gaelic language
msoLanguageIDGalician 	1110 	The Galician language
msoLanguageIDGeorgian 	1079 	The Georgian language
msoLanguageIDGermanAustria 	3079 	The German Austria language
msoLanguageIDGermanLiechtenstein 	5127 	The German Liechtenstein language
msoLanguageIDGermanLuxembourg 	4103 	The German Luxembourg language
msoLanguageIDGreek 	1032 	The Greek language
msoLanguageIDGuarani 	1140 	The Guarani language
msoLanguageIDGujarati 	1095 	The Gujarati language
msoLanguageIDHausa 	1128 	The Hausa language
msoLanguageIDHawaiian 	1141 	The Hawaiian language
msoLanguageIDHebrew 	1037 	The Hebrew language
msoLanguageIDHindi 	1081 	The Hindi language
msoLanguageIDIbibio 	1129 	The Ibibio language
msoLanguageIDIgbo 	1136 	The Igbo language
msoLanguageIDIndonesian 	1057 	The Indonesian language
msoLanguageIDInuktitut 	1117 	The Inuktitut language
msoLanguageIDJapanese 	1041 	The Japanese language
msoLanguageIDKannada 	1099 	The Kannada language
msoLanguageIDKanuri 	1137 	The Kanuri language
msoLanguageIDKashmiri 	1120 	The Kashmiri language
msoLanguageIDKashmiriDevanagari 	2144 	The Kashmiri Devanagari language
msoLanguageIDKazakh 	1087 	The Kazakh language
msoLanguageIDKhmer 	1107 	The Khmer language
msoLanguageIDKirghiz 	1088 	The Kirghiz language
msoLanguageIDKonkani 	1111 	The Konkani language
msoLanguageIDKorean 	1042 	The Korean language
msoLanguageIDKyrgyz 	1088 	The Kyrgyz language
msoLanguageIDLao 	1108 	The Lao language
msoLanguageIDLatin 	1142 	The Latin language
msoLanguageIDLatvian 	1062 	The Latvian language
msoLanguageIDLithuanian 	1063 	The Lithuanian language
msoLanguageIDMacedoninanFYROM 	1071 	The Macedonian FYROM language
msoLanguageIDMalayalam 	1100 	The Malayalam language
msoLanguageIDMalayBruneiDarussalam 	2110 	The Malay Brunei Darussalam language
msoLanguageIDMalaysian 	1086 	The Malaysian language
msoLanguageIDMaltese 	1082 	The Maltese language
msoLanguageIDManipuri 	1112 	The Manipuri language
msoLanguageIDMaori 	1153 	The Maori language
msoLanguageIDMarathi 	1102 	The Marathi language
msoLanguageIDMixed 	-2 	The Mixed language
msoLanguageIDMongolian 	1104 	The Mongolian language
msoLanguageIDNepali 	1121 	The Nepali language
msoLanguageIDNoProofing 	1024 	No proofing
msoLanguageIDNorwegianBokmol 	1044 	The Norwegian Bokmol language
msoLanguageIDNorwegianNynorsk 	2068 	The Norwegian Nynorsk language
msoLanguageIDOriya 	1096 	The Odia language
msoLanguageIDOromo 	1138 	The Oromo language
msoLanguageIDPashto 	1123 	The Pashto language
msoLanguageIDPunjabi 	1094 	The Punjabi language
msoLanguageIDQuechuaBolivia 	1131 	The Quechua Bolivia language
msoLanguageIDQuechuaEcuador 	2155 	The Quechua Ecuador language
msoLanguageIDQuechuaPeru 	3179 	The Quechua Peru language
msoLanguageIDRhaetoRomanic 	1047 	The Rhaeto Romanic language
msoLanguageIDRomanianMoldova 	2072 	The Romanian Moldova language
msoLanguageIDRussianMoldova 	2073 	The Russian Moldova language
msoLanguageIDSamiLappish 	1083 	The Sami Lappish language
msoLanguageIDSanskrit 	1103 	The Sanskrit language
msoLanguageIDSepedi 	1132 	The Sepedi language
msoLanguageIDSerbianBosniaHerzegovinaCyrillic 	7194 	The Serbian Bosnia Herzegovina Cyrillic language
msoLanguageIDSerbianBosniaHerzegovinaLatin 	6170 	The Serbian Bosnia Herzegovina Latin language
msoLanguageIDSerbianCyrillic 	3098 	The Serbian Cyrillic language
msoLanguageIDSerbianLatin 	2074 	The Serbian Latin language
msoLanguageIDSesotho 	1072 	The Sesotho language
msoLanguageIDSimplifiedChinese 	2052 	The Simplified Chinese language
msoLanguageIDSindhi 	1113 	The Sindhi language
msoLanguageIDSindhiPakistan 	2137 	The Sindhi Pakistan language
msoLanguageIDSinhalese 	1115 	The Sinhalese language
msoLanguageIDSlovak 	1051 	The Slovak language
msoLanguageIDSlovenian 	1060 	The Slovenian language
msoLanguageIDSomali 	1143 	The Somali language
msoLanguageIDSorbian 	1070 	The Sorbian language
msoLanguageIDSpanishArgentina 	11274 	The Spanish Argentina language
msoLanguageIDSpanishBolivia 	16394 	The Spanish Bolivia language
msoLanguageIDSpanishChile 	13322 	The Spanish Chile language
msoLanguageIDSpanishColombia 	9226 	The Spanish Colombia language
msoLanguageIDSpanishCostaRica 	5130 	The Spanish Costa Rica language
msoLanguageIDSpanishDominicanRepublic 	7178 	The Spanish Dominican Republic language
msoLanguageIDSpanishEcuador 	12298 	The Spanish Ecuador language
msoLanguageIDSpanishElSalvador 	17418 	The Spanish El Salvador language
msoLanguageIDSpanishGuatemala 	4106 	The Spanish Guatemala language
msoLanguageIDSpanishHonduras 	18442 	The Spanish Honduras language
msoLanguageIDSpanishModernSort 	3082 	The Spanish Modern Sort language
msoLanguageIDSpanishNicaragua 	19466 	The Spanish Nicaragua language
msoLanguageIDSpanishPanama 	6154 	The Spanish Panama language
msoLanguageIDSpanishParaguay 	15370 	The Spanish Paraguay language
msoLanguageIDSpanishPeru 	10250 	The Spanish Peru language
msoLanguageIDSpanishPuertoRico 	20490 	The Spanish Puerto Rico language
msoLanguageIDSpanishUruguay 	14346 	The Spanish Uruguay language
msoLanguageIDSpanishVenezuela 	8202 	The Spanish Venezuela language
msoLanguageIDSutu 	1072 	The Sutu language
msoLanguageIDSwahili 	1089 	The Swahili language
msoLanguageIDSwedishFinland 	2077 	The Swedish Finland language
msoLanguageIDSwissFrench 	4108 	The Swiss French language
msoLanguageIDSwissGerman 	2055 	The Swiss German language
msoLanguageIDSwissItalian 	2064 	The Swiss Italian language
msoLanguageIDSyriac 	1114 	The Syriac language
msoLanguageIDTajik 	1064 	The Tajik language
msoLanguageIDTamazight 	1119 	The Tamazight language
msoLanguageIDTamazightLatin 	2143 	The Tamazight Latin language
msoLanguageIDTamil 	1097 	The Tamil language
msoLanguageIDTatar 	1092 	The Tatar language
msoLanguageIDTelugu 	1098 	The Telugu language
msoLanguageIDThai 	1054 	The Thai language
msoLanguageIDTibetan 	1105 	The Tibetan language
msoLanguageIDTigrignaEritrea 	2163 	The Tigrigna Eritrea language
msoLanguageIDTigrignaEthiopic 	1139 	The Tigrigna Ethiopic language
msoLanguageIDTraditionalChinese 	1028 	The Traditional Chinese language
msoLanguageIDTsonga 	1073 	The Tsonga language
msoLanguageIDTswana 	1074 	The Tswana language
msoLanguageIDTurkish 	1055 	The Turkish language
msoLanguageIDTurkmen 	1090 	The Turkmen language
msoLanguageIDUkrainian 	1058 	The Ukrainian language
msoLanguageIDUrdu 	1056 	The Urdu language
msoLanguageIDUzbekCyrillic 	2115 	The Uzbek Cyrillic language
msoLanguageIDUzbekLatin 	1091 	The Uzbek Latin language
msoLanguageIDVenda 	1075 	The Venda language
msoLanguageIDVietnamese 	1066 	The Vietnamese language
msoLanguageIDWelsh 	1106 	The Welsh language
msoLanguageIDXhosa 	1076 	The Xhosa language
msoLanguageIDYi 	1144 	The Yi language
msoLanguageIDYiddish 	1085 	The Yiddish language
msoLanguageIDYoruba 	1130 	The Yoruba language
*/
