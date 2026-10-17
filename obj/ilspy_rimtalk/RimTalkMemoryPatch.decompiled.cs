using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using HarmonyLib;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RimTalk.Client.Player2;
using RimTalk.Data;
using RimTalk.Memory;
using RimTalk.Memory.AI;
using RimTalk.Memory.AI.Client;
using RimTalk.Memory.AI.Client.Dto;
using RimTalk.Memory.AI.Integration;
using RimTalk.Memory.API;
using RimTalk.Memory.Capture;
using RimTalk.Memory.Debug;
using RimTalk.Memory.Injection;
using RimTalk.Memory.Maintenance;
using RimTalk.Memory.UI;
using RimTalk.Memory.Utils;
using RimTalk.Memory.VectorDB;
using RimTalk.MemoryPatch;
using RimTalk.Prompt;
using RimTalk.Service;
using RimTalk.Source.Data;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using UnityEngine.Networking;
using Verse;
using Verse.AI;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: TargetFramework(".NETFramework,Version=v4.8", FrameworkDisplayName = ".NET Framework 4.8")]
[assembly: AssemblyCompany("RimTalkMemoryPatch")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+36908635a5760587054787232359dd30aee2435d")]
[assembly: AssemblyProduct("RimTalkMemoryPatch")]
[assembly: AssemblyTitle("RimTalkMemoryPatch")]
[assembly: AssemblyVersion("1.0.0.0")]
[module: RefSafetyRules(11)]
[CompilerGenerated]
internal sealed class <>z__ReadOnlyArray<T> : IEnumerable, ICollection, IList, IEnumerable<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection<T>, IList<T>
{
	[CompilerGenerated]
	private readonly T[] _items;

	int ICollection.Count => _items.Length;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => this;

	object IList.this[int index]
	{
		get
		{
			return _items[index];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	bool IList.IsFixedSize => true;

	bool IList.IsReadOnly => true;

	int IReadOnlyCollection<T>.Count => _items.Length;

	T IReadOnlyList<T>.this[int index] => _items[index];

	int ICollection<T>.Count => _items.Length;

	bool ICollection<T>.IsReadOnly => true;

	T IList<T>.this[int index]
	{
		get
		{
			return _items[index];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public <>z__ReadOnlyArray(T[] items)
	{
		_items = items;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_items).GetEnumerator();
	}

	void ICollection.CopyTo(Array array, int index)
	{
		((ICollection)_items).CopyTo(array, index);
	}

	int IList.Add(object value)
	{
		throw new NotSupportedException();
	}

	void IList.Clear()
	{
		throw new NotSupportedException();
	}

	bool IList.Contains(object value)
	{
		return ((IList)_items).Contains(value);
	}

	int IList.IndexOf(object value)
	{
		return ((IList)_items).IndexOf(value);
	}

	void IList.Insert(int index, object value)
	{
		throw new NotSupportedException();
	}

	void IList.Remove(object value)
	{
		throw new NotSupportedException();
	}

	void IList.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return ((IEnumerable<T>)_items).GetEnumerator();
	}

	void ICollection<T>.Add(T item)
	{
		throw new NotSupportedException();
	}

	void ICollection<T>.Clear()
	{
		throw new NotSupportedException();
	}

	bool ICollection<T>.Contains(T item)
	{
		return ((ICollection<T>)_items).Contains(item);
	}

	void ICollection<T>.CopyTo(T[] array, int arrayIndex)
	{
		((ICollection<T>)_items).CopyTo(array, arrayIndex);
	}

	bool ICollection<T>.Remove(T item)
	{
		throw new NotSupportedException();
	}

	int IList<T>.IndexOf(T item)
	{
		return ((IList<T>)_items).IndexOf(item);
	}

	void IList<T>.Insert(int index, T item)
	{
		throw new NotSupportedException();
	}

	void IList<T>.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}
}
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Module, AllowMultiple = false, Inherited = false)]
	internal sealed class RefSafetyRulesAttribute : Attribute
	{
		public readonly int Version;

		public RefSafetyRulesAttribute(int P_0)
		{
			Version = P_0;
		}
	}
}
namespace RimTalk.MemoryPatch
{
	public class RimTalkMemoryPatchMod : Mod
	{
		public static RimTalkMemoryPatchSettings _settings;

		private int _apiSettingsHash;

		public static RimTalkMemoryPatchSettings Settings
		{
			get
			{
				if (_settings == null)
				{
					throw new Exception("[RimTalk.Memory] RimTalkMemoryPatchSettings is null");
				}
				return _settings;
			}
		}

		public RimTalkMemoryPatchMod(ModContentPack content)
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			((Mod)this)..ctor(content);
			_settings = ((Mod)this).GetSettings<RimTalkMemoryPatchSettings>();
			_apiSettingsHash = GetApiSettingsHash(_settings);
			PromptNormalizer.UpdateRules(Settings.normalizationRules);
			new Harmony("cj.rimtalk.expandmemory").PatchAll();
			Log.Message("[RimTalk-Expand Memory] Loaded successfully");
			if (Prefs.DevMode)
			{
				Log.Message($"[PromptNormalizer] Initialized with {PromptNormalizer.GetActiveRuleCount()} active rules");
			}
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			Settings.DoSettingsWindowContents(inRect);
			((Mod)this).DoSettingsWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			return "RimTalk-Expand Memory";
		}

		public override void WriteSettings()
		{
			((Mod)this).WriteSettings();
			PromptNormalizer.UpdateRules(Settings.normalizationRules);
			RimTalkMemoryPatchSettings settings = Settings;
			int apiSettingsHash = GetApiSettingsHash(settings);
			if (apiSettingsHash != _apiSettingsHash)
			{
				_apiSettingsHash = apiSettingsHash;
				AIService.ResetClientPool();
				Log.Message("[RimTalk-Expand Memory] AI config changed, client pool reset.");
			}
		}

		private int GetApiSettingsHash(RimTalkMemoryPatchSettings settings)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(settings.UseRimTalkAIConfig.ToString());
			if (settings.ApiConfigs != null)
			{
				foreach (ApiConfig apiConfig in settings.ApiConfigs)
				{
					stringBuilder.AppendLine(apiConfig.Provider.ToString());
					stringBuilder.AppendLine(apiConfig.ApiKey);
					stringBuilder.AppendLine(apiConfig.CustomModelName);
					stringBuilder.AppendLine(apiConfig.CustomUrl);
					stringBuilder.AppendLine(apiConfig.IsEnabled.ToString());
				}
			}
			return stringBuilder.ToString().GetHashCode();
		}
	}
	public class RimTalkMemoryPatchSettings : ModSettings
	{
		public class ReplacementRule : IExposable
		{
			public string pattern = "";

			public string replacement = "";

			public bool isEnabled = true;

			public ReplacementRule()
			{
			}

			public ReplacementRule(string pattern, string replacement, bool isEnabled = true)
			{
				this.pattern = pattern;
				this.replacement = replacement;
				this.isEnabled = isEnabled;
			}

			public void ExposeData()
			{
				Scribe_Values.Look<string>(ref pattern, "pattern", "", false);
				Scribe_Values.Look<string>(ref replacement, "replacement", "", false);
				Scribe_Values.Look<bool>(ref isEnabled, "isEnabled", true, false);
			}
		}

		private class AdvancedSettingsWindow : Window
		{
			private readonly RimTalkMemoryPatchSettings settings;

			private Vector2 scrollPos;

			public override Vector2 InitialSize
			{
				get
				{
					//IL_000a: Unknown result type (might be due to invalid IL or missing references)
					return new Vector2(900f, 760f);
				}
			}

			public AdvancedSettingsWindow(RimTalkMemoryPatchSettings settings)
				: base((IWindowDrawing)null)
			{
				this.settings = settings;
				base.doCloseX = true;
				base.doCloseButton = true;
				base.absorbInputAroundWindow = true;
				base.closeOnClickedOutside = false;
			}

			public override void DoWindowContents(Rect inRect)
			{
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Expected O, but got Unknown
				//IL_003b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Unknown result type (might be due to invalid IL or missing references)
				//IL_004f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0066: Unknown result type (might be due to invalid IL or missing references)
				//IL_0071: Unknown result type (might be due to invalid IL or missing references)
				//IL_007d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0092: Unknown result type (might be due to invalid IL or missing references)
				//IL_009d: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
				//IL_0130: Unknown result type (might be due to invalid IL or missing references)
				//IL_0161: Unknown result type (might be due to invalid IL or missing references)
				//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
				//IL_0201: Unknown result type (might be due to invalid IL or missing references)
				//IL_0232: Unknown result type (might be due to invalid IL or missing references)
				//IL_019f: Unknown result type (might be due to invalid IL or missing references)
				Listing_Standard listing = new Listing_Standard();
				Rect val = default(Rect);
				((Rect)(ref val))..ctor(0f, 0f, ((Rect)(ref inRect)).width - 20f, 2200f);
				Widgets.BeginScrollView(inRect, ref scrollPos, val, true);
				((Listing)listing).Begin(val);
				Text.Font = (GameFont)2;
				listing.Label(Translator.Translate("RimTalk_Settings_AdvancedSettingsTitle"), -1f, (string)null);
				Text.Font = (GameFont)1;
				GUI.color = Color.gray;
				listing.Label(Translator.Translate("RimTalk_Settings_AdvancedSettingsDesc"), -1f, (string)null);
				GUI.color = Color.white;
				((Listing)listing).GapLine(12f);
				settings.DrawCollapsibleSection(listing, TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_DynamicInjectionSection")), ref expandDynamicInjection, delegate
				{
					settings.DrawDynamicInjectionSettings(listing);
				});
				settings.DrawCollapsibleSection(listing, TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_MemoryCapacitySection")), ref expandMemoryCapacity, delegate
				{
					settings.DrawMemoryCapacitySettings(listing);
				});
				settings.DrawCollapsibleSection(listing, TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_MemoryDecaySection")), ref expandDecayRates, delegate
				{
					settings.DrawDecaySettings(listing);
				});
				settings.DrawCollapsibleSection(listing, TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_SummarizationSection")), ref expandSummarization, delegate
				{
					settings.DrawSummarizationSettings(listing);
				});
				if (settings.useAISummarization)
				{
					settings.DrawCollapsibleSection(listing, TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_AIConfigSection")), ref expandAIConfig, delegate
					{
						settings.DrawAIConfigSettings(listing);
					});
				}
				settings.DrawCollapsibleSection(listing, TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_MemoryTypesSection")), ref expandMemoryTypes, delegate
				{
					settings.DrawMemoryTypesSettings(listing);
				});
				settings.DrawCollapsibleSection(listing, TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_VectorEnhancementSection")), ref expandVectorEnhancement, delegate
				{
					settings.DrawVectorEnhancementSettings(listing);
				});
				settings.DrawCollapsibleSection(listing, TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_ExperimentalSection")), ref expandExperimentalFeatures, delegate
				{
					settings.DrawExperimentalFeaturesSettings(listing);
				});
				((Listing)listing).End();
				Widgets.EndScrollView();
			}
		}

		private class PromptNormalizationWindow : Window
		{
			private readonly RimTalkMemoryPatchSettings settings;

			private Vector2 scrollPos;

			public override Vector2 InitialSize
			{
				get
				{
					//IL_000a: Unknown result type (might be due to invalid IL or missing references)
					return new Vector2(750f, 520f);
				}
			}

			public PromptNormalizationWindow(RimTalkMemoryPatchSettings settings)
				: base((IWindowDrawing)null)
			{
				this.settings = settings;
				base.doCloseX = true;
				base.doCloseButton = true;
				base.absorbInputAroundWindow = true;
				base.closeOnClickedOutside = false;
			}

			public override void DoWindowContents(Rect inRect)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Expected O, but got Unknown
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				//IL_004a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0055: Unknown result type (might be due to invalid IL or missing references)
				//IL_0061: Unknown result type (might be due to invalid IL or missing references)
				//IL_0071: Unknown result type (might be due to invalid IL or missing references)
				//IL_007c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0082: Unknown result type (might be due to invalid IL or missing references)
				Listing_Standard val = new Listing_Standard();
				Rect val2 = default(Rect);
				((Rect)(ref val2))..ctor(0f, 0f, ((Rect)(ref inRect)).width - 20f, 420f);
				Widgets.BeginScrollView(inRect, ref scrollPos, val2, true);
				((Listing)val).Begin(val2);
				Text.Font = (GameFont)2;
				val.Label(Translator.Translate("RimTalk_Settings_PromptReplacementTitle"), -1f, (string)null);
				Text.Font = (GameFont)1;
				GUI.color = Color.gray;
				val.Label(Translator.Translate("RimTalk_Settings_PromptReplacementDesc"), -1f, (string)null);
				GUI.color = Color.white;
				((Listing)val).Gap(6f);
				settings.DrawPromptNormalizationSettings(val);
				((Listing)val).End();
				Widgets.EndScrollView();
				PromptNormalizer.UpdateRules(settings.normalizationRules);
			}
		}

		public List<ReplacementRule> normalizationRules = new List<ReplacementRule>();

		public int maxActiveMemories = 6;

		public int maxSituationalMemories = 20;

		public int maxEventLogMemories = 50;

		public bool IsPlayerDialogueInject = true;

		public bool IsRoundMemoryActive = true;

		public int maxABMInjectionRounds = 3;

		public float ScmDecayRate = 0.05f;

		public const float DefaultScmDecayRate = 0.05f;

		public float ElsDecayRate = 0.00384f;

		public const float DefaultElsDecayRate = 0.00384f;

		public float ClpaDecayRate = 0.00128f;

		public const float DefaultClpaDecayRate = 0.00128f;

		public int AbmLifespanHours = 24;

		public bool EnableDailySummarization = true;

		public int summarizationHour;

		public bool useAISummarization = true;

		public int maxSummaryLength = 80;

		public bool EnableAutoArchive = true;

		public int ArchiveIntervalDays = 15;

		public int maxArchiveMemories = 50;

		public bool EnableAILog;

		public bool UseRimTalkAIConfig = true;

		public List<ApiConfig> ApiConfigs = new List<ApiConfig>();

		public string SummarizePrompt = "殖民者{0}的记忆总结\n\n记忆列表\n{1}\n\n要求提炼地点人物事件\n相似事件合并标注频率\n极简表达不超过80字\n只输出总结文字不要其他格式";

		public const string DefaultSummarizePrompt = "殖民者{0}的记忆总结\n\n记忆列表\n{1}\n\n要求提炼地点人物事件\n相似事件合并标注频率\n极简表达不超过80字\n只输出总结文字不要其他格式";

		public string ArchivePrompt = "殖民者{0}的记忆归档\n\n记忆列表\n{1}\n\n要求提炼核心特征和里程碑事件\n合并相似经历突出长期趋势\n极简表达不超过60字\n只输出总结文字不要其他格式";

		public const string DefaultArchivePrompt = "殖民者{0}的记忆归档\n\n记忆列表\n{1}\n\n要求提炼核心特征和里程碑事件\n合并相似经历突出长期趋势\n极简表达不超过60字\n只输出总结文字不要其他格式";

		public int SummaryMaxTokens = 8000;

		public bool enableMemoryUI = true;

		public bool EnableActionMemory = true;

		public bool EnableCombatMemory = true;

		public bool enableConversationMemory = true;

		public bool enablePawnStatusKnowledge;

		public bool enableEventRecordKnowledge;

		public bool enableConversationCache = true;

		public int conversationCacheSize = 200;

		public int conversationCacheExpireDays = 14;

		public bool enablePromptCache = true;

		public int promptCacheSize = 100;

		public int promptCacheExpireMinutes = 60;

		public bool useDynamicInjection = true;

		public int maxInjectedMemories = 10;

		public int maxInjectedKnowledge = 5;

		public float weightTimeDecay = 0.3f;

		public float weightImportance = 0.3f;

		public float weightKeywordMatch = 0.4f;

		public float memoryScoreThreshold = 0.15f;

		public float knowledgeScoreThreshold = 0.1f;

		public bool enableAdaptiveThreshold;

		public bool autoApplyAdaptiveThreshold;

		public bool enableProactiveRecall;

		public float recallTriggerChance = 0.15f;

		public bool enableVectorEnhancement;

		public float vectorSimilarityThreshold = 0.75f;

		public int maxVectorResults = 5;

		public string embeddingApiKey = "";

		public string embeddingApiUrl = "https://api.siliconflow.cn/v1/embeddings";

		public string embeddingModel = "BAAI/bge-m3";

		public bool enableKnowledgeChaining;

		public int maxChainingRounds = 2;

		public List<string> knowledgeMatchingSources = new List<string> { "prompt", "fullname", "role", "age", "gender", "backstory", "traits", "skills", "relations" };

		private static bool expandDynamicInjection;

		private static bool expandMemoryCapacity;

		private static bool expandDecayRates;

		private static bool expandSummarization;

		private static bool expandAIConfig;

		private static bool expandMemoryTypes;

		private static bool expandVectorEnhancement;

		private static bool expandExperimentalFeatures;

		private static Vector2 scrollPosition;

		public override void ExposeData()
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Invalid comparison between Unknown and I4
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Invalid comparison between Unknown and I4
			//IL_0496: Unknown result type (might be due to invalid IL or missing references)
			//IL_049c: Invalid comparison between Unknown and I4
			//IL_0524: Unknown result type (might be due to invalid IL or missing references)
			//IL_052a: Invalid comparison between Unknown and I4
			((ModSettings)this).ExposeData();
			Scribe_Collections.Look<ReplacementRule>(ref normalizationRules, "normalizationRules", (LookMode)2, Array.Empty<object>());
			if ((int)Scribe.mode == 4 && normalizationRules == null)
			{
				normalizationRules = new List<ReplacementRule>();
			}
			int num = 6;
			Scribe_Values.Look<int>(ref num, "fourLayer_maxActiveMemories", 6, false);
			Scribe_Values.Look<int>(ref maxSituationalMemories, "fourLayer_maxSituationalMemories", 20, false);
			Scribe_Values.Look<int>(ref maxEventLogMemories, "fourLayer_maxEventLogMemories", 50, false);
			Scribe_Values.Look<int>(ref maxABMInjectionRounds, "fourLayer_maxABMInjectionRounds", 0, false);
			Scribe_Values.Look<bool>(ref IsPlayerDialogueInject, "fourLayer_isPlayerDialogueInject", true, false);
			Scribe_Values.Look<bool>(ref IsRoundMemoryActive, "fourLayer_IsRoundMemoryActive", true, false);
			Scribe_Values.Look<float>(ref ScmDecayRate, "fourLayer_scmDecayRate", 0.05f, false);
			Scribe_Values.Look<float>(ref ElsDecayRate, "fourLayer_elsDecayRate", 0.00384f, false);
			Scribe_Values.Look<float>(ref ClpaDecayRate, "fourLayer_clpaDecayRate", 0.00128f, false);
			Scribe_Values.Look<int>(ref AbmLifespanHours, "fourLayer_abmLifespanHours", 24, false);
			Scribe_Values.Look<bool>(ref EnableDailySummarization, "fourLayer_enableDailySummarization", true, false);
			Scribe_Values.Look<int>(ref summarizationHour, "fourLayer_summarizationHour", 0, false);
			Scribe_Values.Look<bool>(ref useAISummarization, "fourLayer_useAISummarization", true, false);
			Scribe_Values.Look<int>(ref maxSummaryLength, "fourLayer_maxSummaryLength", 80, false);
			Scribe_Values.Look<bool>(ref EnableAutoArchive, "fourLayer_enableAutoArchive", true, false);
			Scribe_Values.Look<int>(ref ArchiveIntervalDays, "fourLayer_archiveIntervalDays", 15, false);
			Scribe_Values.Look<int>(ref maxArchiveMemories, "fourLayer_maxArchiveMemories", 50, false);
			Scribe_Values.Look<bool>(ref EnableAILog, "EnableAILog", false, false);
			Scribe_Values.Look<bool>(ref UseRimTalkAIConfig, "UseRimTalkConfig", true, false);
			Scribe_Collections.Look<ApiConfig>(ref ApiConfigs, "ApiConfigs", (LookMode)2, Array.Empty<object>());
			if ((int)Scribe.mode == 4 && ApiConfigs == null)
			{
				ApiConfigs = new List<ApiConfig>();
			}
			Scribe_Values.Look<string>(ref SummarizePrompt, "ai_dailySummaryPrompt", "", false);
			Scribe_Values.Look<string>(ref ArchivePrompt, "ai_deepArchivePrompt", "", false);
			Scribe_Values.Look<int>(ref SummaryMaxTokens, "ai_summaryMaxTokens", 8000, false);
			Scribe_Values.Look<bool>(ref enableMemoryUI, "memoryPatch_enableMemoryUI", true, false);
			Scribe_Values.Look<bool>(ref EnableActionMemory, "memoryPatch_enableActionMemory", true, false);
			Scribe_Values.Look<bool>(ref EnableCombatMemory, "EnableCombatMemory", true, false);
			Scribe_Values.Look<bool>(ref enableConversationMemory, "memoryPatch_enableConversationMemory", true, false);
			Scribe_Values.Look<bool>(ref enablePawnStatusKnowledge, "pawnStatus_enablePawnStatusKnowledge", false, false);
			Scribe_Values.Look<bool>(ref enableEventRecordKnowledge, "eventRecord_enableEventRecordKnowledge", false, false);
			Scribe_Values.Look<bool>(ref enableConversationCache, "cache_enableConversationCache", true, false);
			Scribe_Values.Look<int>(ref conversationCacheSize, "cache_conversationCacheSize", 200, false);
			Scribe_Values.Look<int>(ref conversationCacheExpireDays, "cache_conversationCacheExpireDays", 14, false);
			Scribe_Values.Look<bool>(ref enablePromptCache, "cache_enablePromptCache", true, false);
			Scribe_Values.Look<int>(ref promptCacheSize, "cache_promptCacheSize", 100, false);
			Scribe_Values.Look<int>(ref promptCacheExpireMinutes, "cache_promptCacheExpireMinutes", 60, false);
			Scribe_Values.Look<bool>(ref useDynamicInjection, "dynamic_useDynamicInjection", true, false);
			Scribe_Values.Look<int>(ref maxInjectedMemories, "dynamic_maxInjectedMemories", 10, false);
			Scribe_Values.Look<int>(ref maxInjectedKnowledge, "dynamic_maxInjectedKnowledge", 5, false);
			Scribe_Values.Look<float>(ref weightTimeDecay, "dynamic_weightTimeDecay", 0.3f, false);
			Scribe_Values.Look<float>(ref weightImportance, "dynamic_weightImportance", 0.3f, false);
			Scribe_Values.Look<float>(ref weightKeywordMatch, "dynamic_weightKeywordMatch", 0.4f, false);
			Scribe_Values.Look<float>(ref memoryScoreThreshold, "dynamic_memoryScoreThreshold", 0.15f, false);
			Scribe_Values.Look<float>(ref knowledgeScoreThreshold, "dynamic_knowledgeScoreThreshold", 0.1f, false);
			Scribe_Values.Look<bool>(ref enableAdaptiveThreshold, "adaptive_enableAdaptiveThreshold", false, false);
			Scribe_Values.Look<bool>(ref autoApplyAdaptiveThreshold, "adaptive_autoApplyAdaptiveThreshold", false, false);
			Scribe_Values.Look<bool>(ref enableProactiveRecall, "recall_enableProactiveRecall", false, false);
			Scribe_Values.Look<float>(ref recallTriggerChance, "recall_triggerChance", 0.15f, false);
			Scribe_Values.Look<bool>(ref enableVectorEnhancement, "vector_enableVectorEnhancement", false, false);
			Scribe_Values.Look<float>(ref vectorSimilarityThreshold, "vector_vectorSimilarityThreshold", 0.75f, false);
			Scribe_Values.Look<int>(ref maxVectorResults, "vector_maxVectorResults", 5, false);
			Scribe_Values.Look<string>(ref embeddingApiKey, "vector_embeddingApiKey", "", false);
			Scribe_Values.Look<string>(ref embeddingApiUrl, "vector_embeddingApiUrl", "https://api.siliconflow.cn/v1/embeddings", false);
			Scribe_Values.Look<string>(ref embeddingModel, "vector_embeddingModel", "BAAI/bge-m3", false);
			Scribe_Values.Look<bool>(ref enableKnowledgeChaining, "knowledge_enableKnowledgeChaining", false, false);
			Scribe_Values.Look<int>(ref maxChainingRounds, "knowledge_maxChainingRounds", 2, false);
			Scribe_Collections.Look<string>(ref knowledgeMatchingSources, "knowledgeMatchingSources", (LookMode)1, Array.Empty<object>());
			if ((int)Scribe.mode == 4 && (knowledgeMatchingSources == null || knowledgeMatchingSources.Count == 0))
			{
				knowledgeMatchingSources = new List<string> { "prompt", "fullname", "role", "age", "gender", "backstory", "traits", "skills", "relations" };
			}
			if ((int)Scribe.mode == 4 && knowledgeMatchingSources != null)
			{
				int num2 = knowledgeMatchingSources.RemoveAll((string s) => s == "knowledge" || s.StartsWith("knowledge.", StringComparison.OrdinalIgnoreCase));
				if (num2 > 0)
				{
					Log.Message($"[MemoryPatch] Removed {num2} 'knowledge' entries from matching sources to prevent self-referencing.");
				}
			}
		}

		public void DoSettingsWindowContents(Rect inRect)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected O, but got Unknown
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			Listing_Standard val = new Listing_Standard();
			Rect val2 = default(Rect);
			((Rect)(ref val2))..ctor(0f, 0f, ((Rect)(ref inRect)).width - 20f, 1400f);
			Widgets.BeginScrollView(inRect, ref scrollPosition, val2, true);
			((Listing)val).Begin(val2);
			DrawPresetConfiguration(val);
			((Listing)val).Gap(12f);
			DrawQuickActionButtons(val);
			((Listing)val).GapLine(12f);
			Text.Font = (GameFont)2;
			val.Label(Translator.Translate("RimTalk_Settings_APIConfigTitle"), -1f, (string)null);
			Text.Font = (GameFont)1;
			DrawAIConfigSettings(val);
			((Listing)val).GapLine(12f);
			if (Widgets.ButtonText(((Listing)val).GetRect(40f, 1f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_AdvancedSettings")), true, true, true, (TextAnchor?)null))
			{
				Find.WindowStack.Add((Window)(object)new AdvancedSettingsWindow(this));
			}
			((Listing)val).End();
			Widgets.EndScrollView();
		}

		private void DrawPresetConfiguration(Listing_Standard listing)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			Text.Font = (GameFont)2;
			listing.Label(Translator.Translate("RimTalk_Settings_PresetConfig"), -1f, (string)null);
			Text.Font = (GameFont)1;
			GUI.color = Color.gray;
			listing.Label(Translator.Translate("RimTalk_Settings_PresetConfigDesc"), -1f, (string)null);
			GUI.color = Color.white;
			((Listing)listing).Gap(6f);
			Rect rect = ((Listing)listing).GetRect(95f, 1f);
			float num = 10f;
			float num2 = (((Rect)(ref rect)).width - num * 2f) / 3f;
			float height = ((Rect)(ref rect)).height;
			DrawPresetCard(new Rect(((Rect)(ref rect)).x, ((Rect)(ref rect)).y, num2, height), TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_PresetLight")), 3, 2, 250);
			DrawPresetCard(new Rect(((Rect)(ref rect)).x + num2 + num, ((Rect)(ref rect)).y, num2, height), TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_PresetBalanced")), 6, 4, 520);
			DrawPresetCard(new Rect(((Rect)(ref rect)).x + 2f * (num2 + num), ((Rect)(ref rect)).y, num2, height), TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_PresetEnhanced")), 10, 6, 850);
			((Listing)listing).GapLine(12f);
		}

		private void DrawPresetCard(Rect rect, string title, int memoryCount, int knowledgeCount, int tokenEstimate)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			Widgets.DrawBoxSolid(rect, new Color(0.18f, 0.18f, 0.18f, 0.6f));
			Widgets.DrawHighlightIfMouseover(rect);
			Text.Anchor = (TextAnchor)4;
			GUI.color = new Color(0.8f, 0.9f, 1f);
			Widgets.Label(rect, TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_PresetCardContent", NamedArgument.op_Implicit(title), NamedArgument.op_Implicit(memoryCount), NamedArgument.op_Implicit(knowledgeCount), NamedArgument.op_Implicit(tokenEstimate)));
			GUI.color = Color.white;
			Text.Anchor = (TextAnchor)0;
			TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_PresetCardTooltip", NamedArgument.op_Implicit(memoryCount), NamedArgument.op_Implicit(knowledgeCount), NamedArgument.op_Implicit(tokenEstimate))));
			if (Widgets.ButtonInvisible(rect, true))
			{
				useDynamicInjection = true;
				maxInjectedMemories = memoryCount;
				maxInjectedKnowledge = knowledgeCount;
				Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_PresetApplied", NamedArgument.op_Implicit(title), NamedArgument.op_Implicit(memoryCount), NamedArgument.op_Implicit(knowledgeCount))), MessageTypeDefOf.PositiveEvent, true);
			}
		}

		private void DrawQuickActionButtons(Listing_Standard listing)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			Text.Font = (GameFont)2;
			listing.Label(Translator.Translate("RimTalk_Settings_FeatureEntries"), -1f, (string)null);
			Text.Font = (GameFont)1;
			((Listing)listing).Gap(4f);
			Rect rect = ((Listing)listing).GetRect(60f, 1f);
			float num = 10f;
			float num2 = (((Rect)(ref rect)).width - num * 2f) / 3f;
			float height = ((Rect)(ref rect)).height;
			DrawActionButton(new Rect(((Rect)(ref rect)).x, ((Rect)(ref rect)).y, num2, height), TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_KnowledgeLibrary")), TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_KnowledgeLibraryTip")), delegate
			{
				OpenCommonKnowledgeDialog();
			});
			DrawActionButton(new Rect(((Rect)(ref rect)).x + num2 + num, ((Rect)(ref rect)).y, num2, height), TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_PromptReplacement")), TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_PromptReplacementTip")), delegate
			{
				Find.WindowStack.Add((Window)(object)new PromptNormalizationWindow(this));
			});
			DrawActionButton(new Rect(((Rect)(ref rect)).x + 2f * (num2 + num), ((Rect)(ref rect)).y, num2, height), TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_InjectionPreviewer")), TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_InjectionPreviewerTip")), delegate
			{
				Find.WindowStack.Add((Window)(object)new Dialog_InjectionPreview());
			});
		}

		private void DrawActionButton(Rect rect, string label, string tip, Action onClick)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			if (Widgets.ButtonText(rect, label, true, true, true, (TextAnchor?)null))
			{
				onClick?.Invoke();
			}
			TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tip));
		}

		private void DrawCollapsibleSection(Listing_Standard listing, string title, ref bool expanded, Action drawContent)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			Rect rect = ((Listing)listing).GetRect(30f, 1f);
			Widgets.DrawBoxSolid(rect, new Color(0.2f, 0.2f, 0.2f, 0.5f));
			Text.Font = (GameFont)2;
			Widgets.Label(new Rect(((Rect)(ref rect)).x + 30f, ((Rect)(ref rect)).y + 3f, ((Rect)(ref rect)).width - 30f, ((Rect)(ref rect)).height), title);
			Text.Font = (GameFont)1;
			if (Widgets.ButtonImage(new Rect(((Rect)(ref rect)).x + 5f, ((Rect)(ref rect)).y + 7f, 20f, 20f), expanded ? TexButton.Collapse : TexButton.Reveal, true, (string)null))
			{
				expanded = !expanded;
			}
			((Listing)listing).Gap(3f);
			if (expanded)
			{
				((Listing)listing).Gap(3f);
				drawContent?.Invoke();
				((Listing)listing).Gap(6f);
			}
			((Listing)listing).GapLine(12f);
		}

		private void DrawDynamicInjectionSettings(Listing_Standard listing)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0203: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Unknown result type (might be due to invalid IL or missing references)
			//IL_023e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0243: Unknown result type (might be due to invalid IL or missing references)
			//IL_024e: Unknown result type (might be due to invalid IL or missing references)
			//IL_025f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0264: Unknown result type (might be due to invalid IL or missing references)
			//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0323: Unknown result type (might be due to invalid IL or missing references)
			//IL_0328: Unknown result type (might be due to invalid IL or missing references)
			//IL_0333: Unknown result type (might be due to invalid IL or missing references)
			//IL_036b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0370: Unknown result type (might be due to invalid IL or missing references)
			//IL_037b: Unknown result type (might be due to invalid IL or missing references)
			listing.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_EnableDynamicInjection")), ref useDynamicInjection, (string)null, 0f, 1f);
			if (!useDynamicInjection)
			{
				return;
			}
			GUI.color = new Color(0.8f, 1f, 0.8f);
			listing.Label("  " + Translator.Translate("RimTalk_Settings_DynamicInjectionDesc"), -1f, (string)null);
			GUI.color = Color.white;
			((Listing)listing).Gap(12f);
			listing.Label(TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_MaxABMInjectionRoundsLabel", NamedArgument.op_Implicit(maxABMInjectionRounds)), -1f, (string)null);
			maxABMInjectionRounds = (int)listing.Slider((float)maxABMInjectionRounds, 1f, 10f);
			GUI.color = Color.gray;
			listing.Label("  " + Translator.Translate("RimTalk_Settings_MaxABMInjectionRoundsDesc"), -1f, (string)null);
			GUI.color = Color.white;
			((Listing)listing).Gap(12f);
			listing.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_IsPlayerDialogueInject")), ref IsPlayerDialogueInject, (string)null, 0f, 1f);
			GUI.color = Color.gray;
			listing.Label("  " + Translator.Translate("RimTalk_Settings_IsPlayerDialogueInjectDesc"), -1f, (string)null);
			GUI.color = Color.white;
			((Listing)listing).Gap(12f);
			bool isRoundMemoryActive = IsRoundMemoryActive;
			listing.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_IsRoundMemoryActive")), ref IsRoundMemoryActive, (string)null, 0f, 1f);
			GUI.color = Color.gray;
			listing.Label("  " + Translator.Translate("RimTalk_Settings_IsRoundMemoryActiveDesc"), -1f, (string)null);
			GUI.color = Color.white;
			if (isRoundMemoryActive != IsRoundMemoryActive)
			{
				if (IsRoundMemoryActive)
				{
					maxABMInjectionRounds = 3;
				}
				else
				{
					maxABMInjectionRounds = 0;
				}
			}
			((Listing)listing).Gap(12f);
			listing.Label(TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_MaxInjectedMemoriesLabel", NamedArgument.op_Implicit(maxInjectedMemories)), -1f, (string)null);
			maxInjectedMemories = (int)listing.Slider((float)maxInjectedMemories, 1f, 20f);
			listing.Label(TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_MaxInjectedKnowledgeLabel", NamedArgument.op_Implicit(maxInjectedKnowledge)), -1f, (string)null);
			Rect rect = ((Listing)listing).GetRect(28f, 1f);
			Rect val = default(Rect);
			((Rect)(ref val))..ctor(((Rect)(ref rect)).x, ((Rect)(ref rect)).y, ((Rect)(ref rect)).width - 70f, 28f);
			Rect val2 = new Rect(((Rect)(ref rect)).xMax - 60f, ((Rect)(ref rect)).y, 60f, 24f);
			maxInjectedKnowledge = (int)Widgets.HorizontalSlider(val, (float)maxInjectedKnowledge, 0f, 100f, true, (string)null, (string)null, (string)null, -1f);
			string text = maxInjectedKnowledge.ToString();
			text = Widgets.TextField(val2, text);
			if (int.TryParse(text, out var result))
			{
				maxInjectedKnowledge = Mathf.Clamp(result, 0, 100);
			}
			((Listing)listing).Gap(12f);
			listing.Label(TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_MemoryScoreThresholdLabel", NamedArgument.op_Implicit(memoryScoreThreshold.ToString("P0"))), -1f, (string)null);
			memoryScoreThreshold = listing.Slider(memoryScoreThreshold, 0f, 1f);
			listing.Label(TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_KnowledgeScoreThresholdLabel", NamedArgument.op_Implicit(knowledgeScoreThreshold.ToString("P0"))), -1f, (string)null);
			knowledgeScoreThreshold = listing.Slider(knowledgeScoreThreshold, 0f, 1f);
		}

		private void DrawMemoryCapacitySettings(Listing_Standard listing)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			listing.Label(TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_SCMCapacityLabel", NamedArgument.op_Implicit(maxSituationalMemories)), -1f, (string)null);
			maxSituationalMemories = (int)listing.Slider((float)maxSituationalMemories, 10f, 50f);
			listing.Label(TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_ELSCapacityLabel", NamedArgument.op_Implicit(maxEventLogMemories)), -1f, (string)null);
			maxEventLogMemories = (int)listing.Slider((float)maxEventLogMemories, 20f, 100f);
		}

		private void DrawDecaySettings(Listing_Standard listing)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			listing.Label(TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_SCMDecayLabel", NamedArgument.op_Implicit(ScmDecayRate.ToString("P1"))), -1f, (string)null);
			ScmDecayRate = listing.Slider(ScmDecayRate, 0.01f, 0.1f);
			listing.Label(TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_ELSDecayLabel", NamedArgument.op_Implicit(ElsDecayRate.ToString("P1"))), -1f, (string)null);
			ElsDecayRate = listing.Slider(ElsDecayRate, 0.001f, 0.01f);
			listing.Label(TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_CLPADecayLabel", NamedArgument.op_Implicit(ClpaDecayRate.ToString("P1"))), -1f, (string)null);
			ClpaDecayRate = listing.Slider(ClpaDecayRate, 0.0005f, 0.0015f);
			listing.Label(TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_ABMLifespanLabel", NamedArgument.op_Implicit(AbmLifespanHours)), -1f, (string)null);
			AbmLifespanHours = (int)listing.Slider((float)AbmLifespanHours, 12f, 72f);
		}

		private void DrawSummarizationSettings(Listing_Standard listing)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			listing.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_EnableDailySummarization")), ref EnableDailySummarization, (string)null, 0f, 1f);
			if (EnableDailySummarization)
			{
				listing.Label(TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_TriggerTimeLabel", NamedArgument.op_Implicit(summarizationHour)), -1f, (string)null);
				summarizationHour = (int)listing.Slider((float)summarizationHour, 0f, 23f);
			}
			listing.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_EnableAutoArchive")), ref EnableAutoArchive, (string)null, 0f, 1f);
		}

		private void DrawAIConfigSettings(Listing_Standard listing)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			listing.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_EnableAILog")), ref EnableAILog, (string)null, 0f, 1f);
			listing.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_PreferRimTalkAI")), ref UseRimTalkAIConfig, (string)null, 0f, 1f);
			if (UseRimTalkAIConfig)
			{
				GUI.color = new Color(0.8f, 1f, 0.8f);
				listing.Label("  " + Translator.Translate("RimTalk_Settings_WillFollowRimTalkConfig"), -1f, (string)null);
				GUI.color = Color.white;
				((Listing)listing).Gap(12f);
			}
			else
			{
				GUI.color = Color.gray;
				listing.Label("  " + Translator.Translate("RimTalk_Settings_FallbackEnabledDesc"), -1f, (string)null);
				GUI.color = Color.white;
				((Listing)listing).Gap(12f);
			}
			((Listing)listing).GapLine(12f);
			SettingsUIDrawers.DrawRimTalkStyleApiConfigs(listing, this);
			((Listing)listing).Gap(12f);
			if (Current.Game == null)
			{
				GUI.color = Color.gray;
				listing.Label(Translator.Translate("RimTalk_Settings_ValidateInSaveOnly"), -1f, (string)null);
				GUI.color = Color.white;
				return;
			}
			if (Widgets.ButtonText(((Listing)listing).GetRect(35f, 1f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_ValidateConfig")), true, true, true, (TextAnchor?)null))
			{
				ValidateAIConfig();
			}
			GUI.color = Color.gray;
			listing.Label(Translator.Translate("RimTalk_Settings_ValidateConfigTip"), -1f, (string)null);
			GUI.color = Color.white;
		}

		private async void ValidateAIConfig()
		{
			AIService.ResetClientPool();
			Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_Validating")), MessageTypeDefOf.CautionInput, false);
			if (await AIService.ValidateAIConfigAsync())
			{
				Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_ValidationSuccessAny")), MessageTypeDefOf.PositiveEvent, false);
			}
			else
			{
				Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_ValidationFailed")), MessageTypeDefOf.NegativeEvent, false);
			}
		}

		private void DrawMemoryTypesSettings(Listing_Standard listing)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			listing.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_ActionMemory")), ref EnableActionMemory, (string)null, 0f, 1f);
			listing.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_ConversationMemory")), ref enableConversationMemory, (string)null, 0f, 1f);
			listing.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_CombatMemory")), ref EnableCombatMemory, (string)null, 0f, 1f);
		}

		private void DrawExperimentalFeaturesSettings(Listing_Standard listing)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			listing.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_EnableProactiveRecall")), ref enableProactiveRecall, (string)null, 0f, 1f);
			if (enableProactiveRecall)
			{
				listing.Label(TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_TriggerChanceLabel", NamedArgument.op_Implicit(recallTriggerChance.ToString("P0"))), -1f, (string)null);
				recallTriggerChance = listing.Slider(recallTriggerChance, 0.05f, 0.6f);
			}
			((Listing)listing).Gap(12f);
			((Listing)listing).GapLine(12f);
			SettingsUIDrawers.DrawKnowledgeMatchingSourcesSettings(listing, this);
			SettingsUIDrawers.DrawKnowledgeChainingSettings(listing, this);
		}

		private void DrawVectorEnhancementSettings(Listing_Standard listing)
		{
			SettingsUIDrawers.DrawSiliconFlowSettings(listing, this);
		}

		private void OpenCommonKnowledgeDialog()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			if (Current.Game == null)
			{
				Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_MustEnterGame")), MessageTypeDefOf.RejectInput, true);
				return;
			}
			MemoryManager component = Find.World.GetComponent<MemoryManager>();
			if (component == null)
			{
				Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_CannotFindManager")), MessageTypeDefOf.RejectInput, true);
			}
			else
			{
				Find.WindowStack.Add((Window)(object)new Dialog_CommonKnowledge(component.CommonKnowledge));
			}
		}

		private void DrawPromptNormalizationSettings(Listing_Standard listing)
		{
			SettingsUIDrawers.DrawPromptNormalizationSettings(listing, this);
		}

		static RimTalkMemoryPatchSettings()
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			expandDynamicInjection = true;
			expandMemoryCapacity = false;
			expandDecayRates = false;
			expandSummarization = false;
			expandAIConfig = true;
			expandMemoryTypes = false;
			expandVectorEnhancement = true;
			expandExperimentalFeatures = true;
			scrollPosition = Vector2.zero;
		}
	}
}
namespace RimTalk.Memory
{
	public static class CommonKnowledgeAPI
	{
		public static string AddKnowledge(string tag, string content, float importance = 0.5f)
		{
			try
			{
				CommonKnowledgeLibrary commonKnowledge = MemoryManager.GetCommonKnowledge();
				if (commonKnowledge == null)
				{
					Log.Warning("[CommonKnowledgeAPI] Failed to get library");
					return null;
				}
				CommonKnowledgeEntry commonKnowledgeEntry = new CommonKnowledgeEntry(tag, content)
				{
					importance = Mathf.Clamp01(importance),
					isEnabled = true,
					isUserEdited = false
				};
				commonKnowledge.AddEntry(commonKnowledgeEntry);
				return commonKnowledgeEntry.id;
			}
			catch (Exception arg)
			{
				Log.Error($"[CommonKnowledgeAPI] AddKnowledge failed: {arg}");
				return null;
			}
		}

		public static string AddKnowledgeEx(string tag, string content, float importance = 0.5f, KeywordMatchMode matchMode = KeywordMatchMode.Any, int targetPawnId = -1, bool canBeExtracted = false, bool canBeMatched = false)
		{
			try
			{
				CommonKnowledgeLibrary commonKnowledge = MemoryManager.GetCommonKnowledge();
				if (commonKnowledge == null)
				{
					Log.Warning("[CommonKnowledgeAPI] Failed to get library");
					return null;
				}
				CommonKnowledgeEntry commonKnowledgeEntry = new CommonKnowledgeEntry(tag, content)
				{
					importance = Mathf.Clamp01(importance),
					matchMode = matchMode,
					targetPawnId = targetPawnId,
					isEnabled = true,
					isUserEdited = false
				};
				ExtendedKnowledgeEntry.SetCanBeExtracted(commonKnowledgeEntry, canBeExtracted);
				ExtendedKnowledgeEntry.SetCanBeMatched(commonKnowledgeEntry, canBeMatched);
				commonKnowledge.AddEntry(commonKnowledgeEntry);
				return commonKnowledgeEntry.id;
			}
			catch (Exception arg)
			{
				Log.Error($"[CommonKnowledgeAPI] AddKnowledgeEx failed: {arg}");
				return null;
			}
		}

		public static int AddKnowledgeBatch(List<(string tag, string content)> knowledgeList, float importance = 0.5f)
		{
			if (knowledgeList == null || knowledgeList.Count == 0)
			{
				return 0;
			}
			int num = 0;
			foreach (var (tag, content) in knowledgeList)
			{
				if (AddKnowledge(tag, content, importance) != null)
				{
					num++;
				}
			}
			return num;
		}

		public static bool UpdateKnowledge(string id, string newContent)
		{
			try
			{
				CommonKnowledgeEntry commonKnowledgeEntry = FindKnowledgeById(id);
				if (commonKnowledgeEntry == null)
				{
					Log.Warning("[CommonKnowledgeAPI] Knowledge not found: " + id);
					return false;
				}
				commonKnowledgeEntry.content = newContent;
				commonKnowledgeEntry.InvalidateCache();
				try
				{
					if (RimTalkMemoryPatchMod.Settings.enableVectorEnhancement)
					{
						VectorService.Instance.UpdateKnowledgeVector(commonKnowledgeEntry.id, commonKnowledgeEntry.content);
					}
				}
				catch (Exception ex)
				{
					Log.Warning("[CommonKnowledgeAPI] Failed to update vector: " + ex.Message);
				}
				return true;
			}
			catch (Exception arg)
			{
				Log.Error($"[CommonKnowledgeAPI] UpdateKnowledge failed: {arg}");
				return false;
			}
		}

		public static bool UpdateKnowledgeTag(string id, string newTag)
		{
			try
			{
				CommonKnowledgeEntry commonKnowledgeEntry = FindKnowledgeById(id);
				if (commonKnowledgeEntry == null)
				{
					return false;
				}
				commonKnowledgeEntry.tag = newTag;
				commonKnowledgeEntry.InvalidateCache();
				return true;
			}
			catch (Exception arg)
			{
				Log.Error($"[CommonKnowledgeAPI] UpdateKnowledgeTag failed: {arg}");
				return false;
			}
		}

		public static bool UpdateKnowledgeImportance(string id, float newImportance)
		{
			try
			{
				CommonKnowledgeEntry commonKnowledgeEntry = FindKnowledgeById(id);
				if (commonKnowledgeEntry == null)
				{
					return false;
				}
				commonKnowledgeEntry.importance = Mathf.Clamp01(newImportance);
				return true;
			}
			catch (Exception arg)
			{
				Log.Error($"[CommonKnowledgeAPI] UpdateKnowledgeImportance failed: {arg}");
				return false;
			}
		}

		public static bool SetKnowledgeEnabled(string id, bool enabled)
		{
			try
			{
				CommonKnowledgeEntry commonKnowledgeEntry = FindKnowledgeById(id);
				if (commonKnowledgeEntry == null)
				{
					return false;
				}
				commonKnowledgeEntry.isEnabled = enabled;
				return true;
			}
			catch (Exception arg)
			{
				Log.Error($"[CommonKnowledgeAPI] SetKnowledgeEnabled failed: {arg}");
				return false;
			}
		}

		public static CommonKnowledgeEntry FindKnowledgeById(string id)
		{
			try
			{
				CommonKnowledgeLibrary commonKnowledge = MemoryManager.GetCommonKnowledge();
				if (commonKnowledge == null)
				{
					return null;
				}
				return GenCollection.FirstOrDefault<CommonKnowledgeEntry>(commonKnowledge.Entries, (Predicate<CommonKnowledgeEntry>)((CommonKnowledgeEntry e) => e.id == id));
			}
			catch (Exception arg)
			{
				Log.Error($"[CommonKnowledgeAPI] FindKnowledgeById failed: {arg}");
				return null;
			}
		}

		public static List<CommonKnowledgeEntry> FindKnowledge(string tag)
		{
			try
			{
				CommonKnowledgeLibrary commonKnowledge = MemoryManager.GetCommonKnowledge();
				if (commonKnowledge == null)
				{
					return new List<CommonKnowledgeEntry>();
				}
				return commonKnowledge.Entries.Where((CommonKnowledgeEntry e) => e.tag.IndexOf(tag, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
			}
			catch (Exception arg)
			{
				Log.Error($"[CommonKnowledgeAPI] FindKnowledge failed: {arg}");
				return new List<CommonKnowledgeEntry>();
			}
		}

		public static List<CommonKnowledgeEntry> FindKnowledgeByContent(string content)
		{
			try
			{
				CommonKnowledgeLibrary commonKnowledge = MemoryManager.GetCommonKnowledge();
				if (commonKnowledge == null)
				{
					return new List<CommonKnowledgeEntry>();
				}
				return commonKnowledge.Entries.Where((CommonKnowledgeEntry e) => e.content.IndexOf(content, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
			}
			catch (Exception arg)
			{
				Log.Error($"[CommonKnowledgeAPI] FindKnowledgeByContent failed: {arg}");
				return new List<CommonKnowledgeEntry>();
			}
		}

		public static List<CommonKnowledgeEntry> GetAllKnowledge()
		{
			try
			{
				CommonKnowledgeLibrary commonKnowledge = MemoryManager.GetCommonKnowledge();
				if (commonKnowledge == null)
				{
					return new List<CommonKnowledgeEntry>();
				}
				return commonKnowledge.Entries.ToList();
			}
			catch (Exception arg)
			{
				Log.Error($"[CommonKnowledgeAPI] GetAllKnowledge failed: {arg}");
				return new List<CommonKnowledgeEntry>();
			}
		}

		public static int GetKnowledgeCount()
		{
			try
			{
				return MemoryManager.GetCommonKnowledge()?.Entries.Count ?? 0;
			}
			catch (Exception arg)
			{
				Log.Error($"[CommonKnowledgeAPI] GetKnowledgeCount failed: {arg}");
				return 0;
			}
		}

		public static bool RemoveKnowledge(string id)
		{
			try
			{
				CommonKnowledgeLibrary commonKnowledge = MemoryManager.GetCommonKnowledge();
				if (commonKnowledge == null)
				{
					return false;
				}
				CommonKnowledgeEntry commonKnowledgeEntry = GenCollection.FirstOrDefault<CommonKnowledgeEntry>(commonKnowledge.Entries, (Predicate<CommonKnowledgeEntry>)((CommonKnowledgeEntry e) => e.id == id));
				if (commonKnowledgeEntry == null)
				{
					return false;
				}
				commonKnowledge.RemoveEntry(commonKnowledgeEntry);
				return true;
			}
			catch (Exception arg)
			{
				Log.Error($"[CommonKnowledgeAPI] RemoveKnowledge failed: {arg}");
				return false;
			}
		}

		public static int RemoveKnowledgeByTag(string tag)
		{
			try
			{
				CommonKnowledgeLibrary commonKnowledge = MemoryManager.GetCommonKnowledge();
				if (commonKnowledge == null)
				{
					return 0;
				}
				List<CommonKnowledgeEntry> list = commonKnowledge.Entries.Where((CommonKnowledgeEntry e) => e.tag.IndexOf(tag, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
				int num = 0;
				foreach (CommonKnowledgeEntry item in list)
				{
					commonKnowledge.RemoveEntry(item);
					num++;
				}
				return num;
			}
			catch (Exception arg)
			{
				Log.Error($"[CommonKnowledgeAPI] RemoveKnowledgeByTag failed: {arg}");
				return 0;
			}
		}

		public static bool ClearAllKnowledge()
		{
			try
			{
				CommonKnowledgeLibrary commonKnowledge = MemoryManager.GetCommonKnowledge();
				if (commonKnowledge == null)
				{
					return false;
				}
				commonKnowledge.Clear();
				return true;
			}
			catch (Exception arg)
			{
				Log.Error($"[CommonKnowledgeAPI] ClearAllKnowledge failed: {arg}");
				return false;
			}
		}

		public static int ImportFromText(string text, bool clearExisting = false)
		{
			try
			{
				return MemoryManager.GetCommonKnowledge()?.ImportFromText(text, clearExisting) ?? 0;
			}
			catch (Exception arg)
			{
				Log.Error($"[CommonKnowledgeAPI] ImportFromText failed: {arg}");
				return 0;
			}
		}

		public static string ExportToText()
		{
			try
			{
				CommonKnowledgeLibrary commonKnowledge = MemoryManager.GetCommonKnowledge();
				if (commonKnowledge == null)
				{
					return string.Empty;
				}
				return commonKnowledge.ExportToText();
			}
			catch (Exception arg)
			{
				Log.Error($"[CommonKnowledgeAPI] ExportToText failed: {arg}");
				return string.Empty;
			}
		}

		public static bool ExistsKnowledge(string id)
		{
			return FindKnowledgeById(id) != null;
		}

		public static KnowledgeStats GetStats()
		{
			try
			{
				CommonKnowledgeLibrary commonKnowledge = MemoryManager.GetCommonKnowledge();
				if (commonKnowledge == null)
				{
					return default(KnowledgeStats);
				}
				return new KnowledgeStats
				{
					TotalCount = commonKnowledge.Entries.Count,
					EnabledCount = GenCollection.Count<CommonKnowledgeEntry>(commonKnowledge.Entries, (Predicate<CommonKnowledgeEntry>)((CommonKnowledgeEntry e) => e.isEnabled)),
					DisabledCount = GenCollection.Count<CommonKnowledgeEntry>(commonKnowledge.Entries, (Predicate<CommonKnowledgeEntry>)((CommonKnowledgeEntry e) => !e.isEnabled)),
					UserEditedCount = GenCollection.Count<CommonKnowledgeEntry>(commonKnowledge.Entries, (Predicate<CommonKnowledgeEntry>)((CommonKnowledgeEntry e) => e.isUserEdited)),
					GlobalCount = GenCollection.Count<CommonKnowledgeEntry>(commonKnowledge.Entries, (Predicate<CommonKnowledgeEntry>)((CommonKnowledgeEntry e) => e.targetPawnId == -1)),
					PawnSpecificCount = GenCollection.Count<CommonKnowledgeEntry>(commonKnowledge.Entries, (Predicate<CommonKnowledgeEntry>)((CommonKnowledgeEntry e) => e.targetPawnId != -1))
				};
			}
			catch (Exception arg)
			{
				Log.Error($"[CommonKnowledgeAPI] GetStats failed: {arg}");
				return default(KnowledgeStats);
			}
		}
	}
	public struct KnowledgeStats
	{
		public int TotalCount;

		public int EnabledCount;

		public int DisabledCount;

		public int UserEditedCount;

		public int GlobalCount;

		public int PawnSpecificCount;
	}
	public static class CommonKnowledgeEntryExtensions
	{
		public static bool IsCompleteWordMatch(string text, string word)
		{
			if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(word))
			{
				return false;
			}
			if (string.Equals(text, word, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			int startIndex = 0;
			while ((startIndex = text.IndexOf(word, startIndex, StringComparison.OrdinalIgnoreCase)) >= 0)
			{
				bool num = startIndex == 0 || IsWordBoundary(text[startIndex - 1]);
				int num2 = startIndex + word.Length;
				bool flag = num2 == text.Length || IsWordBoundary(text[num2]);
				if (num & flag)
				{
					return true;
				}
				startIndex += word.Length;
			}
			return false;
		}

		public static bool IsWordBoundary(char c)
		{
			if (char.IsWhiteSpace(c) || c == ',' || c == '，' || c == '、' || c == ';' || c == '；' || c == '.' || c == '。' || c == '!' || c == '！' || c == '?' || c == '？' || c == ':' || c == '：' || c == '-' || c == '_' || c == '/' || c == '\\' || c == '(' || c == ')' || c == '（' || c == '）' || c == '[' || c == ']' || c == '{' || c == '}' || c == '<' || c == '>' || c == '「' || c == '」' || c == '『' || c == '』' || c == '【' || c == '】')
			{
				return true;
			}
			if (char.IsPunctuation(c) || char.IsSymbol(c))
			{
				return true;
			}
			return false;
		}

		public static bool IsRuleKnowledge(this CommonKnowledgeEntry entry)
		{
			if (entry == null || string.IsNullOrEmpty(entry.tag))
			{
				return false;
			}
			string text = entry.tag.ToLower();
			if (!text.Contains("规则") && !text.Contains("instructions"))
			{
				return text.Contains("rule");
			}
			return true;
		}
	}
	public class CommonKnowledgeLibrary : IExposable
	{
		private List<CommonKnowledgeEntry> entries = new List<CommonKnowledgeEntry>();

		private List<string> vectorIds;

		private List<string> vectorDataSerialized;

		private List<string> vectorHashes;

		public List<CommonKnowledgeEntry> Entries => entries;

		public void ExposeData()
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Invalid comparison between Unknown and I4
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Invalid comparison between Unknown and I4
			Scribe_Collections.Look<CommonKnowledgeEntry>(ref entries, "commonKnowledge", (LookMode)2, Array.Empty<object>());
			ExtendedKnowledgeEntry.ExposeData();
			if ((int)Scribe.mode == 1 && RimTalkMemoryPatchMod.Settings.enableVectorEnhancement)
			{
				try
				{
					VectorService.Instance.ExportVectorsForSave(out vectorIds, out var vectors, out vectorHashes);
					vectorDataSerialized = new List<string>();
					if (vectors != null)
					{
						foreach (List<float> item in vectors)
						{
							if (item != null)
							{
								vectorDataSerialized.Add(string.Join(",", item));
							}
							else
							{
								vectorDataSerialized.Add("");
							}
						}
					}
				}
				catch (Exception arg)
				{
					Log.Error($"[RimTalk-ExpandMemory] Failed to export vectors for save: {arg}");
					vectorIds = null;
					vectorDataSerialized = null;
					vectorHashes = null;
				}
			}
			Scribe_Collections.Look<string>(ref vectorIds, "vectorIds", (LookMode)1, Array.Empty<object>());
			Scribe_Collections.Look<string>(ref vectorDataSerialized, "vectorDataSerialized", (LookMode)1, Array.Empty<object>());
			Scribe_Collections.Look<string>(ref vectorHashes, "vectorHashes", (LookMode)1, Array.Empty<object>());
			if ((int)Scribe.mode != 4)
			{
				return;
			}
			if (entries == null)
			{
				entries = new List<CommonKnowledgeEntry>();
			}
			if (!RimTalkMemoryPatchMod.Settings.enableVectorEnhancement)
			{
				return;
			}
			try
			{
				if (vectorIds != null && vectorDataSerialized != null && vectorHashes != null && vectorIds.Count > 0)
				{
					Log.Message($"[RimTalk-ExpandMemory] Restoring {vectorIds.Count} vectors from save...");
					List<List<float>> list = new List<List<float>>();
					foreach (string item2 in vectorDataSerialized)
					{
						if (!string.IsNullOrEmpty(item2))
						{
							List<float> list2 = new List<float>();
							string[] array = item2.Split(',', StringSplitOptions.None);
							for (int i = 0; i < array.Length; i++)
							{
								if (float.TryParse(array[i], out var result))
								{
									list2.Add(result);
								}
							}
							list.Add(list2);
						}
						else
						{
							list.Add(new List<float>());
						}
					}
					VectorService.Instance.ImportVectorsFromLoad(vectorIds, list, vectorHashes);
				}
				else
				{
					Log.Message("[RimTalk-ExpandMemory] No saved vectors found, will perform full sync.");
				}
				Log.Message("[RimTalk-ExpandMemory] Syncing knowledge library to vector database...");
				VectorService.Instance.SyncKnowledgeLibrary(this);
			}
			catch (Exception arg2)
			{
				Log.Error($"[RimTalk-ExpandMemory] Failed to restore/sync vectors on game load: {arg2}");
			}
		}

		public void AddEntry(CommonKnowledgeEntry entry)
		{
			if (entry == null || entries.Contains(entry))
			{
				return;
			}
			entries.Add(entry);
			if (!RimTalkMemoryPatchMod.Settings.enableVectorEnhancement)
			{
				return;
			}
			try
			{
				if (entry.isEnabled)
				{
					VectorService.Instance.UpdateKnowledgeVector(entry.id, entry.content);
				}
			}
			catch (Exception ex)
			{
				Log.Warning("[RimTalk-ExpandMemory] Failed to sync vector on AddEntry: " + ex.Message);
			}
		}

		public void AddEntry(string tag, string content)
		{
			CommonKnowledgeEntry entry = new CommonKnowledgeEntry(tag, content);
			AddEntry(entry);
		}

		public void RemoveEntry(CommonKnowledgeEntry entry)
		{
			if (entry == null)
			{
				return;
			}
			entries.Remove(entry);
			if (RimTalkMemoryPatchMod.Settings.enableVectorEnhancement)
			{
				try
				{
					VectorService.Instance.RemoveKnowledgeVector(entry.id);
				}
				catch (Exception ex)
				{
					Log.Warning("[RimTalk-ExpandMemory] Failed to remove vector on RemoveEntry: " + ex.Message);
				}
			}
			ExtendedKnowledgeEntry.CleanupDeletedEntries(this);
		}

		public void Clear()
		{
			entries.Clear();
			if (RimTalkMemoryPatchMod.Settings.enableVectorEnhancement)
			{
				try
				{
					VectorService.Instance.SyncKnowledgeLibrary(this);
				}
				catch (Exception ex)
				{
					Log.Warning("[RimTalk-ExpandMemory] Failed to clear vectors on Clear: " + ex.Message);
				}
			}
			ExtendedKnowledgeEntry.CleanupDeletedEntries(this);
		}

		public int ImportFromText(string text, bool clearExisting = false)
		{
			if (string.IsNullOrEmpty(text))
			{
				return 0;
			}
			if (clearExisting)
			{
				entries.Clear();
			}
			int num = 0;
			string[] array = text.Split(new char[2] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				string text2 = array[i].Trim();
				if (!string.IsNullOrEmpty(text2))
				{
					CommonKnowledgeEntry commonKnowledgeEntry = ParseLine(text2);
					if (commonKnowledgeEntry != null)
					{
						entries.Add(commonKnowledgeEntry);
						num++;
					}
				}
			}
			if (RimTalkMemoryPatchMod.Settings.enableVectorEnhancement)
			{
				try
				{
					VectorService.Instance.SyncKnowledgeLibrary(this);
				}
				catch (Exception ex)
				{
					Log.Warning("[RimTalk-ExpandMemory] Failed to sync vectors on ImportFromText: " + ex.Message);
				}
			}
			return num;
		}

		private CommonKnowledgeEntry ParseLine(string line)
		{
			if (string.IsNullOrEmpty(line))
			{
				return null;
			}
			int num = line.IndexOf('[');
			int num2 = -1;
			if (num >= 0)
			{
				num2 = line.IndexOf(']', num + 1);
				if (num2 == -1)
				{
					int num3 = line.IndexOf('}', num + 1);
					if (num3 > num)
					{
						num2 = num3;
						Log.Warning("[CommonKnowledge] 检测到错误的标签格式（使用了花括号）: " + line.Substring(0, Math.Min(50, line.Length)));
					}
				}
			}
			if (num == -1 || num2 == -1 || num2 <= num)
			{
				return new CommonKnowledgeEntry("通用", line)
				{
					importance = 0.5f
				};
			}
			string text = line.Substring(num + 1, num2 - num - 1).Trim();
			string text2 = line.Substring(num2 + 1).Trim();
			if (string.IsNullOrEmpty(text2))
			{
				return null;
			}
			string[] array = text.Split('|', StringSplitOptions.None);
			string tag = ((array.Length != 0) ? array[0].Trim() : "通用");
			float result = 0.5f;
			KeywordMatchMode result2 = KeywordMatchMode.Any;
			bool result3 = false;
			bool result4 = false;
			if (array.Length > 1)
			{
				string text3 = array[1].Trim();
				if (!float.TryParse(text3, out result))
				{
					result = 0.5f;
					Log.Warning("[CommonKnowledge] Failed to parse importance '" + text3 + "' in line: " + line.Substring(0, Math.Min(50, line.Length)));
				}
				result = Math.Max(0f, Math.Min(1f, result));
			}
			if (array.Length > 2)
			{
				string text4 = array[2].Trim();
				if (!Enum.TryParse<KeywordMatchMode>(text4, ignoreCase: true, out result2))
				{
					result2 = KeywordMatchMode.Any;
					Log.Warning("[CommonKnowledge] Failed to parse matchMode '" + text4 + "', using default 'Any'");
				}
			}
			if (array.Length > 3 && !bool.TryParse(array[3].Trim(), out result3))
			{
				result3 = false;
			}
			if (array.Length > 4 && !bool.TryParse(array[4].Trim(), out result4))
			{
				result4 = false;
			}
			CommonKnowledgeEntry obj = new CommonKnowledgeEntry(tag, text2)
			{
				importance = result,
				matchMode = result2
			};
			ExtendedKnowledgeEntry.SetCanBeExtracted(obj, result3);
			ExtendedKnowledgeEntry.SetCanBeMatched(obj, result4);
			return obj;
		}

		public string ExportToText()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (CommonKnowledgeEntry entry in entries)
			{
				if (entry != null)
				{
					stringBuilder.AppendLine(entry.FormatForExport());
				}
			}
			return stringBuilder.ToString();
		}

		public string InjectKnowledge(string context, int maxEntries = 5)
		{
			List<KnowledgeScore> scores;
			return InjectKnowledgeWithDetails(context, maxEntries, out scores);
		}

		public string InjectKnowledgeWithDetails(string context, int maxEntries, out List<KnowledgeScore> scores, Pawn currentPawn = null, Pawn targetPawn = null)
		{
			List<KnowledgeScoreDetail> allScores;
			KeywordExtractionInfo keywordInfo;
			return InjectKnowledgeWithDetails(context, maxEntries, out scores, out allScores, out keywordInfo, currentPawn, targetPawn);
		}

		public string InjectKnowledgeWithDetails(string context, int maxEntries, out List<KnowledgeScore> scores, out KeywordExtractionInfo keywordInfo, Pawn currentPawn = null, Pawn targetPawn = null)
		{
			List<KnowledgeScoreDetail> allScores;
			return InjectKnowledgeWithDetails(context, maxEntries, out scores, out allScores, out keywordInfo, currentPawn, targetPawn);
		}

		public string InjectKnowledgeWithDetails(string context, int maxEntries, out List<KnowledgeScore> scores, out List<KnowledgeScoreDetail> allScores, out KeywordExtractionInfo keywordInfo, Pawn currentPawn = null, Pawn targetPawn = null)
		{
			scores = new List<KnowledgeScore>();
			allScores = new List<KnowledgeScoreDetail>();
			keywordInfo = new KeywordExtractionInfo();
			RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(context);
			if (currentPawn != null)
			{
				stringBuilder.Append(" ");
				stringBuilder.Append(BuildCompletePawnInfoText(currentPawn));
				PawnKeywordInfo pawnKeywordInfo = KeywordExtractionHelper.ExtractPawnKeywords(new List<string>(), currentPawn);
				keywordInfo.PawnInfo = pawnKeywordInfo;
				keywordInfo.PawnKeywordsCount = pawnKeywordInfo.TotalCount;
			}
			if (targetPawn != null && targetPawn != currentPawn)
			{
				stringBuilder.Append(" ");
				stringBuilder.Append(BuildCompletePawnInfoText(targetPawn));
			}
			string text = stringBuilder.ToString();
			string text2 = text;
			keywordInfo.ContextKeywords = new List<string> { context };
			keywordInfo.TotalKeywords = 1;
			HashSet<CommonKnowledgeEntry> hashSet = new HashSet<CommonKnowledgeEntry>();
			int num = ((!settings.enableKnowledgeChaining) ? 1 : settings.maxChainingRounds);
			for (int i = 0; i < num; i++)
			{
				if (string.IsNullOrEmpty(text2))
				{
					break;
				}
				bool isChaining = i > 0;
				string matchText = ((i == 0) ? text : text2);
				List<CommonKnowledgeEntry> list = MatchKnowledgeByTags(matchText, currentPawn, hashSet, isChaining);
				if (list.Count == 0)
				{
					break;
				}
				foreach (CommonKnowledgeEntry item in list)
				{
					hashSet.Add(item);
				}
				if (!settings.enableKnowledgeChaining || i >= num - 1)
				{
					break;
				}
				text2 = BuildMatchTextFromKnowledge(list);
			}
			List<KnowledgeScore> scoredEntries = new List<KnowledgeScore>();
			foreach (CommonKnowledgeEntry item2 in hashSet)
			{
				KnowledgeMatchType matchType = KnowledgeMatchType.Keyword;
				float num2 = 0.5f;
				float num3 = num2 + item2.importance;
				allScores.Add(new KnowledgeScoreDetail
				{
					Entry = item2,
					IsEnabled = item2.isEnabled,
					TotalScore = num3,
					BaseScore = item2.importance,
					ManualBonus = 0f,
					MatchTypeScore = num2,
					MatchType = matchType,
					MatchedTags = item2.GetTags(),
					FailReason = "Pending"
				});
				scoredEntries.Add(new KnowledgeScore
				{
					Entry = item2,
					Score = num3
				});
			}
			scoredEntries.Sort((KnowledgeScore a, KnowledgeScore b) => b.Score.CompareTo(a.Score));
			int i2;
			for (i2 = 0; i2 < scoredEntries.Count; i2++)
			{
				KnowledgeScoreDetail knowledgeScoreDetail = GenCollection.FirstOrDefault<KnowledgeScoreDetail>(allScores, (Predicate<KnowledgeScoreDetail>)((KnowledgeScoreDetail d) => d.Entry == scoredEntries[i2].Entry));
				if (knowledgeScoreDetail != null)
				{
					if (i2 < maxEntries)
					{
						knowledgeScoreDetail.FailReason = "Selected";
						scores.Add(scoredEntries[i2]);
					}
					else
					{
						knowledgeScoreDetail.FailReason = "ExceedMaxEntries";
					}
				}
			}
			List<CommonKnowledgeEntry> list2 = scores.Select((KnowledgeScore s) => s.Entry).ToList();
			if (list2.Count == 0)
			{
				return null;
			}
			StringBuilder stringBuilder2 = new StringBuilder();
			int num4 = 1;
			foreach (CommonKnowledgeEntry item3 in list2)
			{
				stringBuilder2.AppendLine($"{num4}. [{item3.tag}] {item3.content}");
				num4++;
			}
			return stringBuilder2.ToString();
		}

		private List<CommonKnowledgeEntry> MatchKnowledgeByTags(string matchText, Pawn currentPawn, HashSet<CommonKnowledgeEntry> alreadyMatched, bool isChaining = false)
		{
			List<CommonKnowledgeEntry> list = new List<CommonKnowledgeEntry>();
			if (string.IsNullOrEmpty(matchText))
			{
				return list;
			}
			foreach (CommonKnowledgeEntry entry in entries)
			{
				if (!alreadyMatched.Contains(entry) && entry.isEnabled && (!isChaining || ExtendedKnowledgeEntry.CanBeMatched(entry)) && (entry.targetPawnId == -1 || (currentPawn != null && entry.targetPawnId == ((Thing)currentPawn).thingIDNumber)) && IsMatched(matchText, entry))
				{
					list.Add(entry);
				}
			}
			return list;
		}

		private bool IsMatched(string text, CommonKnowledgeEntry entry)
		{
			List<string> tags = entry.GetTags();
			if (tags == null || tags.Count == 0)
			{
				return false;
			}
			switch (entry.matchMode)
			{
			case KeywordMatchMode.Any:
				foreach (string item in tags)
				{
					if (!string.IsNullOrWhiteSpace(item) && text.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return true;
					}
				}
				return false;
			case KeywordMatchMode.All:
				foreach (string item2 in tags)
				{
					if (!string.IsNullOrWhiteSpace(item2) && text.IndexOf(item2, StringComparison.OrdinalIgnoreCase) < 0)
					{
						return false;
					}
				}
				return true;
			default:
				return false;
			}
		}

		private string BuildMatchTextFromKnowledge(List<CommonKnowledgeEntry> entries)
		{
			if (entries == null || entries.Count == 0)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (CommonKnowledgeEntry entry in entries)
			{
				if (ExtendedKnowledgeEntry.CanBeExtracted(entry) && !string.IsNullOrEmpty(entry.content))
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append(" ");
					}
					stringBuilder.Append(entry.content);
				}
			}
			return stringBuilder.ToString();
		}

		private string BuildCompletePawnInfoText(Pawn pawn)
		{
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_043a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0484: Unknown result type (might be due to invalid IL or missing references)
			if (pawn == null)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				Name name = pawn.Name;
				if (!string.IsNullOrEmpty((name != null) ? name.ToStringShort : null))
				{
					stringBuilder.Append(pawn.Name.ToStringShort);
					stringBuilder.Append(" ");
				}
				if (pawn.RaceProps != null && pawn.RaceProps.Humanlike)
				{
					float ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
					if (ageBiologicalYearsFloat < 3f)
					{
						stringBuilder.Append("婴儿 宝宝 ");
					}
					else if (ageBiologicalYearsFloat < 13f)
					{
						stringBuilder.Append("儿童 小孩 ");
					}
					else if (ageBiologicalYearsFloat < 18f)
					{
						stringBuilder.Append("青少年 ");
					}
					else
					{
						stringBuilder.Append("成人 ");
					}
				}
				stringBuilder.Append(GenderUtility.GetLabel(pawn.gender, false));
				stringBuilder.Append(" ");
				if (((Thing)pawn).def != null)
				{
					stringBuilder.Append(((Def)((Thing)pawn).def).label);
					stringBuilder.Append(" ");
					try
					{
						if (pawn.genes != null && pawn.genes.Xenotype != null)
						{
							string value = ((Def)pawn.genes.Xenotype).label ?? ((Def)pawn.genes.Xenotype).defName;
							if (!string.IsNullOrEmpty(value))
							{
								stringBuilder.Append(value);
								stringBuilder.Append(" ");
							}
						}
					}
					catch
					{
					}
				}
				if (pawn.IsColonist)
				{
					stringBuilder.Append("殖民者 ");
				}
				else if (pawn.IsPrisoner)
				{
					stringBuilder.Append("囚犯 ");
				}
				else if (pawn.IsSlaveOfColony)
				{
					stringBuilder.Append("奴隶 ");
				}
				else if (pawn.HostFaction == Faction.OfPlayer)
				{
					stringBuilder.Append("访客 ");
				}
				else if (((Thing)pawn).Faction != null && ((Thing)pawn).Faction != Faction.OfPlayer)
				{
					stringBuilder.Append(((Thing)pawn).Faction.Name);
					stringBuilder.Append(" ");
				}
				if (pawn.story?.traits != null)
				{
					foreach (Trait allTrait in pawn.story.traits.allTraits)
					{
						if (allTrait != null && ((Def)(allTrait.def?)).label != null)
						{
							stringBuilder.Append(((Def)allTrait.def).label);
							stringBuilder.Append(" ");
						}
					}
				}
				if (pawn.skills != null)
				{
					foreach (SkillRecord skill in pawn.skills.skills)
					{
						if (skill.TotallyDisabled || ((Def)(skill.def?)).label == null)
						{
							continue;
						}
						int level = skill.Level;
						if (level >= 5)
						{
							stringBuilder.Append(((Def)skill.def).label);
							stringBuilder.Append(level);
							stringBuilder.Append(" ");
							if (level >= 15)
							{
								stringBuilder.Append(((Def)skill.def).label);
								stringBuilder.Append("精通 ");
							}
							else if (level >= 10)
							{
								stringBuilder.Append(((Def)skill.def).label);
								stringBuilder.Append("熟练 ");
							}
						}
					}
				}
				if (pawn.health != null)
				{
					if (GenCollection.Any<BodyPartRecord>(pawn.health.hediffSet.GetInjuredParts()))
					{
						stringBuilder.Append("受伤 ");
					}
					else if (!pawn.health.HasHediffsNeedingTend(false))
					{
						stringBuilder.Append("健康 ");
					}
				}
				if (pawn.relations != null)
				{
					foreach (Pawn item in pawn.relations.RelatedPawns.Take(5))
					{
						Name name2 = item.Name;
						if (!string.IsNullOrEmpty((name2 != null) ? name2.ToStringShort : null))
						{
							stringBuilder.Append(item.Name.ToStringShort);
							stringBuilder.Append(" ");
						}
					}
				}
				Pawn_StoryTracker story = pawn.story;
				if (((story != null) ? story.Adulthood : null) != null)
				{
					string value2 = pawn.story.Adulthood.TitleFor(pawn.gender);
					if (!string.IsNullOrEmpty(value2))
					{
						stringBuilder.Append(value2);
						stringBuilder.Append(" ");
					}
				}
				Pawn_StoryTracker story2 = pawn.story;
				if (((story2 != null) ? story2.Childhood : null) != null)
				{
					string value3 = pawn.story.Childhood.TitleFor(pawn.gender);
					if (!string.IsNullOrEmpty(value3))
					{
						stringBuilder.Append(value3);
						stringBuilder.Append(" ");
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warning("[RimTalk-ExpandMemory] Error building complete pawn info text: " + ex.Message);
			}
			return stringBuilder.ToString().Trim();
		}
	}
	public static class EventRecordKnowledgeGenerator
	{
		private static HashSet<int> processedLogIDs = new HashSet<int>();

		private static readonly Dictionary<string, float> ImportantKeywords = new Dictionary<string, float>
		{
			{ "死亡", 1f },
			{ "倒下", 1f },
			{ "被杀", 1f },
			{ "击杀", 1f },
			{ "牺牲", 1f },
			{ "died", 1f },
			{ "killed", 1f },
			{ "death", 1f },
			{ "dead", 1f },
			{ "袭击", 0.9f },
			{ "进攻", 0.9f },
			{ "防御", 0.9f },
			{ "raid", 0.9f },
			{ "attack", 0.9f },
			{ "击退", 0.85f },
			{ "战胜", 0.85f },
			{ "defeated", 0.85f },
			{ "葬礼", 0.9f },
			{ "葬", 0.9f },
			{ "埋葬", 0.9f },
			{ "funeral", 0.9f },
			{ "burial", 0.9f },
			{ "举行葬礼", 0.9f },
			{ "安葬", 0.9f },
			{ "结婚", 0.85f },
			{ "订婚", 0.85f },
			{ "married", 0.85f },
			{ "engaged", 0.85f },
			{ "婚礼", 0.85f },
			{ "wedding", 0.85f },
			{ "举行婚礼", 0.85f },
			{ "分手", 0.75f },
			{ "离婚", 0.75f },
			{ "breakup", 0.75f },
			{ "生日", 0.7f },
			{ "birthday", 0.7f },
			{ "庆祝", 0.6f },
			{ "celebration", 0.6f },
			{ "过生日", 0.7f },
			{ "庆祝生日", 0.7f },
			{ "突破", 0.8f },
			{ "breakthrough", 0.8f },
			{ "完成研究", 0.8f },
			{ "research complete", 0.8f },
			{ "研究完成", 0.8f },
			{ "发明", 0.8f },
			{ "invention", 0.8f },
			{ "周年", 0.7f },
			{ "anniversary", 0.7f },
			{ "周年纪念", 0.7f },
			{ "加入", 0.8f },
			{ "逃跑", 0.8f },
			{ "离开", 0.8f },
			{ "joined", 0.8f },
			{ "fled", 0.8f },
			{ "招募", 0.75f },
			{ "recruited", 0.75f },
			{ "新成员", 0.8f },
			{ "爆炸", 0.85f },
			{ "烟雾", 0.85f },
			{ "火灾", 0.85f },
			{ "explosion", 0.85f },
			{ "fire", 0.85f },
			{ "毒船", 0.85f },
			{ "龙卷风", 0.85f },
			{ "tornado", 0.85f },
			{ "疾病", 0.85f },
			{ "饥荒", 0.8f },
			{ "饿死", 0.8f },
			{ "starvation", 0.8f },
			{ "日食", 0.75f },
			{ "eclipse", 0.75f },
			{ "虫族", 0.85f },
			{ "infestation", 0.85f },
			{ "贸易", 0.6f },
			{ "caravan", 0.6f },
			{ "visitor", 0.6f },
			{ "任务", 0.65f },
			{ "quest", 0.65f }
		};

		public static void ScanRecentPlayLog()
		{
			if (!RimTalkMemoryPatchMod.Settings.enableEventRecordKnowledge)
			{
				return;
			}
			try
			{
				PlayLog playLog = Find.PlayLog;
				if (playLog == null || playLog.AllEntries == null)
				{
					return;
				}
				CommonKnowledgeLibrary commonKnowledge = MemoryManager.GetCommonKnowledge();
				if (commonKnowledge == null)
				{
					return;
				}
				UpdateEventKnowledgeTimePrefix(commonKnowledge);
				int num = 0;
				int ticksGame = Find.TickManager.TicksGame;
				int num2 = ticksGame - 2500;
				List<LogEntry> allEntries = playLog.AllEntries;
				int count = allEntries.Count;
				int num3 = 0;
				int num4 = Math.Min(50, count);
				int num5 = count - 1;
				while (num5 >= 0 && num3 < num4)
				{
					try
					{
						LogEntry val = allEntries[num5];
						if (val != null)
						{
							num3++;
							if (val.Age < num2)
							{
								break;
							}
							int hashCode = ((object)val).GetHashCode();
							if (!processedLogIDs.Contains(hashCode))
							{
								processedLogIDs.Add(hashCode);
								if (processedLogIDs.Count > 2000)
								{
									foreach (int item in processedLogIDs.Take(1000).ToList())
									{
										processedLogIDs.Remove(item);
									}
								}
								string eventText = ExtractEventInfo(val);
								if (!string.IsNullOrEmpty(eventText) && !GenCollection.Any<CommonKnowledgeEntry>(commonKnowledge.Entries, (Predicate<CommonKnowledgeEntry>)((CommonKnowledgeEntry e) => e.content.Contains(eventText.Substring(0, Math.Min(15, eventText.Length))))))
								{
									float importance = CalculateImportance(eventText);
									string originalEventText = ExtractOriginalEventText(eventText);
									CommonKnowledgeEntry entry = new CommonKnowledgeEntry("事件,历史", eventText)
									{
										importance = importance,
										isEnabled = true,
										isUserEdited = false,
										creationTick = ticksGame,
										originalEventText = originalEventText
									};
									commonKnowledge.AddEntry(entry);
									num++;
								}
							}
						}
					}
					catch (Exception ex)
					{
						if (Prefs.DevMode && Random.value < 0.2f)
						{
							Log.Warning("[EventRecord] Error processing log entry: " + ex.Message);
						}
					}
					num5--;
				}
			}
			catch (Exception ex2)
			{
				if (Prefs.DevMode && Random.value < 0.2f)
				{
					Log.Error("[EventRecord] Error scanning PlayLog: " + ex2.Message);
				}
			}
		}

		private static string ExtractEventInfo(LogEntry logEntry)
		{
			if (logEntry == null)
			{
				return null;
			}
			try
			{
				if (((object)logEntry).GetType().Name == "PlayLogEntry_Interaction")
				{
					return null;
				}
				if (((object)logEntry).GetType().Name == "PlayLogEntry_Incident")
				{
					string text = logEntry.ToGameStringFromPOV((Thing)null, false);
					if (string.IsNullOrEmpty(text))
					{
						return null;
					}
					if (!text.Contains("死亡") && !text.Contains("died") && !text.Contains("killed") && !text.Contains("dead") && !text.Contains("death") && !text.Contains("葬礼") && !text.Contains("葬") && !text.Contains("埋葬") && !text.Contains("结婚") && !text.Contains("婚礼") && !text.Contains("married") && !text.Contains("生日") && !text.Contains("birthday") && !text.Contains("突破") && !text.Contains("breakthrough"))
					{
						return null;
					}
				}
				string text2 = logEntry.ToGameStringFromPOV((Thing)null, false);
				if (string.IsNullOrEmpty(text2))
				{
					return null;
				}
				if (text2.Length < 10 || text2.Length > 200)
				{
					return null;
				}
				if (IsBoringMessage(text2))
				{
					return null;
				}
				if (!ImportantKeywords.Keys.Any((string k) => text2.Contains(k)) && !(((object)logEntry).GetType().Name == "PlayLogEntry_Incident"))
				{
					return null;
				}
				if (IsConversationContent(text2))
				{
					return null;
				}
				string text3 = ExtractKeyInformation(text2);
				if (string.IsNullOrEmpty(text3))
				{
					return null;
				}
				int num = (Find.TickManager.TicksGame - logEntry.Age) / 60000;
				string text4 = "";
				if (num < 1)
				{
					text4 = "今天";
				}
				else if (num < 3)
				{
					text4 = $"{num}天前";
				}
				else
				{
					if (num >= 7)
					{
						return null;
					}
					text4 = $"约{num}天前";
				}
				return text4 + text3;
			}
			catch (Exception)
			{
				return null;
			}
		}

		private static string ExtractKeyInformation(string fullText)
		{
			if (string.IsNullOrEmpty(fullText))
			{
				return null;
			}
			try
			{
				fullText = CleanBillText(fullText);
				string text = ExtractMainPerson(fullText);
				string text2 = ExtractMainAction(fullText);
				string text3 = ExtractTarget(fullText, text);
				string text4 = ExtractQuantity(fullText);
				List<string> list = new List<string>();
				if (!string.IsNullOrEmpty(text))
				{
					list.Add(text);
				}
				if (!string.IsNullOrEmpty(text2))
				{
					list.Add(text2);
				}
				if (!string.IsNullOrEmpty(text3))
				{
					list.Add(text3);
				}
				if (!string.IsNullOrEmpty(text4))
				{
					list.Add(text4);
				}
				if (list.Count < 2)
				{
					return (fullText.Length > 40) ? fullText.Substring(0, 40) : fullText;
				}
				return string.Join("", list);
			}
			catch (Exception ex)
			{
				if (Prefs.DevMode)
				{
					Log.Warning("[EventRecord] Key info extraction failed: " + ex.Message);
				}
				return (fullText.Length > 40) ? fullText.Substring(0, 40) : fullText;
			}
		}

		private static string CleanBillText(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}
			try
			{
				Match match = Regex.Match(text, "完成(target\\.[A-Za-z0-9]+|.+?)清单[－\\-](.+?)(?:[，。、\\s]|$)", RegexOptions.IgnoreCase);
				if (match.Success)
				{
					string input = match.Groups[2].Value.Trim();
					input = Regex.Replace(input, "[（\\(].*?[）\\)]", "").Trim();
					string[] array = text.Split(new char[3] { '完', '成', ' ' }, StringSplitOptions.RemoveEmptyEntries);
					string text2 = ((array.Length != 0) ? array[0].Trim() : "");
					if (!string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(input))
					{
						return text2 + "在" + input + "制造物品";
					}
					if (!string.IsNullOrEmpty(input))
					{
						return "在" + input + "制造物品";
					}
				}
				text = Regex.Replace(text, "target\\.[A-Za-z0-9]+", "", RegexOptions.IgnoreCase);
				return text.Trim();
			}
			catch (Exception ex)
			{
				if (Prefs.DevMode)
				{
					Log.Warning("[EventRecord] Bill text cleaning failed: " + ex.Message);
				}
				return text;
			}
		}

		private static string ExtractMainPerson(string text)
		{
			string[] array = new string[9] { "在", "的", "将", "被", "向", "与", "和", "对", " " };
			foreach (string value in array)
			{
				int num = text.IndexOf(value);
				if (num > 0 && num < 15)
				{
					string text2 = text.Substring(0, num).Trim();
					if (text2.Length >= 2 && text2.Length <= 8 && !text2.Any((char c) => char.IsPunctuation(c) || char.IsDigit(c)))
					{
						return text2;
					}
				}
			}
			if (text.Length >= 2)
			{
				string text3 = text.Substring(0, Math.Min(5, text.Length)).Trim();
				if (!text3.Any((char c) => char.IsPunctuation(c)))
				{
					return text3;
				}
			}
			return null;
		}

		private static string ExtractMainAction(string text)
		{
			List<string> list = (from k in ImportantKeywords.Keys
				where text.Contains(k)
				orderby ImportantKeywords[k] descending, k.Length descending
				select k).ToList();
			if (GenCollection.Any<string>(list))
			{
				return list.First();
			}
			string[] array = new string[14]
			{
				"种植", "建造", "完成", "击杀", "攻击", "防御", "制作", "烹饪", "研究", "治疗",
				"加入", "离开", "死亡", "受伤"
			};
			foreach (string text2 in array)
			{
				if (text.Contains(text2))
				{
					return text2;
				}
			}
			return null;
		}

		private static string ExtractTarget(string text, string mainPerson)
		{
			string text2 = text;
			if (!string.IsNullOrEmpty(mainPerson))
			{
				int num = text.IndexOf(mainPerson);
				if (num >= 0)
				{
					text2 = text.Substring(num + mainPerson.Length);
				}
			}
			string[] array = new string[8] { "击杀", "攻击", "种植", "建造", "制作", "完成", "治疗", "了" };
			foreach (string text3 in array)
			{
				int num2 = text2.IndexOf(text3);
				if (num2 < 0)
				{
					continue;
				}
				int num3 = num2 + text3.Length;
				if (num3 < text2.Length)
				{
					string text4 = text2.Substring(num3, Math.Min(15, text2.Length - num3));
					text4 = text4.Split('，', '。', '、', '；', '的', '在', '和', ' ')[0].Trim();
					if (text4.Length >= 2 && text4.Length <= 10)
					{
						return text4;
					}
				}
			}
			return null;
		}

		private static string ExtractQuantity(string text)
		{
			Match match = Regex.Match(text, "\\d+");
			if (match.Success)
			{
				int num = int.Parse(match.Value);
				if (num > 1 && num < 10000)
				{
					return $"×{num}";
				}
			}
			return null;
		}

		public static void CleanupProcessedRecords()
		{
			if (processedLogIDs.Count <= 2000)
			{
				return;
			}
			foreach (int item in processedLogIDs.Take(1000).ToList())
			{
				processedLogIDs.Remove(item);
			}
		}

		private static void UpdateEventKnowledgeTimePrefix(CommonKnowledgeLibrary library)
		{
			if (library == null)
			{
				return;
			}
			try
			{
				int ticksGame = Find.TickManager.TicksGame;
				int num = 0;
				foreach (CommonKnowledgeEntry item in (from e in library.Entries
					where e.tag != null && (e.tag.Contains("事件") || e.tag.Contains("历史"))
					where e.creationTick >= 0
					select e).ToList())
				{
					item.UpdateEventTimePrefix(ticksGame);
					num++;
				}
			}
			catch (Exception ex)
			{
				Log.Error("[EventRecord] Error updating event time prefix: " + ex.Message);
			}
		}

		private static string ExtractOriginalEventText(string eventText)
		{
			if (string.IsNullOrEmpty(eventText))
			{
				return eventText;
			}
			string[] array = new string[12]
			{
				"今天", "1天前", "2天前", "3天前", "4天前", "5天前", "6天前", "约3天前", "约4天前", "约5天前",
				"约6天前", "约7天前"
			};
			foreach (string text in array)
			{
				if (eventText.StartsWith(text))
				{
					return eventText.Substring(text.Length);
				}
			}
			return eventText;
		}

		private static bool IsConversationContent(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}
			return new string[20]
			{
				"说:", "said:", "说：", "说道:", "说道：", "问:", "asked:", "问：", "问道:", "问道：",
				"回答:", "replied:", "回答：", "答道:", "答道：", "叫道:", "shouted:", "叫道：", "喊道:", "喊道："
			}.Any((string marker) => text.Contains(marker));
		}

		private static bool IsBoringMessage(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return true;
			}
			return new string[11]
			{
				"走路", "吃饭", "睡觉", "娱乐", "闲逛", "休息", "walking", "eating", "sleeping", "recreation",
				"wandering"
			}.Any((string k) => text.Contains(k));
		}

		private static float CalculateImportance(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return 0.5f;
			}
			KeyValuePair<string, float> keyValuePair = (from kv in ImportantKeywords
				where text.Contains(kv.Key)
				orderby kv.Value descending
				select kv).FirstOrDefault();
			if (keyValuePair.Key != null)
			{
				return keyValuePair.Value;
			}
			return 0.4f;
		}
	}
	public static class ExtendedKnowledgeEntry
	{
		public class ExtendedProperties
		{
			public bool canBeExtracted;

			public bool canBeMatched;
		}

		private static Dictionary<string, ExtendedProperties> extendedProps = new Dictionary<string, ExtendedProperties>();

		public static ExtendedProperties GetExtendedProperties(CommonKnowledgeEntry entry)
		{
			if (entry == null)
			{
				return new ExtendedProperties();
			}
			if (!extendedProps.ContainsKey(entry.id))
			{
				extendedProps[entry.id] = new ExtendedProperties();
			}
			return extendedProps[entry.id];
		}

		public static void SetCanBeExtracted(CommonKnowledgeEntry entry, bool value)
		{
			if (entry != null)
			{
				GetExtendedProperties(entry).canBeExtracted = value;
			}
		}

		public static void SetCanBeMatched(CommonKnowledgeEntry entry, bool value)
		{
			if (entry != null)
			{
				GetExtendedProperties(entry).canBeMatched = value;
			}
		}

		public static bool CanBeExtracted(CommonKnowledgeEntry entry)
		{
			if (entry == null)
			{
				return false;
			}
			return GetExtendedProperties(entry).canBeExtracted;
		}

		public static bool CanBeMatched(CommonKnowledgeEntry entry)
		{
			if (entry == null)
			{
				return false;
			}
			return GetExtendedProperties(entry).canBeMatched;
		}

		public static void CleanupDeletedEntries(CommonKnowledgeLibrary library)
		{
			if (library == null)
			{
				return;
			}
			HashSet<string> hashSet = new HashSet<string>();
			foreach (CommonKnowledgeEntry entry in library.Entries)
			{
				if (entry != null)
				{
					hashSet.Add(entry.id);
				}
			}
			List<string> list = new List<string>();
			foreach (string key in extendedProps.Keys)
			{
				if (!hashSet.Contains(key))
				{
					list.Add(key);
				}
			}
			foreach (string item in list)
			{
				extendedProps.Remove(item);
			}
		}

		public static void ExposeData()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Invalid comparison between Unknown and I4
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Invalid comparison between Unknown and I4
			List<string> list = null;
			List<bool> list2 = null;
			List<bool> list3 = null;
			if ((int)Scribe.mode == 1)
			{
				list = new List<string>(extendedProps.Keys);
				list2 = new List<bool>();
				list3 = new List<bool>();
				foreach (string item in list)
				{
					list2.Add(extendedProps[item].canBeExtracted);
					list3.Add(extendedProps[item].canBeMatched);
				}
			}
			Scribe_Collections.Look<string>(ref list, "extendedKnowledgeKeys", (LookMode)1, Array.Empty<object>());
			Scribe_Collections.Look<bool>(ref list2, "canBeExtractedList", (LookMode)1, Array.Empty<object>());
			Scribe_Collections.Look<bool>(ref list3, "canBeMatchedList", (LookMode)1, Array.Empty<object>());
			if (((int)Scribe.mode == 2 || (int)Scribe.mode == 4) && list != null && list2 != null && list3 != null)
			{
				extendedProps.Clear();
				for (int i = 0; i < list.Count && i < list2.Count && i < list3.Count; i++)
				{
					extendedProps[list[i]] = new ExtendedProperties
					{
						canBeExtracted = list2[i],
						canBeMatched = list3[i]
					};
				}
				Log.Message($"[RimTalk-ExpandMemory] Loaded extended properties for {list.Count} knowledge entries");
			}
		}
	}
	public static class KeywordExtractionHelper
	{
		public static PawnKeywordInfo ExtractPawnKeywords(List<string> keywords, Pawn pawn)
		{
			PawnKeywordInfo pawnKeywordInfo = new PawnKeywordInfo
			{
				PawnName = (((pawn != null) ? ((Entity)pawn).LabelShort : null) ?? "Unknown")
			};
			if (pawn == null || keywords == null)
			{
				return pawnKeywordInfo;
			}
			try
			{
				ExtractNameKeywords(pawn, keywords, pawnKeywordInfo);
				ExtractAgeKeywords(pawn, keywords, pawnKeywordInfo);
				ExtractGenderKeywords(pawn, keywords, pawnKeywordInfo);
				ExtractRaceKeywords(pawn, keywords, pawnKeywordInfo);
				ExtractIdentityKeywords(pawn, keywords, pawnKeywordInfo);
				ExtractTraitKeywords(pawn, keywords, pawnKeywordInfo);
				ExtractSkillKeywords(pawn, keywords, pawnKeywordInfo);
				ExtractHealthKeywords(pawn, keywords, pawnKeywordInfo);
				ExtractRelationshipKeywords(pawn, keywords, pawnKeywordInfo);
				ExtractBackstoryKeywords(pawn, keywords, pawnKeywordInfo);
				pawnKeywordInfo.TotalCount = pawnKeywordInfo.NameKeywords.Count + pawnKeywordInfo.AgeKeywords.Count + pawnKeywordInfo.GenderKeywords.Count + pawnKeywordInfo.RaceKeywords.Count + pawnKeywordInfo.TraitKeywords.Count + pawnKeywordInfo.SkillKeywords.Count + pawnKeywordInfo.SkillLevelKeywords.Count + pawnKeywordInfo.HealthKeywords.Count + pawnKeywordInfo.RelationshipKeywords.Count + pawnKeywordInfo.BackstoryKeywords.Count + pawnKeywordInfo.ChildhoodKeywords.Count;
			}
			catch (Exception ex)
			{
				Log.Error("[KeywordExtraction] Error extracting pawn keywords: " + ex.Message);
			}
			return pawnKeywordInfo;
		}

		private static void ExtractNameKeywords(Pawn pawn, List<string> keywords, PawnKeywordInfo info)
		{
			Name name = pawn.Name;
			if (!string.IsNullOrEmpty((name != null) ? name.ToStringShort : null))
			{
				AddAndRecord(pawn.Name.ToStringShort, keywords, info.NameKeywords);
			}
		}

		private static void ExtractAgeKeywords(Pawn pawn, List<string> keywords, PawnKeywordInfo info)
		{
			if (pawn.RaceProps != null && pawn.RaceProps.Humanlike)
			{
				float ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
				if (ageBiologicalYearsFloat < 3f)
				{
					AddAndRecord("婴儿", keywords, info.AgeKeywords);
					AddAndRecord("宝宝", keywords, info.AgeKeywords);
				}
				else if (ageBiologicalYearsFloat < 13f)
				{
					AddAndRecord("儿童", keywords, info.AgeKeywords);
					AddAndRecord("小孩", keywords, info.AgeKeywords);
				}
				else if (ageBiologicalYearsFloat < 18f)
				{
					AddAndRecord("青少年", keywords, info.AgeKeywords);
				}
				else
				{
					AddAndRecord("成人", keywords, info.AgeKeywords);
				}
			}
		}

		private static void ExtractGenderKeywords(Pawn pawn, List<string> keywords, PawnKeywordInfo info)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			AddAndRecord(GenderUtility.GetLabel(pawn.gender, false), keywords, info.GenderKeywords);
		}

		private static void ExtractRaceKeywords(Pawn pawn, List<string> keywords, PawnKeywordInfo info)
		{
			if (((Thing)pawn).def == null)
			{
				return;
			}
			AddAndRecord(((Def)((Thing)pawn).def).label, keywords, info.RaceKeywords);
			try
			{
				if (pawn.genes != null && pawn.genes.Xenotype != null)
				{
					string text = ((Def)pawn.genes.Xenotype).label ?? ((Def)pawn.genes.Xenotype).defName;
					if (!string.IsNullOrEmpty(text))
					{
						AddAndRecord(text, keywords, info.RaceKeywords);
					}
				}
			}
			catch
			{
			}
		}

		private static void ExtractIdentityKeywords(Pawn pawn, List<string> keywords, PawnKeywordInfo info)
		{
			if (pawn.IsColonist)
			{
				AddAndRecord("殖民者", keywords, info.IdentityKeywords);
			}
			else if (pawn.IsPrisoner)
			{
				AddAndRecord("囚犯", keywords, info.IdentityKeywords);
			}
			else if (pawn.IsSlaveOfColony)
			{
				AddAndRecord("奴隶", keywords, info.IdentityKeywords);
			}
			else if (pawn.HostFaction == Faction.OfPlayer)
			{
				AddAndRecord("访客", keywords, info.IdentityKeywords);
			}
			else if (((Thing)pawn).Faction != null && ((Thing)pawn).Faction != Faction.OfPlayer)
			{
				AddAndRecord(((Thing)pawn).Faction.Name, keywords, info.IdentityKeywords);
			}
		}

		private static void ExtractTraitKeywords(Pawn pawn, List<string> keywords, PawnKeywordInfo info)
		{
			if (pawn.story?.traits == null)
			{
				return;
			}
			foreach (Trait allTrait in pawn.story.traits.allTraits)
			{
				if (allTrait != null && ((Def)(allTrait.def?)).label != null)
				{
					AddAndRecord(((Def)allTrait.def).label, keywords, info.TraitKeywords);
				}
			}
		}

		private static void ExtractSkillKeywords(Pawn pawn, List<string> keywords, PawnKeywordInfo info)
		{
			if (pawn.skills == null)
			{
				return;
			}
			foreach (SkillRecord skill in pawn.skills.skills)
			{
				if (skill.TotallyDisabled || ((Def)(skill.def?)).label == null)
				{
					continue;
				}
				int level = skill.Level;
				if (level >= 5)
				{
					AddAndRecord(((Def)skill.def).label, keywords, info.SkillKeywords);
					AddAndRecord(((Def)skill.def).label + level, keywords, info.SkillKeywords);
					if (level >= 15)
					{
						AddAndRecord(((Def)skill.def).label + "精通", keywords, info.SkillLevelKeywords);
					}
					else if (level >= 10)
					{
						AddAndRecord(((Def)skill.def).label + "熟练", keywords, info.SkillLevelKeywords);
					}
				}
			}
		}

		private static void ExtractHealthKeywords(Pawn pawn, List<string> keywords, PawnKeywordInfo info)
		{
			if (pawn.health != null)
			{
				if (GenCollection.Any<BodyPartRecord>(pawn.health.hediffSet.GetInjuredParts()))
				{
					AddAndRecord("受伤", keywords, info.HealthKeywords);
				}
				else if (!pawn.health.HasHediffsNeedingTend(false))
				{
					AddAndRecord("健康", keywords, info.HealthKeywords);
				}
			}
		}

		private static void ExtractRelationshipKeywords(Pawn pawn, List<string> keywords, PawnKeywordInfo info)
		{
			if (pawn.relations == null)
			{
				return;
			}
			foreach (Pawn item in pawn.relations.RelatedPawns.Take(5))
			{
				Name name = item.Name;
				if (!string.IsNullOrEmpty((name != null) ? name.ToStringShort : null))
				{
					AddAndRecord(item.Name.ToStringShort, keywords, info.RelationshipKeywords);
				}
			}
		}

		private static void ExtractBackstoryKeywords(Pawn pawn, List<string> keywords, PawnKeywordInfo info)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			Pawn_StoryTracker story = pawn.story;
			if (((story != null) ? story.Adulthood : null) != null)
			{
				string text = pawn.story.Adulthood.TitleFor(pawn.gender);
				if (!string.IsNullOrEmpty(text))
				{
					AddAndRecord(text, keywords, info.BackstoryKeywords);
				}
			}
			Pawn_StoryTracker story2 = pawn.story;
			if (((story2 != null) ? story2.Childhood : null) != null)
			{
				string text2 = pawn.story.Childhood.TitleFor(pawn.gender);
				if (!string.IsNullOrEmpty(text2))
				{
					AddAndRecord(text2, keywords, info.ChildhoodKeywords);
				}
			}
		}

		private static void AddAndRecord(string keyword, List<string> allKeywords, List<string> categoryKeywords)
		{
			if (!string.IsNullOrEmpty(keyword))
			{
				if (!allKeywords.Contains(keyword))
				{
					allKeywords.Add(keyword);
				}
				if (!categoryKeywords.Contains(keyword))
				{
					categoryKeywords.Add(keyword);
				}
			}
		}
	}
	public enum KnowledgeEntryCategory
	{
		None,
		Instructions,
		Lore,
		PawnStatus,
		History,
		Other
	}
	public enum KeywordMatchMode
	{
		Any,
		All
	}
	public enum KnowledgeMatchType
	{
		Keyword,
		Vector,
		Mixed
	}
	public class CommonKnowledgeEntry : IExposable
	{
		public string id;

		public string tag;

		public string content;

		public float importance;

		public List<string> keywords;

		public bool isEnabled;

		public bool isUserEdited;

		public int targetPawnId = -1;

		public int creationTick = -1;

		public string originalEventText = "";

		public KeywordMatchMode matchMode;

		public KnowledgeEntryCategory category;

		private List<string> cachedTags;

		public void InvalidateCache()
		{
			cachedTags = null;
		}

		public CommonKnowledgeEntry()
		{
			id = "ck-" + Guid.NewGuid().ToString("N").Substring(0, 12);
			keywords = new List<string>();
			isEnabled = true;
			importance = 0.5f;
			targetPawnId = -1;
			creationTick = -1;
			originalEventText = "";
			matchMode = KeywordMatchMode.Any;
		}

		public CommonKnowledgeEntry(string tag, string content)
			: this()
		{
			this.tag = tag;
			this.content = content;
		}

		public void ExposeData()
		{
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Invalid comparison between Unknown and I4
			Scribe_Values.Look<string>(ref id, "id", (string)null, false);
			Scribe_Values.Look<string>(ref tag, "tag", (string)null, false);
			Scribe_Values.Look<string>(ref content, "content", (string)null, false);
			Scribe_Values.Look<float>(ref importance, "importance", 0.5f, false);
			Scribe_Values.Look<bool>(ref isEnabled, "isEnabled", true, false);
			Scribe_Values.Look<bool>(ref isUserEdited, "isUserEdited", false, false);
			Scribe_Values.Look<int>(ref targetPawnId, "targetPawnId", -1, false);
			Scribe_Values.Look<int>(ref creationTick, "creationTick", -1, false);
			Scribe_Values.Look<string>(ref originalEventText, "originalEventText", "", false);
			Scribe_Collections.Look<string>(ref keywords, "keywords", (LookMode)1, Array.Empty<object>());
			Scribe_Values.Look<KeywordMatchMode>(ref matchMode, "matchMode", KeywordMatchMode.Any, false);
			Scribe_Values.Look<KnowledgeEntryCategory>(ref category, "category", KnowledgeEntryCategory.None, false);
			if ((int)Scribe.mode == 4)
			{
				if (keywords == null)
				{
					keywords = new List<string>();
				}
				cachedTags = null;
				if (string.IsNullOrEmpty(originalEventText) && !string.IsNullOrEmpty(content))
				{
					originalEventText = RemoveTimePrefix(content);
				}
			}
		}

		private static string RemoveTimePrefix(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}
			string[] array = new string[12]
			{
				"今天", "1天前", "2天前", "3天前", "4天前", "5天前", "6天前", "约3天前", "约4天前", "约5天前",
				"约6天前", "约7天前"
			};
			foreach (string text2 in array)
			{
				if (text.StartsWith(text2))
				{
					return text.Substring(text2.Length);
				}
			}
			return text;
		}

		public void UpdateEventTimePrefix(int currentTick)
		{
			if (creationTick >= 0 && !string.IsNullOrEmpty(originalEventText) && !isUserEdited)
			{
				int num = (currentTick - creationTick) / 60000;
				string text = "";
				text = ((num < 1) ? "今天" : ((num == 1) ? "1天前" : ((num == 2) ? "2天前" : ((num >= 7) ? "约7天前" : $"约{num}天前"))));
				content = text + originalEventText;
			}
		}

		public List<string> GetTags()
		{
			if (cachedTags != null)
			{
				return cachedTags;
			}
			if (string.IsNullOrEmpty(tag))
			{
				cachedTags = new List<string>();
				return cachedTags;
			}
			cachedTags = (from t in tag.Split(new char[5] { ',', '，', '、', ';', '；' }, StringSplitOptions.RemoveEmptyEntries)
				select t.Trim() into t
				where !string.IsNullOrEmpty(t)
				select t).ToList();
			return cachedTags;
		}

		public string FormatForExport()
		{
			bool flag = ExtendedKnowledgeEntry.CanBeExtracted(this);
			bool flag2 = ExtendedKnowledgeEntry.CanBeMatched(this);
			return $"[{tag}|{importance:F2}|{matchMode}|{flag}|{flag2}]{content}";
		}

		public override string ToString()
		{
			return FormatForExport();
		}

		public bool IsRuleKnowledge()
		{
			if (string.IsNullOrEmpty(tag))
			{
				return false;
			}
			string text = tag.ToLower();
			if (!text.Contains("规则") && !text.Contains("instructions"))
			{
				return text.Contains("rule");
			}
			return true;
		}
	}
	public class KnowledgeScore
	{
		public CommonKnowledgeEntry Entry;

		public float Score;
	}
	public class KnowledgeScoreDetail
	{
		public CommonKnowledgeEntry Entry;

		public bool IsEnabled;

		public float TotalScore;

		public float BaseScore;

		public float ManualBonus;

		public float MatchTypeScore;

		public KnowledgeMatchType MatchType;

		public float JaccardScore;

		public float TagScore;

		public float ImportanceScore;

		public int KeywordMatchCount;

		public List<string> MatchedKeywords = new List<string>();

		public List<string> MatchedTags = new List<string>();

		public string FailReason;
	}
	public class KeywordExtractionInfo
	{
		public List<string> ContextKeywords = new List<string>();

		public int TotalKeywords;

		public int PawnKeywordsCount;

		public PawnKeywordInfo PawnInfo;
	}
	public class PawnKeywordInfo
	{
		public string PawnName;

		public List<string> NameKeywords = new List<string>();

		public List<string> AgeKeywords = new List<string>();

		public List<string> GenderKeywords = new List<string>();

		public List<string> RaceKeywords = new List<string>();

		public List<string> IdentityKeywords = new List<string>();

		public List<string> TraitKeywords = new List<string>();

		public List<string> SkillKeywords = new List<string>();

		public List<string> SkillLevelKeywords = new List<string>();

		public List<string> HealthKeywords = new List<string>();

		public List<string> RelationshipKeywords = new List<string>();

		public List<string> BackstoryKeywords = new List<string>();

		public List<string> ChildhoodKeywords = new List<string>();

		public int TotalCount;
	}
	public static class PawnStatusKnowledgeGenerator
	{
		private static Dictionary<int, int> lastUpdateTicks = new Dictionary<int, int>();

		private const int UPDATE_INTERVAL_TICKS = 60000;

		private static HashSet<int> userDeletedPawns = new HashSet<int>();

		private const int NEW_COLONIST_THRESHOLD_DAYS = 7;

		public static void MarkAsUserDeleted(int pawnId)
		{
			userDeletedPawns.Add(pawnId);
			lastUpdateTicks.Remove(pawnId);
			if (Prefs.DevMode)
			{
				Log.Message($"[PawnStatus] Marked pawn {pawnId} as user-deleted, will not regenerate");
			}
		}

		public static bool IsUserDeleted(int pawnId)
		{
			return userDeletedPawns.Contains(pawnId);
		}

		public static void ClearUserDeletedMark(int pawnId)
		{
			userDeletedPawns.Remove(pawnId);
			if (Prefs.DevMode)
			{
				Log.Message($"[PawnStatus] Cleared user-deleted mark for pawn {pawnId}");
			}
		}

		public static void UpdateAllColonistStatus()
		{
			if (!RimTalkMemoryPatchMod.Settings.enablePawnStatusKnowledge)
			{
				return;
			}
			CommonKnowledgeLibrary commonKnowledge = MemoryManager.GetCommonKnowledge();
			if (commonKnowledge == null)
			{
				return;
			}
			int ticksGame = Find.TickManager.TicksGame;
			int num = 0;
			List<Pawn> list = new List<Pawn>();
			foreach (Map map in Find.Maps)
			{
				if (map.mapPawns != null)
				{
					list.AddRange(map.mapPawns.FreeColonists);
				}
			}
			foreach (Caravan caravan in Find.WorldObjects.Caravans)
			{
				if (!caravan.IsPlayerControlled || caravan.pawns == null)
				{
					continue;
				}
				foreach (Pawn item in caravan.pawns.InnerListForReading)
				{
					if (item.IsColonist && !list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
			foreach (Pawn item2 in list)
			{
				try
				{
					int thingIDNumber = ((Thing)item2).thingIDNumber;
					if (!IsUserDeleted(thingIDNumber))
					{
						if (!lastUpdateTicks.TryGetValue(thingIDNumber, out var value))
						{
							value = 0;
						}
						if (ticksGame - value >= 60000)
						{
							UpdatePawnStatusKnowledge(item2, commonKnowledge, ticksGame);
							lastUpdateTicks[thingIDNumber] = ticksGame;
							num++;
						}
					}
				}
				catch (Exception ex)
				{
					if (Prefs.DevMode && Random.value < 0.2f)
					{
						Log.Error("[PawnStatus] Error updating status for " + ((Entity)item2).LabelShort + ": " + ex.Message);
					}
				}
			}
			_ = 0;
		}

		public static void UpdatePawnStatusKnowledge(Pawn pawn, CommonKnowledgeLibrary library, int currentTick)
		{
			if (pawn == null || library == null)
			{
				return;
			}
			try
			{
				if (pawn.RaceProps != null && pawn.RaceProps.Humanlike && pawn.ageTracker.AgeBiologicalYearsFloat < 3f)
				{
					CleanupPawnStatusKnowledge(pawn, library);
					lastUpdateTicks.Remove(((Thing)pawn).thingIDNumber);
					return;
				}
				int joinTick = CalculateJoinTick(pawn, currentTick);
				int daysInColony = CalculateDaysInColony(joinTick, currentTick);
				string tag = "殖民者状态," + ((Entity)pawn).LabelShort;
				CommonKnowledgeEntry commonKnowledgeEntry = GenCollection.FirstOrDefault<CommonKnowledgeEntry>(library.Entries, (Predicate<CommonKnowledgeEntry>)((CommonKnowledgeEntry e) => (e.targetPawnId == ((Thing)pawn).thingIDNumber && e.tag.Contains("殖民者状态")) || (e.tag.Contains(((Entity)pawn).LabelShort) && e.tag.Contains("殖民者状态"))));
				float importance = 0.5f;
				if (commonKnowledgeEntry != null)
				{
					if (!commonKnowledgeEntry.isUserEdited && IsAutoGeneratedContent(commonKnowledgeEntry.content))
					{
						string existingJoinDate = ExtractJoinDateFromContent(commonKnowledgeEntry.content);
						string content = GenerateStatusContent(pawn, daysInColony, joinTick, existingJoinDate);
						commonKnowledgeEntry.content = content;
						commonKnowledgeEntry.importance = importance;
						commonKnowledgeEntry.targetPawnId = ((Thing)pawn).thingIDNumber;
						if (!commonKnowledgeEntry.tag.Contains(((Entity)pawn).LabelShort))
						{
							commonKnowledgeEntry.tag = tag;
						}
					}
				}
				else
				{
					string content2 = GenerateStatusContent(pawn, daysInColony, joinTick);
					CommonKnowledgeEntry entry = new CommonKnowledgeEntry(tag, content2)
					{
						importance = importance,
						isEnabled = true,
						isUserEdited = false,
						targetPawnId = ((Thing)pawn).thingIDNumber
					};
					library.AddEntry(entry);
				}
			}
			catch (Exception ex)
			{
				Pawn obj = pawn;
				Log.Error("[PawnStatus] Failed to update status for " + (((obj != null) ? ((Entity)obj).LabelShort : null) ?? "Unknown") + ": " + ex.Message);
			}
		}

		private static int CalculateJoinTick(Pawn pawn, int currentTick)
		{
			try
			{
				if (pawn.records == null)
				{
					return currentTick;
				}
				RecordDef timeAsColonistOrColonyAnimal = RecordDefOf.TimeAsColonistOrColonyAnimal;
				if (timeAsColonistOrColonyAnimal == null)
				{
					if (Prefs.DevMode)
					{
						Log.Warning("[PawnStatus] RecordDef 'TimeAsColonistOrColonyAnimal' not found (this should never happen with strong reference)");
					}
					return currentTick;
				}
				float value = pawn.records.GetValue(timeAsColonistOrColonyAnimal);
				if (value <= 0f)
				{
					return currentTick;
				}
				int num = currentTick - (int)value;
				if (num < 0)
				{
					num = 0;
				}
				return num;
			}
			catch (Exception ex)
			{
				Log.Error("[PawnStatus] Error calculating join tick for " + ((pawn != null) ? ((Entity)pawn).LabelShort : null) + ": " + ex.Message);
				return currentTick;
			}
		}

		private static int CalculateDaysInColony(int joinTick, int currentTick)
		{
			int num = (currentTick - joinTick) / 60000;
			if (num < 0)
			{
				Log.Warning($"[PawnStatus] Negative days detected: {num}, resetting to 0");
				num = 0;
			}
			return num;
		}

		private static string GenerateStatusContent(Pawn pawn, int daysInColony, int joinTick, string existingJoinDate = null)
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			string labelShort = ((Entity)pawn).LabelShort;
			string text;
			if (!string.IsNullOrEmpty(existingJoinDate))
			{
				text = existingJoinDate;
			}
			else
			{
				Map map = ((Thing)pawn).Map;
				? val;
				if (map == null)
				{
					Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
					val = ((anyPlayerHomeMap != null) ? anyPlayerHomeMap.Tile : PlanetTile.op_Implicit(0));
				}
				else
				{
					val = map.Tile;
				}
				int num = PlanetTile.op_Implicit((PlanetTile)val);
				float x = Find.WorldGrid.LongLatOf(PlanetTile.op_Implicit(num)).x;
				int num2 = GenDate.DayOfQuadrum((long)joinTick, x) + 1;
				Quadrum val2 = GenDate.Quadrum((long)joinTick, x);
				int num3 = GenDate.Year((long)joinTick, x);
				text = $"{QuadrumUtility.Label(val2)} {num2}日, {num3}年";
			}
			string completeRaceInfo = GetCompleteRaceInfo(pawn);
			string text2 = "";
			text2 = ((daysInColony >= 7) ? $"{labelShort}是殖民地的资深成员，已加入殖民地 {daysInColony} 天（加入于{text}），对殖民地的历史和成员关系较为熟悉" : (daysInColony switch
			{
				0 => labelShort + "是殖民地的新成员，今天(" + text + ")刚加入", 
				1 => labelShort + "是殖民地的新成员，昨天(" + text + ")加入", 
				_ => $"{labelShort}是殖民地的新成员，{daysInColony}天前({text})加入", 
			}));
			if (!string.IsNullOrEmpty(completeRaceInfo))
			{
				if (daysInColony < 7)
				{
					return text2 + "。" + completeRaceInfo + "。对殖民地的历史和成员关系尚不熟悉";
				}
				return text2 + "。" + completeRaceInfo;
			}
			return text2;
		}

		private static string ExtractJoinDateFromContent(string content)
		{
			if (string.IsNullOrEmpty(content))
			{
				return null;
			}
			try
			{
				Match match = Regex.Match(content, "\\(([^)]+季\\s*\\d+日,\\s*\\d+年)\\)");
				if (match.Success)
				{
					return match.Groups[1].Value;
				}
				match = Regex.Match(content, "加入于([^，。）]+季\\s*\\d+日,\\s*\\d+年)");
				if (match.Success)
				{
					return match.Groups[1].Value;
				}
			}
			catch (Exception ex)
			{
				if (Prefs.DevMode)
				{
					Log.Warning("[PawnStatus] Failed to extract join date from content: " + ex.Message);
				}
			}
			return null;
		}

		private static string GetCompleteRaceInfo(Pawn pawn)
		{
			if (((Thing)(pawn?)).def == null)
			{
				return "";
			}
			try
			{
				string labelShort = ((Entity)pawn).LabelShort;
				string text = ((Def)((Thing)pawn).def).label ?? ((Def)((Thing)pawn).def).defName;
				string text2 = "";
				if (pawn.genes != null && pawn.genes.Xenotype != null)
				{
					text2 = ((Def)pawn.genes.Xenotype).label ?? ((Def)pawn.genes.Xenotype).defName;
				}
				if (string.IsNullOrEmpty(text2) && pawn.story != null)
				{
					FieldInfo field = ((object)pawn.story).GetType().GetField("xenotype");
					if (field != null)
					{
						object value = field.GetValue(pawn.story);
						if (value != null)
						{
							PropertyInfo property = value.GetType().GetProperty("label");
							if (property != null)
							{
								text2 = property.GetValue(value) as string;
							}
						}
					}
				}
				if (string.IsNullOrEmpty(text2) && pawn.genes != null)
				{
					FieldInfo field2 = ((object)pawn.genes).GetType().GetField("xenotypeName");
					if (field2 != null)
					{
						text2 = field2.GetValue(pawn.genes) as string;
					}
				}
				if (!string.IsNullOrEmpty(text2))
				{
					if (text2.Equals(text, StringComparison.OrdinalIgnoreCase))
					{
						return labelShort + "的种族是" + text;
					}
					return labelShort + "的种族是" + text + "-" + text2;
				}
				return labelShort + "的种族是" + text;
			}
			catch (Exception ex)
			{
				if (Prefs.DevMode)
				{
					Log.Warning("[PawnStatus] Failed to extract race info for " + ((Entity)pawn).LabelShort + ": " + ex.Message);
				}
				return ((Entity)pawn).LabelShort + "的种族是" + (((Def)(((Thing)pawn).def?)).label ?? "未知");
			}
		}

		private static bool IsAutoGeneratedContent(string content)
		{
			if (string.IsNullOrEmpty(content))
			{
				return false;
			}
			return new string[4] { "刚加入", "新成员", "资深成员", "已加入殖民地" }.Any((string k) => content.Contains(k));
		}

		public static void CleanupPawnStatusKnowledge(Pawn pawn, CommonKnowledgeLibrary library)
		{
			if (pawn != null && library != null)
			{
				CommonKnowledgeEntry commonKnowledgeEntry = GenCollection.FirstOrDefault<CommonKnowledgeEntry>(library.Entries, (Predicate<CommonKnowledgeEntry>)((CommonKnowledgeEntry e) => e.tag.Contains(((Entity)pawn).LabelShort) && e.tag.Contains("殖民者状态")));
				if (commonKnowledgeEntry != null)
				{
					library.RemoveEntry(commonKnowledgeEntry);
					lastUpdateTicks.Remove(((Thing)pawn).thingIDNumber);
				}
			}
		}

		public static void CleanupUpdateRecords()
		{
			List<Pawn> list = new List<Pawn>();
			foreach (Map map in Find.Maps)
			{
				if (map.mapPawns != null)
				{
					list.AddRange(map.mapPawns.FreeColonists);
				}
			}
			foreach (Caravan caravan in Find.WorldObjects.Caravans)
			{
				if (!caravan.IsPlayerControlled || caravan.pawns == null)
				{
					continue;
				}
				foreach (Pawn item in caravan.pawns.InnerListForReading)
				{
					if (item.IsColonist && !list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
			HashSet<int> hashSet = new HashSet<int>(list.Select((Pawn p) => ((Thing)p).thingIDNumber));
			List<int> list2 = new List<int>();
			foreach (int pawnID in lastUpdateTicks.Keys.ToList())
			{
				if (hashSet.Contains(pawnID))
				{
					continue;
				}
				Pawn val = null;
				foreach (Map map2 in Find.Maps)
				{
					val = GenCollection.FirstOrDefault<Pawn>(map2.mapPawns.AllPawns, (Predicate<Pawn>)((Pawn p) => ((Thing)p).thingIDNumber == pawnID));
					if (val != null)
					{
						break;
					}
				}
				if (val == null && Find.WorldPawns != null)
				{
					val = GenCollection.FirstOrDefault<Pawn>(Find.WorldPawns.AllPawnsAlive, (Predicate<Pawn>)((Pawn p) => ((Thing)p).thingIDNumber == pawnID));
				}
				bool flag = false;
				if (val == null)
				{
					flag = true;
				}
				else if (val.Dead)
				{
					flag = true;
				}
				else if (((Thing)val).Faction != Faction.OfPlayer)
				{
					flag = true;
				}
				if (flag)
				{
					list2.Add(pawnID);
				}
			}
			foreach (int item2 in list2)
			{
				lastUpdateTicks.Remove(item2);
			}
		}
	}
	public static class SuperKeywordEngine
	{
		private static readonly Regex EnglishWordRegex = new Regex("\\b[a-zA-Z][a-zA-Z0-9]{1,}\\b", RegexOptions.Compiled);

		private static readonly HashSet<string> StopWords = new HashSet<string>
		{
			"的", "了", "是", "在", "我", "有", "和", "就", "不", "人",
			"都", "一", "个", "也", "上", "他", "们", "到", "说", "要",
			"去", "你", "会", "着", "没有", "看", "好", "自己", "这", "那",
			"为", "来", "而", "能", "可以", "与", "但", "很", "吗", "吧",
			"啊", "呢", "么", "什么", "怎么", "为什么", "哪里", "谁", "多少", "几个",
			"一些", "一点", "有点", "太", "非常", "比较", "还", "更", "最", "大",
			"小", "多", "少", "新", "旧", "好", "坏"
		};

		private static readonly HashSet<string> ImportantPrefixes = new HashSet<string>
		{
			"龙王", "索拉克", "梅菲斯特", "殖民", "战斗", "受伤", "死亡", "爱情", "友谊", "仇恨",
			"任务", "建造", "种植", "采矿", "研究", "医疗", "袭击", "防御", "贸易", "谈判"
		};

		public static List<WeightedKeyword> ExtractKeywords(string text, int maxKeywords = 100)
		{
			if (string.IsNullOrEmpty(text))
			{
				return new List<WeightedKeyword>();
			}
			if (text.Length > 500)
			{
				text = text.Substring(0, 500);
			}
			Dictionary<string, KeywordScore> dictionary = new Dictionary<string, KeywordScore>();
			ExtractEnglishWords(text, dictionary);
			ExtractChineseWords(text, dictionary);
			int num = dictionary.Values.Sum((KeywordScore s) => s.Frequency);
			foreach (KeywordScore value in dictionary.Values)
			{
				float num2 = (float)value.Frequency / (float)num;
				float num3 = 1f + (float)(value.Length - 2) * 0.3f;
				float num4 = 1f - (float)value.FirstPosition / (float)text.Length * 0.3f;
				float num5 = 1f;
				foreach (string importantPrefix in ImportantPrefixes)
				{
					if (value.Word.StartsWith(importantPrefix))
					{
						num5 = 1.5f;
						break;
					}
				}
				value.Weight = num2 * num3 * num4 * num5;
			}
			return (from s in dictionary.Values.OrderByDescending((KeywordScore s) => s.Weight).ThenBy((KeywordScore s) => s.Word, StringComparer.Ordinal).Take(maxKeywords)
				select new WeightedKeyword
				{
					Word = s.Word,
					Weight = s.Weight
				}).ToList();
		}

		private static void ExtractEnglishWords(string text, Dictionary<string, KeywordScore> keywordScores)
		{
			foreach (Match item in EnglishWordRegex.Matches(text))
			{
				string value = item.Value;
				int index = item.Index;
				if (!IsLowQualityKeyword(value) && !StopWords.Contains(value.ToLower()))
				{
					if (!keywordScores.ContainsKey(value))
					{
						keywordScores[value] = new KeywordScore
						{
							Word = value,
							Length = value.Length,
							FirstPosition = index
						};
					}
					keywordScores[value].Frequency++;
				}
			}
		}

		private static void ExtractChineseWords(string text, Dictionary<string, KeywordScore> keywordScores)
		{
			for (int i = 2; i <= 6; i++)
			{
				for (int j = 0; j <= text.Length - i; j++)
				{
					string text2 = text.Substring(j, i);
					if (ContainsChinese(text2) && !StopWords.Contains(text2))
					{
						if (!keywordScores.ContainsKey(text2))
						{
							keywordScores[text2] = new KeywordScore
							{
								Word = text2,
								Length = i,
								FirstPosition = j
							};
						}
						keywordScores[text2].Frequency++;
					}
				}
			}
		}

		private static bool ContainsChinese(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}
			foreach (char c in text)
			{
				if (c >= '一' && c <= '龥')
				{
					return true;
				}
			}
			return false;
		}

		public static float CalculateBM25Score(List<WeightedKeyword> queryKeywords, string document, List<string> documentKeywords, float k1 = 1.5f, float b = 0.75f)
		{
			if (queryKeywords.Count == 0 || string.IsNullOrEmpty(document))
			{
				return 0f;
			}
			float num = 0f;
			int length = document.Length;
			float num2 = 100f;
			foreach (WeightedKeyword queryKw in queryKeywords)
			{
				int num3 = GenCollection.Count<string>(documentKeywords, (Predicate<string>)((string kw) => kw == queryKw.Word));
				if (num3 != 0)
				{
					float num4 = (float)Math.Log(1.0 + 1.0 / ((double)num3 + 0.5));
					float num5 = (float)num3 * (k1 + 1f) / ((float)num3 + k1 * (1f - b + b * (float)length / num2));
					num += num4 * num5 * queryKw.Weight;
				}
			}
			return num;
		}

		public static bool FuzzyMatch(string word1, string word2, float threshold = 0.8f)
		{
			if (word1 == word2)
			{
				return true;
			}
			int num = LevenshteinDistance(word1, word2);
			int num2 = Math.Max(word1.Length, word2.Length);
			return 1f - (float)num / (float)num2 >= threshold;
		}

		private static int LevenshteinDistance(string s, string t)
		{
			int length = s.Length;
			int length2 = t.Length;
			int[,] array = new int[length + 1, length2 + 1];
			if (length == 0)
			{
				return length2;
			}
			if (length2 == 0)
			{
				return length;
			}
			for (int i = 0; i <= length; i++)
			{
				array[i, 0] = i;
			}
			for (int j = 0; j <= length2; j++)
			{
				array[0, j] = j;
			}
			for (int k = 1; k <= length; k++)
			{
				for (int l = 1; l <= length2; l++)
				{
					int num = ((t[l - 1] != s[k - 1]) ? 1 : 0);
					array[k, l] = Math.Min(Math.Min(array[k - 1, l] + 1, array[k, l - 1] + 1), array[k - 1, l - 1] + num);
				}
			}
			return array[length, length2];
		}

		private static bool IsLowQualityKeyword(string word)
		{
			if (string.IsNullOrEmpty(word))
			{
				return true;
			}
			if (word.Length <= 2 && word.All(char.IsDigit))
			{
				return true;
			}
			if (word.Length <= 3 && word.All((char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')) && !new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "AI", "HP", "DPS", "XP", "UI", "API", "CPU", "GPU", "RAM", "SSD" }.Contains(word))
			{
				return true;
			}
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"tion", "ment", "ing", "ed", "er", "ly", "ness", "ity", "able", "ible",
				"al", "ful", "less", "ous", "ive", "ant", "ent", "ism", "ist", "ship"
			};
			if (word.Length <= 4 && hashSet.Contains(word))
			{
				return true;
			}
			if (word.Length == 2)
			{
				bool num = word.Any(char.IsDigit);
				bool flag = word.Any((char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'));
				if (num & flag)
				{
					return true;
				}
			}
			if (word.All((char c) => !char.IsLetterOrDigit(c)))
			{
				return true;
			}
			if (word.Trim().Length == 1)
			{
				return true;
			}
			return false;
		}
	}
	internal class KeywordScore
	{
		public string Word;

		public int Length;

		public int Frequency;

		public int FirstPosition;

		public float Weight;
	}
	public class WeightedKeyword
	{
		public string Word;

		public float Weight;
	}
	public static class AdvancedScoringSystem
	{
		public enum ConversationSceneType
		{
			Casual,
			EmotionalTalk,
			WorkDiscussion,
			HistoryRecall,
			Emergency,
			Introduction
		}

		public class ScoringWeights
		{
			public float ContextRelevance = 0.4f;

			public float Recency = 0.2f;

			public float Importance = 0.2f;

			public float Diversity = 0.1f;

			public float LayerPriority = 0.1f;

			public float EmotionalBoost = 1.3f;

			public float RelationshipBoost = 1.2f;

			public float ConversationBoost = 1.1f;
		}

		public class ContextFeatures
		{
			public List<string> Keywords = new List<string>();

			public List<string> Entities = new List<string>();

			public List<string> Topics = new List<string>();

			public HashSet<string> EmotionWords = new HashSet<string>();

			public ConversationSceneType Scene;
		}

		private static ScoringWeights defaultWeights = new ScoringWeights();

		public static List<ScoredItem<MemoryEntry>> ScoreMemories(List<MemoryEntry> memories, string context, Pawn speaker = null, Pawn listener = null)
		{
			if (memories == null || memories.Count == 0)
			{
				return new List<ScoredItem<MemoryEntry>>();
			}
			ScoringWeights weights = AdjustWeightsForScene(IdentifyScene(context), defaultWeights);
			ContextFeatures features = ExtractContextFeatures(context, speaker, listener);
			List<ScoredItem<MemoryEntry>> list = new List<ScoredItem<MemoryEntry>>();
			foreach (MemoryEntry memory in memories)
			{
				float score = ScoreSingleMemory(memory, features, weights, speaker, listener);
				list.Add(new ScoredItem<MemoryEntry>
				{
					Item = memory,
					Score = score,
					Breakdown = new ScoreBreakdown
					{
						ContextRelevance = CalculateContextRelevance(memory, features),
						Recency = CalculateRecency(memory),
						Importance = memory.Importance,
						Diversity = 0f,
						LayerPriority = GetLayerPriority(memory.Layer)
					}
				});
			}
			ApplyDiversityBoost(list);
			return list.OrderByDescending((ScoredItem<MemoryEntry> s) => s.Score).ToList();
		}

		public static List<ScoredItem<CommonKnowledgeEntry>> ScoreKnowledge(List<CommonKnowledgeEntry> knowledge, string context, Pawn speaker = null, Pawn listener = null)
		{
			if (knowledge == null || knowledge.Count == 0)
			{
				return new List<ScoredItem<CommonKnowledgeEntry>>();
			}
			ContextFeatures features = ExtractContextFeatures(context, speaker, listener);
			List<ScoredItem<CommonKnowledgeEntry>> list = new List<ScoredItem<CommonKnowledgeEntry>>();
			foreach (CommonKnowledgeEntry item in knowledge)
			{
				if (item.isEnabled)
				{
					float score = ScoreSingleKnowledge(item, features, speaker);
					list.Add(new ScoredItem<CommonKnowledgeEntry>
					{
						Item = item,
						Score = score,
						Breakdown = new ScoreBreakdown
						{
							ContextRelevance = CalculateKnowledgeRelevance(item, features),
							Importance = item.importance,
							Diversity = 0f
						}
					});
				}
			}
			ApplyDiversityBoost(list);
			return list.OrderByDescending((ScoredItem<CommonKnowledgeEntry> s) => s.Score).ToList();
		}

		private static ConversationSceneType IdentifyScene(string context)
		{
			if (string.IsNullOrEmpty(context))
			{
				return ConversationSceneType.Casual;
			}
			context = context.ToLower();
			if (ContainsAny(context, new string[7] { "袭击", "敌人", "危险", "受伤", "死", "快", "救" }))
			{
				return ConversationSceneType.Emergency;
			}
			if (ContainsAny(context, new string[6] { "过去", "以前", "曾经", "记得", "那时", "当时" }))
			{
				return ConversationSceneType.HistoryRecall;
			}
			if (ContainsAny(context, new string[7] { "感觉", "心情", "难过", "开心", "想", "喜欢", "讨厌" }))
			{
				return ConversationSceneType.EmotionalTalk;
			}
			if (ContainsAny(context, new string[6] { "工作", "任务", "建造", "种植", "研究", "搬运" }))
			{
				return ConversationSceneType.WorkDiscussion;
			}
			if (ContainsAny(context, new string[5] { "你是", "叫什么", "来自", "背景", "擅长" }))
			{
				return ConversationSceneType.Introduction;
			}
			return ConversationSceneType.Casual;
		}

		private static ScoringWeights AdjustWeightsForScene(ConversationSceneType scene, ScoringWeights baseWeights)
		{
			ScoringWeights scoringWeights = new ScoringWeights
			{
				ContextRelevance = baseWeights.ContextRelevance,
				Recency = baseWeights.Recency,
				Importance = baseWeights.Importance,
				Diversity = baseWeights.Diversity,
				LayerPriority = baseWeights.LayerPriority
			};
			switch (scene)
			{
			case ConversationSceneType.Emergency:
				scoringWeights.Recency = 0.5f;
				scoringWeights.ContextRelevance = 0.3f;
				scoringWeights.Importance = 0.15f;
				break;
			case ConversationSceneType.HistoryRecall:
				scoringWeights.Recency = 0.1f;
				scoringWeights.LayerPriority = 0.25f;
				scoringWeights.ContextRelevance = 0.45f;
				break;
			case ConversationSceneType.EmotionalTalk:
				scoringWeights.EmotionalBoost = 1.5f;
				scoringWeights.RelationshipBoost = 1.4f;
				break;
			case ConversationSceneType.WorkDiscussion:
				scoringWeights.ContextRelevance = 0.5f;
				scoringWeights.Recency = 0.25f;
				break;
			case ConversationSceneType.Introduction:
				scoringWeights.LayerPriority = 0.3f;
				scoringWeights.ContextRelevance = 0.35f;
				scoringWeights.Importance = 0.25f;
				break;
			}
			return scoringWeights;
		}

		private static ContextFeatures ExtractContextFeatures(string context, Pawn speaker, Pawn listener)
		{
			ContextFeatures contextFeatures = new ContextFeatures
			{
				Scene = IdentifyScene(context)
			};
			if (string.IsNullOrEmpty(context))
			{
				return contextFeatures;
			}
			contextFeatures.Keywords = ExtractImportantKeywords(context);
			if (speaker != null)
			{
				contextFeatures.Entities.Add(((Entity)speaker).LabelShort);
			}
			if (listener != null && listener != speaker)
			{
				contextFeatures.Entities.Add(((Entity)listener).LabelShort);
			}
			contextFeatures.Topics = ExtractTopics(context);
			contextFeatures.EmotionWords = ExtractEmotionWords(context);
			return contextFeatures;
		}

		private static List<string> ExtractImportantKeywords(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return new List<string>();
			}
			return (from k in SuperKeywordEngine.ExtractKeywords(text, 30)
				select k.Word).ToList();
		}

		private static List<string> ExtractTopics(string context)
		{
			List<string> list = new List<string>();
			if (ContainsAny(context, new string[4] { "工作", "任务", "建造", "种植" }))
			{
				list.Add("工作");
			}
			if (ContainsAny(context, new string[4] { "战斗", "袭击", "敌人", "武器" }))
			{
				list.Add("战斗");
			}
			if (ContainsAny(context, new string[4] { "聊天", "朋友", "关系", "喜欢" }))
			{
				list.Add("社交");
			}
			if (ContainsAny(context, new string[4] { "受伤", "治疗", "生病", "健康" }))
			{
				list.Add("健康");
			}
			return list;
		}

		private static HashSet<string> ExtractEmotionWords(string context)
		{
			HashSet<string> hashSet = new HashSet<string>();
			string[] array = new string[16]
			{
				"开心", "高兴", "快乐", "愉快", "难过", "悲伤", "伤心", "痛苦", "愤怒", "生气",
				"恼火", "讨厌", "害怕", "恐惧", "担心", "焦虑"
			};
			foreach (string text in array)
			{
				if (context.Contains(text))
				{
					hashSet.Add(text);
				}
			}
			return hashSet;
		}

		private static float ScoreSingleMemory(MemoryEntry memory, ContextFeatures features, ScoringWeights weights, Pawn speaker, Pawn listener)
		{
			float num = CalculateContextRelevance(memory, features);
			float num2 = 0f + num * weights.ContextRelevance;
			float num3 = CalculateRecency(memory);
			float num4 = num2 + num3 * weights.Recency + memory.Importance * weights.Importance;
			float layerPriority = GetLayerPriority(memory.Layer);
			float num5 = num4 + layerPriority * weights.LayerPriority;
			float typeBoost = GetTypeBoost(memory.Type, weights);
			float num6 = num5 * typeBoost;
			if (memory.IsPinned)
			{
				num6 *= 1.5f;
			}
			if (memory.IsUserEdited)
			{
				num6 *= 1.3f;
			}
			if (!string.IsNullOrEmpty(memory.relatedPawnName) && listener != null && memory.relatedPawnName == ((Entity)listener).LabelShort)
			{
				num6 *= 1.2f;
			}
			return num6;
		}

		private static float ScoreSingleKnowledge(CommonKnowledgeEntry entry, ContextFeatures features, Pawn speaker)
		{
			float num = CalculateKnowledgeRelevance(entry, features);
			return 0f + num * 0.6f + entry.importance * 0.4f;
		}

		private static float CalculateContextRelevance(MemoryEntry memory, ContextFeatures features)
		{
			float num = 0f;
			int num2 = 0;
			int num3 = 0;
			if (memory.keywords != null && memory.keywords.Count > 0)
			{
				int num4 = memory.keywords.Intersect(features.Keywords).Count();
				if (features.Keywords.Count > 0)
				{
					num += (float)num4 / (float)features.Keywords.Count * 0.4f;
					num2 += num4;
					num3 += features.Keywords.Count;
				}
			}
			foreach (string item in features.Keywords.Take(10))
			{
				if (memory.Content.Contains(item))
				{
					num += 0.1f;
				}
			}
			foreach (string topic in features.Topics)
			{
				if (memory.tags.Contains(topic) || memory.Content.Contains(topic))
				{
					num += 0.15f;
				}
			}
			if (memory.Type == MemoryType.Emotion)
			{
				foreach (string emotionWord in features.EmotionWords)
				{
					if (memory.Content.Contains(emotionWord))
					{
						num += 0.2f;
					}
				}
			}
			return Math.Min(num, 1f);
		}

		private static float CalculateKnowledgeRelevance(CommonKnowledgeEntry entry, ContextFeatures features)
		{
			float num = 0f;
			foreach (string tag in entry.GetTags())
			{
				if (GenCollection.Any<string>(features.Topics, (Predicate<string>)((string t) => t.Contains(tag) || tag.Contains(t))))
				{
					num += 0.3f;
				}
			}
			int num2 = 0;
			foreach (string item in features.Keywords.Take(15))
			{
				if (entry.content.Contains(item))
				{
					num2++;
				}
			}
			if (features.Keywords.Count > 0)
			{
				num += (float)num2 / (float)features.Keywords.Count * 0.5f;
			}
			return Math.Min(num, 1f);
		}

		private static float CalculateRecency(MemoryEntry memory)
		{
			int num = Find.TickManager.TicksGame - memory.GameTick;
			if (num < 2500)
			{
				return 1f;
			}
			if (num < 15000)
			{
				return 0.9f;
			}
			if (num < 60000)
			{
				return 0.7f;
			}
			if (num < 300000)
			{
				return 0.5f;
			}
			if (num < 900000)
			{
				return 0.3f;
			}
			return 0.1f;
		}

		private static float GetLayerPriority(MemoryLayer layer)
		{
			return layer switch
			{
				MemoryLayer.Active => 1f, 
				MemoryLayer.Situational => 0.8f, 
				MemoryLayer.EventLog => 0.5f, 
				MemoryLayer.Archive => 0.3f, 
				_ => 0f, 
			};
		}

		private static float GetTypeBoost(MemoryType type, ScoringWeights weights)
		{
			return type switch
			{
				MemoryType.Emotion => weights.EmotionalBoost, 
				MemoryType.Relationship => weights.RelationshipBoost, 
				MemoryType.Conversation => weights.ConversationBoost, 
				_ => 1f, 
			};
		}

		private static void ApplyDiversityBoost<T>(List<ScoredItem<T>> scored) where T : class
		{
			if (scored.Count == 0)
			{
				return;
			}
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			foreach (ScoredItem<T> item in scored)
			{
				string itemType = GetItemType(item.Item);
				if (!dictionary.ContainsKey(itemType))
				{
					dictionary[itemType] = 0;
				}
				dictionary[itemType]++;
				float num = 1f / (1f + (float)dictionary[itemType] * 0.1f);
				item.Breakdown.Diversity = num - 1f;
				item.Score *= num;
			}
		}

		private static string GetItemType(object item)
		{
			if (item is MemoryEntry memoryEntry)
			{
				return memoryEntry.Type.ToString();
			}
			if (item is CommonKnowledgeEntry commonKnowledgeEntry)
			{
				return commonKnowledgeEntry.GetTags().FirstOrDefault() ?? "unknown";
			}
			return "unknown";
		}

		private static bool ContainsAny(string text, string[] keywords)
		{
			foreach (string value in keywords)
			{
				if (text.Contains(value))
				{
					return true;
				}
			}
			return false;
		}
	}
	public class ScoredItem<T> where T : class
	{
		public T Item;

		public float Score;

		public ScoreBreakdown Breakdown;
	}
	public class ScoreBreakdown
	{
		public float ContextRelevance;

		public float Recency;

		public float Importance;

		public float Diversity;

		public float LayerPriority;

		public float TypeBoost;
	}
	public static class DynamicMemoryInjection
	{
		public static class Weights
		{
			public static float LayerBonus = 0.2f;

			public static float PinnedBonus = 0.5f;

			public static float UserEditedBonus = 0.3f;
		}

		private class ScoredMemory
		{
			public MemoryEntry Memory;

			public float Score;
		}

		public class MemoryScore
		{
			public MemoryEntry Memory;

			public float TotalScore;

			public float TimeScore;

			public float ImportanceScore;

			public float KeywordScore;

			public float BonusScore;
		}

		public static string InjectMemoriesWithDetails(FourLayerMemoryComp memoryComp, string context, int maxMemories, out List<MemoryScore> scores, MemoryLayer? layerA = null, MemoryLayer? layerB = null)
		{
			scores = new List<MemoryScore>();
			if (memoryComp == null)
			{
				return string.Empty;
			}
			ThingWithComps parent = ((ThingComp)memoryComp).parent;
			Pawn val = (Pawn)(object)((parent is Pawn) ? parent : null);
			if (val == null)
			{
				return string.Empty;
			}
			SceneType sceneType = DetermineScene(val, context);
			SceneAnalysisResult sceneAnalysisResult = SceneAnalyzer.AnalyzeScene(context);
			DynamicWeights sceneWeights = SceneAnalyzer.GetDynamicWeights(sceneType, sceneAnalysisResult.Confidence);
			List<string> contextKeywords = ExtractKeywords(context);
			List<MemoryEntry> list = new List<MemoryEntry>();
			if (IsMatch(MemoryLayer.Situational))
			{
				list.AddRange(memoryComp.SituationalMemories);
			}
			if (IsMatch(MemoryLayer.EventLog))
			{
				list.AddRange(memoryComp.EventLogMemories);
			}
			if (IsMatch(MemoryLayer.Archive) && (sceneType == SceneType.Social || sceneType == SceneType.Event || ShouldIncludeArchive(context)))
			{
				list.AddRange(memoryComp.ArchiveMemories.Take(20));
			}
			if (list.Count == 0)
			{
				return string.Empty;
			}
			float threshold = RimTalkMemoryPatchMod.Settings?.memoryScoreThreshold ?? 0.15f;
			List<ScoredMemory> list2 = (from m in list
				select new ScoredMemory
				{
					Memory = m,
					Score = CalculateMemoryScore(m, contextKeywords, sceneWeights, memoryComp)
				} into sm
				where sm.Score >= threshold
				orderby sm.Score descending
				select sm).Take(maxMemories).ToList();
			if (list2.Count == 0)
			{
				return null;
			}
			foreach (ScoredMemory item in list2)
			{
				float timeScore = CalculateTimeDecayScore(item.Memory, sceneWeights.TimeDecay, sceneWeights.RecencyWindow);
				float importanceScore = item.Memory.Importance * sceneWeights.Importance;
				float keywordScore = CalculateKeywordMatchScore(item.Memory, contextKeywords) * sceneWeights.KeywordMatch;
				float num = GetLayerBonus(item.Memory.Layer) * Weights.LayerBonus;
				float num2 = CalculateRelationshipBonus(item.Memory, val) * sceneWeights.RelationshipBonus;
				num += num2;
				if (item.Memory.IsPinned)
				{
					num += Weights.PinnedBonus;
				}
				if (item.Memory.IsUserEdited)
				{
					num += Weights.UserEditedBonus;
				}
				scores.Add(new MemoryScore
				{
					Memory = item.Memory,
					TotalScore = item.Score,
					TimeScore = timeScore,
					ImportanceScore = importanceScore,
					KeywordScore = keywordScore,
					BonusScore = num
				});
			}
			return string.Empty;
			bool IsMatch(MemoryLayer target)
			{
				if ((layerA ?? layerB).HasValue && layerA != target)
				{
					return layerB == target;
				}
				return true;
			}
		}

		private static SceneType DetermineScene(Pawn pawn, string context)
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Invalid comparison between Unknown and I4
			if (pawn == null)
			{
				return SceneType.Neutral;
			}
			if (pawn.Drafted || pawn.mindState?.enemyTarget != null)
			{
				return SceneType.Combat;
			}
			if (!pawn.Downed)
			{
				Pawn_HealthTracker health = pawn.health;
				if (health == null || (int)health.State != 1)
				{
					if (((Def)(pawn.CurJob?.def?)).defName != null && ((Def)pawn.CurJob.def).defName.Contains("Lovin"))
					{
						return SceneType.Social;
					}
					if (pawn.CurJob?.def != null)
					{
						string defName = ((Def)pawn.CurJob.def).defName;
						if (defName.Contains("Research"))
						{
							return SceneType.Research;
						}
						if (defName.Contains("Doctor") || defName.Contains("Tend") || defName.Contains("Surgery"))
						{
							return SceneType.Medical;
						}
						if (defName.Contains("Construct") || defName.Contains("Plant") || defName.Contains("Haul") || defName.Contains("Cook") || defName.Contains("Mine") || defName.Contains("Clean"))
						{
							return SceneType.Work;
						}
					}
					if (!string.IsNullOrEmpty(context))
					{
						return SceneAnalyzer.AnalyzeScene(context).PrimaryScene;
					}
					return SceneType.Neutral;
				}
			}
			return SceneType.Medical;
		}

		private static float CalculateMemoryScore(MemoryEntry memory, List<string> contextKeywords, DynamicWeights sceneWeights, FourLayerMemoryComp memoryComp)
		{
			float num = 0f;
			float num2 = CalculateTimeDecayScore(memory, sceneWeights.TimeDecay, sceneWeights.RecencyWindow);
			num += num2;
			num += memory.Importance * sceneWeights.Importance;
			float num3 = CalculateKeywordMatchScore(memory, contextKeywords);
			num += num3 * sceneWeights.KeywordMatch;
			float layerBonus = GetLayerBonus(memory.Layer);
			num += layerBonus * Weights.LayerBonus;
			ThingWithComps parent = ((ThingComp)memoryComp).parent;
			Pawn currentPawn = (Pawn)(object)((parent is Pawn) ? parent : null);
			float num4 = CalculateRelationshipBonus(memory, currentPawn);
			num += num4 * sceneWeights.RelationshipBonus;
			if (memory.IsPinned)
			{
				num += Weights.PinnedBonus;
			}
			if (memory.IsUserEdited)
			{
				num += Weights.UserEditedBonus;
			}
			return num + memory.Activity * 0.1f;
		}

		private static float CalculateTimeDecayScore(MemoryEntry memory, float decayRate, int recencyWindow)
		{
			int num = Find.TickManager.TicksGame - memory.GameTick;
			if (num > recencyWindow)
			{
				return Mathf.Exp((0f - (float)(num - recencyWindow) / 60000f) * 2f) * 0.1f;
			}
			return Mathf.Exp((0f - (float)num / 60000f) * decayRate);
		}

		private static float CalculateKeywordMatchScore(MemoryEntry memory, List<string> contextKeywords)
		{
			if (contextKeywords == null || contextKeywords.Count == 0)
			{
				return 0f;
			}
			if (memory.keywords == null || memory.keywords.Count == 0)
			{
				return 0f;
			}
			int num = memory.keywords.Intersect(contextKeywords).Count();
			int num2 = memory.keywords.Union(contextKeywords).Count();
			if (num2 == 0)
			{
				return 0f;
			}
			float num3 = (float)num / (float)num2;
			float num4 = 0f;
			foreach (string contextKeyword in contextKeywords)
			{
				if (memory.Content.Contains(contextKeyword))
				{
					num4 += 0.2f;
				}
			}
			return Mathf.Min(num3 + num4, 1f);
		}

		private static float CalculateRelationshipBonus(MemoryEntry memory, Pawn currentPawn)
		{
			if (string.IsNullOrEmpty(memory.relatedPawnId) || currentPawn == null)
			{
				return 0f;
			}
			Pawn relatedPawn = null;
			foreach (Map map in Find.Maps)
			{
				relatedPawn = GenCollection.FirstOrDefault<Pawn>(map.mapPawns.AllPawns, (Predicate<Pawn>)((Pawn p) => ((Thing)p).ThingID == memory.relatedPawnId || ((Entity)p).LabelShort == memory.relatedPawnName));
				if (relatedPawn != null)
				{
					break;
				}
			}
			if (relatedPawn == null)
			{
				return 0f;
			}
			if (currentPawn.relations == null || relatedPawn.relations == null)
			{
				return 0f;
			}
			List<DirectPawnRelation> list = currentPawn.relations.DirectRelations.Where((DirectPawnRelation r) => r.otherPawn == relatedPawn).ToList();
			if (GenCollection.Any<DirectPawnRelation>(list))
			{
				if (GenCollection.Any<DirectPawnRelation>(list, (Predicate<DirectPawnRelation>)((DirectPawnRelation r) => r.def == PawnRelationDefOf.Spouse || r.def == PawnRelationDefOf.Lover)))
				{
					return 1f;
				}
				if (GenCollection.Any<DirectPawnRelation>(list, (Predicate<DirectPawnRelation>)((DirectPawnRelation r) => r.def == PawnRelationDefOf.Parent || r.def == PawnRelationDefOf.Child || r.def == PawnRelationDefOf.Sibling)))
				{
					return 0.6f;
				}
				return 0.3f;
			}
			int num = currentPawn.relations.OpinionOf(relatedPawn);
			if (num > 50)
			{
				return 0.5f;
			}
			if (num > 20)
			{
				return 0.3f;
			}
			if (num < -20)
			{
				return 0.2f;
			}
			return 0f;
		}

		private static float GetLayerBonus(MemoryLayer layer)
		{
			return layer switch
			{
				MemoryLayer.Active => 1f, 
				MemoryLayer.Situational => 0.7f, 
				MemoryLayer.EventLog => 0.4f, 
				MemoryLayer.Archive => 0.2f, 
				_ => 0f, 
			};
		}

		private static bool ShouldIncludeArchive(string context)
		{
			if (string.IsNullOrEmpty(context))
			{
				return false;
			}
			string[] array = new string[8] { "过去", "以前", "曾经", "记得", "回忆", "历史", "当时", "那时候" };
			foreach (string value in array)
			{
				if (context.Contains(value))
				{
					return true;
				}
			}
			return false;
		}

		private static List<string> ExtractKeywords(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return new List<string>();
			}
			if (text.Length > 500)
			{
				text = text.Substring(0, 500);
			}
			List<WeightedKeyword> list = SuperKeywordEngine.ExtractKeywords(text);
			if (list.Count == 0)
			{
				return new List<string>();
			}
			List<WeightedKeyword> source = list.OrderByDescending((WeightedKeyword kw) => kw.Word.Length).ThenBy((WeightedKeyword kw) => kw.Word, StringComparer.Ordinal).ToList();
			List<WeightedKeyword> source2 = source.Take(10).ToList();
			List<WeightedKeyword> list2 = source.Skip(10).ToList();
			List<WeightedKeyword> source3 = new List<WeightedKeyword>();
			if (list2.Count > 0)
			{
				source3 = list2.OrderBy((WeightedKeyword kw) => kw.Word, StringComparer.Ordinal).Take(10).ToList();
			}
			List<string> list3 = new List<string>();
			list3.AddRange(source2.Select((WeightedKeyword kw) => kw.Word));
			list3.AddRange(source3.Select((WeightedKeyword kw) => kw.Word));
			return list3;
		}
	}
	public class FourLayerMemoryComp : ThingComp
	{
		private List<MemoryEntry> activeMemories = new List<MemoryEntry>();

		private List<MemoryEntry> situationalMemories = new List<MemoryEntry>();

		private List<MemoryEntry> eventLogMemories = new List<MemoryEntry>();

		private List<MemoryEntry> archiveMemories = new List<MemoryEntry>();

		private HashSet<long> _summarizedIds;

		private readonly JobMemoryCapturer _jobCapturer;

		private readonly CombatMemoryCapturer _combatCapturer;

		private readonly MemoryMaintainer _maintainer;

		private readonly MemorySummarizer _summarizer;

		private bool _summarizedIdsInitialized = true;

		private bool _alreadyUpdateSummarizationType = true;

		internal HashSet<long> SummarizedIds => _summarizedIds ?? (_summarizedIds = new HashSet<long>());

		public List<MemoryEntry> ActiveMemories => activeMemories;

		public List<MemoryEntry> SituationalMemories => situationalMemories;

		public List<MemoryEntry> EventLogMemories => eventLogMemories;

		public List<MemoryEntry> ArchiveMemories => archiveMemories;

		public JobMemoryCapturer JobCapturer => _jobCapturer;

		public CombatMemoryCapturer CombatCapturer => _combatCapturer;

		public MemoryMaintainer Maintainer => _maintainer;

		public MemorySummarizer Summarizer => _summarizer;

		public FourLayerMemoryComp()
		{
			_jobCapturer = new JobMemoryCapturer(this);
			_combatCapturer = new CombatMemoryCapturer(this);
			_maintainer = new MemoryMaintainer(this);
			_summarizer = new MemorySummarizer(this);
		}

		public override void PostExposeData()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Invalid comparison between Unknown and I4
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Invalid comparison between Unknown and I4
			if ((int)Scribe.mode == 1)
			{
				_summarizedIds?.IntersectWith(from m in activeMemories.Concat(situationalMemories).Concat(eventLogMemories).Concat(archiveMemories)
					where m != null
					select m.OriginId);
			}
			((ThingComp)this).PostExposeData();
			Scribe_Collections.Look<MemoryEntry>(ref activeMemories, "activeMemories", (LookMode)2, Array.Empty<object>());
			Scribe_Collections.Look<MemoryEntry>(ref situationalMemories, "situationalMemories", (LookMode)2, Array.Empty<object>());
			Scribe_Collections.Look<MemoryEntry>(ref eventLogMemories, "eventLogMemories", (LookMode)2, Array.Empty<object>());
			Scribe_Collections.Look<MemoryEntry>(ref archiveMemories, "archiveMemories", (LookMode)2, Array.Empty<object>());
			Scribe_Collections.Look<long>(ref _summarizedIds, "summarizedIds", (LookMode)1);
			Scribe_Values.Look<bool>(ref _summarizedIdsInitialized, "summarizedIdsInitialized", false, false);
			Scribe_Values.Look<bool>(ref _alreadyUpdateSummarizationType, "AlreadyUpdateSummarizationType", false, false);
			if (activeMemories == null)
			{
				activeMemories = new List<MemoryEntry>();
			}
			if (situationalMemories == null)
			{
				situationalMemories = new List<MemoryEntry>();
			}
			if (eventLogMemories == null)
			{
				eventLogMemories = new List<MemoryEntry>();
			}
			if (archiveMemories == null)
			{
				archiveMemories = new List<MemoryEntry>();
			}
			if ((int)Scribe.mode != 4)
			{
				return;
			}
			if (!_summarizedIdsInitialized)
			{
				SummarizedIds.UnionWith(from m in activeMemories.Concat(situationalMemories).Concat(eventLogMemories)
					where m != null
					select m.OriginId);
				_summarizedIdsInitialized = true;
			}
			if (_alreadyUpdateSummarizationType)
			{
				return;
			}
			foreach (MemoryEntry item in EventLogMemories.Concat(ArchiveMemories))
			{
				if (item != null)
				{
					item.Type = MemoryType.Summarization;
				}
			}
			_alreadyUpdateSummarizationType = true;
		}

		public override void CompTickRare()
		{
			((ThingComp)this).CompTickRare();
			if (Gen.IsHashIntervalTick((Thing)(object)base.parent, 2500))
			{
				_summarizer.DailySummarize();
				_maintainer.RunDecay();
				_maintainer.ConvertActiveMemories();
				_maintainer.CleanupLowActivityMemories();
			}
			if (Gen.IsHashIntervalTick((Thing)(object)base.parent, 60000))
			{
				_summarizer.PeriodicArchive();
				_maintainer.EnforceMemoryLimits();
			}
		}

		public List<MemoryEntry> RetrieveMemories(MemoryQuery query)
		{
			List<MemoryEntry> list = new List<MemoryEntry>();
			IOrderedEnumerable<MemoryEntry> collection = from m in activeMemories
				where MatchesQuery(m, query)
				orderby m.GameTick descending
				select m;
			list.AddRange(collection);
			if (situationalMemories.Count > 0)
			{
				IEnumerable<MemoryEntry> collection2 = (from m in situationalMemories
					where MatchesQuery(m, query)
					orderby m.CalculateRetrievalScore(null, query.keywords) descending, m.Id
					select m).Take(5);
				list.AddRange(collection2);
			}
			if (query.includeContext && list.Count < query.maxCount)
			{
				IEnumerable<MemoryEntry> collection3 = (from m in eventLogMemories
					where MatchesQuery(m, query)
					orderby m.CalculateRetrievalScore(null, query.keywords) descending, m.Id
					select m).Take(query.maxCount - list.Count);
				list.AddRange(collection3);
			}
			if (query.layer == MemoryLayer.Archive)
			{
				IEnumerable<MemoryEntry> collection4 = (from m in archiveMemories
					where MatchesQuery(m, query)
					orderby m.Importance descending, m.Id
					select m).Take(3);
				list.AddRange(collection4);
			}
			return list.Take(query.maxCount).ToList();
		}

		private bool MatchesQuery(MemoryEntry memory, MemoryQuery query)
		{
			if (query.type.HasValue && memory.Type != query.type.Value)
			{
				return false;
			}
			if (query.layer.HasValue && memory.Layer != query.layer.Value)
			{
				return false;
			}
			if (!string.IsNullOrEmpty(query.relatedPawn) && memory.relatedPawnName != query.relatedPawn)
			{
				return false;
			}
			if (GenCollection.Any<string>(query.tags) && !GenCollection.Any<string>(query.tags, (Predicate<string>)((string t) => memory.tags.Contains(t))))
			{
				return false;
			}
			return true;
		}

		public string GetMemoryContext(int count = 5)
		{
			MemoryQuery query = new MemoryQuery
			{
				maxCount = count,
				includeContext = true
			};
			List<MemoryEntry> list = RetrieveMemories(query);
			StringBuilder stringBuilder = new StringBuilder();
			foreach (MemoryEntry item in list)
			{
				stringBuilder.AppendLine("- [" + item.TypeName + "] " + item.Content + " (" + item.AgeString + ")");
			}
			return stringBuilder.ToString();
		}

		public List<MemoryEntry> GetRelevantMemories(int count = 5)
		{
			MemoryQuery query = new MemoryQuery
			{
				maxCount = count,
				includeContext = true
			};
			return RetrieveMemories(query);
		}
	}
	public enum MemoryLayer
	{
		Active,
		Situational,
		EventLog,
		Archive
	}
	public enum MemoryType
	{
		Conversation,
		[Obsolete("已废弃，保留以确保 enum 运作")]
		Interaction,
		Action,
		Summarization,
		Event,
		Emotion,
		Relationship,
		Internal
	}
	public static class MemoryTags
	{
		public const string 开心 = "开心";

		public const string 悲伤 = "悲伤";

		public const string 愤怒 = "愤怒";

		public const string 焦虑 = "焦虑";

		public const string 平静 = "平静";

		public const string 战斗 = "战斗";

		public const string 袭击 = "袭击";

		public const string 受伤 = "受伤";

		public const string 死亡 = "死亡";

		public const string 完成任务 = "完成任务";

		public const string 闲聊 = "闲聊";

		public const string 深谈 = "深谈";

		public const string 争吵 = "争吵";

		public const string 表白 = "表白";

		public const string 友好 = "友好";

		public const string 敌对 = "敌对";

		public const string 烹饪 = "烹饪";

		public const string 建造 = "建造";

		public const string 种植 = "种植";

		public const string 采矿 = "采矿";

		public const string 研究 = "研究";

		public const string 医疗 = "医疗";

		public const string 重要 = "重要";

		public const string 紧急 = "紧急";

		public const string 深度归档 = "深度归档";

		public const string 用户编辑 = "用户编辑";
	}
	public class MemoryEntry : IExposable
	{
		public long Id;

		private long _originId;

		public int GameTick = -1;

		private int _endGameTick = -1;

		public string Content;

		public MemoryType Type;

		public MemoryLayer Layer;

		private float _importance = -1f;

		public float Activity = -1f;

		public string relatedPawnId;

		public string relatedPawnName;

		public string location;

		public List<string> tags = new List<string>();

		public List<string> keywords = new List<string>();

		public bool IsUserEdited;

		public bool IsPinned;

		public string Notes;

		public long OriginId
		{
			get
			{
				if (_originId != 0L)
				{
					return _originId;
				}
				return Id;
			}
			set
			{
				_originId = value;
			}
		}

		public int EndGameTick
		{
			get
			{
				if (_endGameTick != -1)
				{
					return _endGameTick;
				}
				return GameTick;
			}
			set
			{
				_endGameTick = value;
			}
		}

		public float Importance
		{
			get
			{
				return _importance;
			}
			set
			{
				_importance = Math.Clamp(value, 0f, 1f);
			}
		}

		public string LayerName => Layer switch
		{
			MemoryLayer.Active => "超短期", 
			MemoryLayer.Situational => "短期", 
			MemoryLayer.EventLog => "中期", 
			MemoryLayer.Archive => "长期", 
			_ => "未知", 
		};

		public string TypeName => Type switch
		{
			MemoryType.Conversation => "对话", 
			MemoryType.Action => "行动", 
			MemoryType.Summarization => "总结", 
			MemoryType.Event => "事件", 
			MemoryType.Emotion => "情绪", 
			MemoryType.Relationship => "关系", 
			MemoryType.Internal => "内部", 
			_ => "未知", 
		};

		public bool ShouldBeCleaned
		{
			get
			{
				if (Activity < 0.025f)
				{
					return !IsPinned;
				}
				return false;
			}
		}

		public string AgeString
		{
			get
			{
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
				if (Layer == MemoryLayer.Archive)
				{
					return GenDate.DateMonthYearStringAt((long)GenDate.TickGameToAbs(GameTick), Vector2.zero);
				}
				TickManager tickManager = Find.TickManager;
				int? num = ((tickManager != null) ? new int?(tickManager.TicksGame) : ((int?)null)) - GameTick;
				if (!num.HasValue)
				{
					goto IL_00b6;
				}
				int valueOrDefault = num.GetValueOrDefault();
				string result;
				if (valueOrDefault >= 15000)
				{
					result = ((valueOrDefault < 120000) ? ((valueOrDefault >= 60000) ? "昨天" : "一天内") : ((valueOrDefault >= 180000) ? GenDate.DateFullStringAt((long)GenDate.TickGameToAbs(GameTick), Vector2.zero) : "前天"));
				}
				else
				{
					if (valueOrDefault < 0)
					{
						goto IL_00b6;
					}
					result = ((valueOrDefault >= 2500) ? "几小时前" : "刚刚");
				}
				goto IL_00fd;
				IL_00fd:
				return result;
				IL_00b6:
				result = "异常时间";
				goto IL_00fd;
			}
		}

		public MemoryEntry()
		{
		}

		public MemoryEntry(string content, MemoryType type, MemoryLayer layer, float importance = 0.5f, string relatedPawn = null)
		{
			Id = GenerateId();
			TickManager tickManager = Find.TickManager;
			GameTick = ((tickManager != null) ? tickManager.TicksGame : (-1));
			Content = content;
			Type = type;
			Layer = layer;
			Activity = 1f;
			Importance = importance;
			relatedPawnName = relatedPawn;
			AddTypeTag();
		}

		public virtual void ExposeData()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Invalid comparison between Unknown and I4
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Invalid comparison between Unknown and I4
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Invalid comparison between Unknown and I4
			if ((int)Scribe.mode == 1)
			{
				Scribe_Values.Look<long>(ref Id, "id", 0L, false);
			}
			if ((int)Scribe.mode == 2)
			{
				string serializedId = null;
				Scribe_Values.Look<string>(ref serializedId, "id", (string)null, false);
				Id = ParseId(serializedId);
			}
			Scribe_Values.Look<long>(ref _originId, "OriginId", 0L, false);
			Scribe_Values.Look<int>(ref GameTick, "timestamp", -1, false);
			Scribe_Values.Look<int>(ref _endGameTick, "EndGameTick", 0, false);
			Scribe_Values.Look<string>(ref Content, "content", (string)null, false);
			Scribe_Values.Look<MemoryType>(ref Type, "type", MemoryType.Conversation, false);
			Scribe_Values.Look<MemoryLayer>(ref Layer, "layer", MemoryLayer.Active, false);
			if ((int)Scribe.mode == 2 && Layer == MemoryLayer.Archive && EndGameTick == 0)
			{
				EndGameTick = GameTick + 900000;
			}
			Scribe_Values.Look<float>(ref _importance, "importance", -1f, false);
			Scribe_Values.Look<float>(ref Activity, "activity", -1f, false);
			Scribe_Values.Look<string>(ref relatedPawnId, "relatedPawnId", (string)null, false);
			Scribe_Values.Look<string>(ref relatedPawnName, "relatedPawnName", (string)null, false);
			Scribe_Values.Look<string>(ref location, "location", (string)null, false);
			Scribe_Collections.Look<string>(ref tags, "tags", (LookMode)1, Array.Empty<object>());
			Scribe_Collections.Look<string>(ref keywords, "keywords", (LookMode)1, Array.Empty<object>());
			Scribe_Values.Look<bool>(ref IsUserEdited, "isUserEdited", false, false);
			Scribe_Values.Look<bool>(ref IsPinned, "isPinned", false, false);
			Scribe_Values.Look<string>(ref Notes, "notes", (string)null, false);
			if (tags == null)
			{
				tags = new List<string>();
			}
			if (keywords == null)
			{
				keywords = new List<string>();
			}
		}

		private unsafe static long GenerateId()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			Span<byte> val = new Span<byte>((void*)stackalloc byte[8], 8);
			long num;
			do
			{
				RandomNumberGenerator.Fill(val);
				num = BitConverter.ToInt64(Span<byte>.op_Implicit(val)) & 0x7FFFFFFFFFFFFFFFL;
			}
			while (num == 0L);
			return num;
		}

		private static long ParseId(string serializedId)
		{
			if (serializedId == null)
			{
				Log.Warning("[RimTalk.Memory] 记忆 ID 为 null，已生成新 ID");
				return GenerateId();
			}
			if (long.TryParse(serializedId, NumberStyles.None, CultureInfo.InvariantCulture, out var result) && result > 0)
			{
				return result;
			}
			if (serializedId.StartsWith("mem-", StringComparison.Ordinal) && long.TryParse(serializedId.Substring("mem-".Length), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out result) && result > 0)
			{
				return result;
			}
			Log.Warning("[RimTalk.Memory] 记忆 ID \"" + serializedId + "\" 无效，已生成新 ID");
			return GenerateId();
		}

		private void AddTypeTag()
		{
			AddTag(Type switch
			{
				MemoryType.Conversation => "对话", 
				MemoryType.Action => "行动", 
				MemoryType.Summarization => "总结", 
				MemoryType.Event => "事件", 
				MemoryType.Emotion => "情绪", 
				MemoryType.Relationship => "关系", 
				MemoryType.Internal => "内部上下文", 
				_ => null, 
			});
		}

		public void AddTag(string tag)
		{
			if (!string.IsNullOrWhiteSpace(tag) && !tags.Contains(tag))
			{
				tags.Add(tag);
			}
		}

		public void RemoveTag(string tag)
		{
			if (!string.IsNullOrWhiteSpace(tag))
			{
				tags.Remove(tag);
			}
		}

		public void AddKeyword(string keyword)
		{
			if (!string.IsNullOrWhiteSpace(keyword) && !keywords.Contains(keyword))
			{
				keywords.Add(keyword);
			}
		}

		public void RemoveKeyword(string keyword)
		{
			if (!string.IsNullOrWhiteSpace(keyword))
			{
				keywords.Remove(keyword);
			}
		}

		public void Decay(float rate)
		{
			if (!IsPinned)
			{
				Activity *= 1f - rate;
			}
		}

		public float CalculateRetrievalScore(string context, List<string> contextKeywords)
		{
			float num = 0f;
			float num2 = (float)Math.Exp((0f - (float)(Find.TickManager.TicksGame - GameTick)) / 60000f);
			num += num2 * 0.3f;
			num += Importance * 0.3f;
			num += Activity * 0.2f;
			if (contextKeywords != null && contextKeywords.Count > 0)
			{
				int num3 = 0;
				foreach (string keyword in keywords)
				{
					if (contextKeywords.Contains(keyword))
					{
						num3++;
					}
				}
				float num4 = (float)num3 / (float)Math.Max(keywords.Count, contextKeywords.Count);
				num += num4 * 0.2f;
			}
			if (IsPinned)
			{
				num += 0.3f;
			}
			if (IsUserEdited)
			{
				num += 0.2f;
			}
			return num;
		}

		public virtual MemoryEntry Privatize()
		{
			return this;
		}
	}
	public class MemoryQuery
	{
		public MemoryLayer? layer;

		public MemoryType? type;

		public string relatedPawn;

		public List<string> tags;

		public List<string> keywords;

		public int maxCount = 10;

		public bool includeContext = true;

		public MemoryQuery()
		{
			tags = new List<string>();
			keywords = new List<string>();
		}
	}
	public class MemoryManager : WorldComponent
	{
		private int lastDecayTick;

		private const int DecayInterval = 2500;

		private int sessionStartTick = -1;

		private const int COLD_START_DELAY = 200;

		private CommonKnowledgeLibrary commonKnowledge;

		private PromptCache promptCache;

		public CommonKnowledgeLibrary CommonKnowledge
		{
			get
			{
				if (commonKnowledge == null)
				{
					commonKnowledge = new CommonKnowledgeLibrary();
				}
				return commonKnowledge;
			}
		}

		public PromptCache PromptCache
		{
			get
			{
				if (promptCache == null)
				{
					promptCache = new PromptCache();
				}
				return promptCache;
			}
		}

		static MemoryManager()
		{
		}

		public static CommonKnowledgeLibrary GetCommonKnowledge()
		{
			if (Current.Game == null)
			{
				return new CommonKnowledgeLibrary();
			}
			return Find.World.GetComponent<MemoryManager>()?.CommonKnowledge ?? new CommonKnowledgeLibrary();
		}

		public static PromptCache GetPromptCache()
		{
			if (Current.Game == null)
			{
				return new PromptCache();
			}
			return Find.World.GetComponent<MemoryManager>()?.PromptCache ?? new PromptCache();
		}

		public MemoryManager(World world)
			: base(world)
		{
			commonKnowledge = new CommonKnowledgeLibrary();
		}

		public override void WorldComponentTick()
		{
			((WorldComponent)this).WorldComponentTick();
			if (sessionStartTick == -1)
			{
				sessionStartTick = Find.TickManager.TicksGame;
			}
			if (Find.TickManager.TicksGame - sessionStartTick >= 200 && Find.TickManager.TicksGame - lastDecayTick >= 2500)
			{
				lastDecayTick = Find.TickManager.TicksGame;
				if (RimTalkMemoryPatchMod.Settings.enablePawnStatusKnowledge)
				{
					PawnStatusKnowledgeGenerator.UpdateAllColonistStatus();
				}
				PawnStatusKnowledgeGenerator.CleanupUpdateRecords();
			}
		}

		public override void ExposeData()
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Invalid comparison between Unknown and I4
			((WorldComponent)this).ExposeData();
			Scribe_Values.Look<int>(ref lastDecayTick, "lastDecayTick", 0, false);
			Scribe_Deep.Look<CommonKnowledgeLibrary>(ref commonKnowledge, "commonKnowledge", Array.Empty<object>());
			Scribe_Deep.Look<PromptCache>(ref promptCache, "promptCache", Array.Empty<object>());
			if ((int)Scribe.mode == 4)
			{
				if (commonKnowledge == null)
				{
					commonKnowledge = new CommonKnowledgeLibrary();
					Log.Warning("[RimTalk Memory] commonKnowledge was null, initialized new instance");
				}
				if (promptCache == null)
				{
					promptCache = new PromptCache();
					Log.Warning("[RimTalk Memory] promptCache was null, initialized new instance");
				}
				_ = GenDate.DaysPassed;
				Log.Message("[RimTalk Memory] MemoryManager loaded successfully.");
			}
		}

		private void UpdateEventKnowledgeTimePrefixes()
		{
			if (commonKnowledge == null || commonKnowledge.Entries == null)
			{
				return;
			}
			int ticksGame = Find.TickManager.TicksGame;
			int num = 0;
			foreach (CommonKnowledgeEntry entry in commonKnowledge.Entries)
			{
				if (entry.creationTick >= 0 && !string.IsNullOrEmpty(entry.originalEventText))
				{
					string content = entry.content;
					entry.UpdateEventTimePrefix(ticksGame);
					if (entry.content != content)
					{
						num++;
					}
				}
			}
		}

		private static bool IsColonyAnimalWithVocalLink(Pawn pawn)
		{
			if (pawn == null || ((Thing)pawn).Faction != Faction.OfPlayer)
			{
				return false;
			}
			RaceProperties raceProps = pawn.RaceProps;
			if (raceProps != null && raceProps.Humanlike)
			{
				return false;
			}
			try
			{
				HediffDef named = DefDatabase<HediffDef>.GetNamed("VocalLinkImplant", false);
				int result;
				if (named != null)
				{
					Pawn_HealthTracker health = pawn.health;
					if (health == null)
					{
						result = 0;
					}
					else
					{
						HediffSet hediffSet = health.hediffSet;
						result = ((((hediffSet != null) ? new bool?(hediffSet.HasHediff(named, false)) : ((bool?)null)) == true) ? 1 : 0);
					}
				}
				else
				{
					result = 0;
				}
				return (byte)result != 0;
			}
			catch
			{
				return false;
			}
		}
	}
	public class CompProperties_PawnMemory : CompProperties
	{
		public CompProperties_PawnMemory()
		{
			base.compClass = typeof(PawnMemoryComp);
		}
	}
	public class PawnMemoryComp : FourLayerMemoryComp
	{
	}
	public class PromptCache : IExposable
	{
		public class CacheEntry : IExposable
		{
			public string memoryPrompt;

			public string knowledgePrompt;

			public string fullPrompt;

			public int timestamp;

			public int lastUsedTick;

			public int useCount;

			public int pawnMemoryCount;

			public int knowledgeCount;

			public CacheEntry()
			{
			}

			public CacheEntry(string memoryPrompt, string knowledgePrompt, string fullPrompt, int pawnMemoryCount, int knowledgeCount)
			{
				this.memoryPrompt = memoryPrompt;
				this.knowledgePrompt = knowledgePrompt;
				this.fullPrompt = fullPrompt;
				timestamp = Find.TickManager.TicksGame;
				lastUsedTick = timestamp;
				useCount = 1;
				this.pawnMemoryCount = pawnMemoryCount;
				this.knowledgeCount = knowledgeCount;
			}

			public bool IsValid(int currentMemoryCount, int currentKnowledgeCount, int currentTick, int expireTicks)
			{
				if (Math.Abs(pawnMemoryCount - currentMemoryCount) > 5)
				{
					return false;
				}
				if (Math.Abs(knowledgeCount - currentKnowledgeCount) > 10)
				{
					return false;
				}
				if (currentTick - timestamp > expireTicks)
				{
					return false;
				}
				return true;
			}

			public void ExposeData()
			{
				Scribe_Values.Look<string>(ref memoryPrompt, "memoryPrompt", (string)null, false);
				Scribe_Values.Look<string>(ref knowledgePrompt, "knowledgePrompt", (string)null, false);
				Scribe_Values.Look<string>(ref fullPrompt, "fullPrompt", (string)null, false);
				Scribe_Values.Look<int>(ref timestamp, "timestamp", 0, false);
				Scribe_Values.Look<int>(ref lastUsedTick, "lastUsedTick", 0, false);
				Scribe_Values.Look<int>(ref useCount, "useCount", 0, false);
				Scribe_Values.Look<int>(ref pawnMemoryCount, "pawnMemoryCount", 0, false);
				Scribe_Values.Look<int>(ref knowledgeCount, "knowledgeCount", 0, false);
			}
		}

		private Dictionary<string, CacheEntry> cache = new Dictionary<string, CacheEntry>();

		private int totalHits;

		private int totalMisses;

		private int totalInvalidations;

		private int MaxCacheSize => RimTalkMemoryPatchMod.Settings.promptCacheSize;

		private int ExpireMinutes => RimTalkMemoryPatchMod.Settings.promptCacheExpireMinutes;

		public float HitRate
		{
			get
			{
				int num = totalHits + totalMisses;
				if (num == 0)
				{
					return 0f;
				}
				return (float)totalHits / (float)num;
			}
		}

		public CacheEntry TryGet(Pawn pawn, string context, out bool needsRegeneration)
		{
			needsRegeneration = true;
			if (!RimTalkMemoryPatchMod.Settings.enablePromptCache || pawn == null)
			{
				return null;
			}
			string key = GenerateCacheKey(pawn, context);
			if (cache.TryGetValue(key, out var value))
			{
				FourLayerMemoryComp memoryComp = ThingCompUtility.TryGetComp<FourLayerMemoryComp>((Thing)(object)pawn);
				int memoryCount = GetMemoryCount(memoryComp);
				int knowledgeCount = GetKnowledgeCount();
				int ticksGame = Find.TickManager.TicksGame;
				int expireTicks = ExpireMinutes * 2500;
				if (value.IsValid(memoryCount, knowledgeCount, ticksGame, expireTicks))
				{
					value.lastUsedTick = ticksGame;
					value.useCount++;
					totalHits++;
					needsRegeneration = false;
					return value;
				}
				cache.Remove(key);
				totalInvalidations++;
			}
			totalMisses++;
			return null;
		}

		public void Add(Pawn pawn, string context, string memoryPrompt, string knowledgePrompt, string fullPrompt)
		{
			if (RimTalkMemoryPatchMod.Settings.enablePromptCache && pawn != null)
			{
				FourLayerMemoryComp memoryComp = ThingCompUtility.TryGetComp<FourLayerMemoryComp>((Thing)(object)pawn);
				int memoryCount = GetMemoryCount(memoryComp);
				int knowledgeCount = GetKnowledgeCount();
				string key = GenerateCacheKey(pawn, context);
				CacheEntry value = new CacheEntry(memoryPrompt, knowledgePrompt, fullPrompt, memoryCount, knowledgeCount);
				cache[key] = value;
				if (cache.Count > MaxCacheSize)
				{
					EvictLRU();
				}
			}
		}

		public void InvalidateForPawn(Pawn pawn)
		{
			if (pawn == null)
			{
				return;
			}
			foreach (string item in cache.Keys.Where((string k) => k.StartsWith(((Thing)pawn).ThingID + "_")).ToList())
			{
				cache.Remove(item);
			}
		}

		public void Clear()
		{
			int count = cache.Count;
			cache.Clear();
			totalHits = 0;
			totalMisses = 0;
			totalInvalidations = 0;
			Log.Message($"[Prompt Cache] ??? Cleared {count} cached prompts");
		}

		public void CleanExpired()
		{
			int currentTick = Find.TickManager.TicksGame;
			int expireTicks = ExpireMinutes * 2500;
			foreach (string item in (from kvp in cache
				where currentTick - kvp.Value.timestamp > expireTicks
				select kvp.Key).ToList())
			{
				cache.Remove(item);
			}
		}

		public string GetStats()
		{
			return $"Cached: {cache.Count}/{MaxCacheSize}, Hits: {totalHits}, Misses: {totalMisses}, " + $"Invalidations: {totalInvalidations}, Hit Rate: {HitRate:P1}";
		}

		private string GenerateCacheKey(Pawn pawn, string context)
		{
			string text = GetStableHash(context).ToString();
			return ((Thing)pawn).ThingID + "_" + text;
		}

		private int GetMemoryCount(FourLayerMemoryComp memoryComp)
		{
			if (memoryComp == null)
			{
				return 0;
			}
			return memoryComp.SituationalMemories.Count + memoryComp.EventLogMemories.Count + memoryComp.ArchiveMemories.Count;
		}

		private int GetKnowledgeCount()
		{
			World world = Find.World;
			return (((world != null) ? world.GetComponent<MemoryManager>() : null)?.CommonKnowledge?.Entries?.Count).GetValueOrDefault();
		}

		private int GetStableHash(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return 0;
			}
			return ((text.Length > 100) ? text.Substring(0, 100) : text).GetHashCode();
		}

		private void EvictLRU()
		{
			if (cache.Count == 0)
			{
				return;
			}
			string text = null;
			int num = int.MaxValue;
			int num2 = int.MaxValue;
			foreach (KeyValuePair<string, CacheEntry> item in cache)
			{
				if (item.Value.useCount < num || (item.Value.useCount == num && item.Value.lastUsedTick < num2))
				{
					num = item.Value.useCount;
					num2 = item.Value.lastUsedTick;
					text = item.Key;
				}
			}
			if (text != null)
			{
				cache.Remove(text);
			}
		}

		private int EstimateComputeCost()
		{
			return Random.Range(8, 15);
		}

		public void ExposeData()
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Invalid comparison between Unknown and I4
			Scribe_Collections.Look<string, CacheEntry>(ref cache, "promptCache", (LookMode)1, (LookMode)2);
			Scribe_Values.Look<int>(ref totalHits, "totalHits", 0, false);
			Scribe_Values.Look<int>(ref totalMisses, "totalMisses", 0, false);
			Scribe_Values.Look<int>(ref totalInvalidations, "totalInvalidations", 0, false);
			if ((int)Scribe.mode == 4 && cache == null)
			{
				cache = new Dictionary<string, CacheEntry>();
			}
		}
	}
	public static class PromptNormalizer
	{
		private sealed class NormalizerSnapshot
		{
			public readonly List<RimTalkMemoryPatchSettings.ReplacementRule> Rules;

			public readonly Dictionary<string, Regex> Cache;

			public NormalizerSnapshot(List<RimTalkMemoryPatchSettings.ReplacementRule> rules, Dictionary<string, Regex> cache)
			{
				Rules = rules;
				Cache = cache;
			}
		}

		private static volatile NormalizerSnapshot _snapshot = new NormalizerSnapshot(new List<RimTalkMemoryPatchSettings.ReplacementRule>(), new Dictionary<string, Regex>());

		public static void UpdateRules(List<RimTalkMemoryPatchSettings.ReplacementRule> rules)
		{
			List<RimTalkMemoryPatchSettings.ReplacementRule> list = ((rules == null) ? new List<RimTalkMemoryPatchSettings.ReplacementRule>() : rules.Where((RimTalkMemoryPatchSettings.ReplacementRule r) => r?.isEnabled ?? false).ToList());
			Dictionary<string, Regex> dictionary = new Dictionary<string, Regex>();
			foreach (RimTalkMemoryPatchSettings.ReplacementRule item in list)
			{
				if (!string.IsNullOrEmpty(item.pattern))
				{
					try
					{
						Regex value = new Regex(item.pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
						dictionary[item.pattern] = value;
					}
					catch (Exception ex)
					{
						Log.Warning("[PromptNormalizer] Invalid regex pattern '" + item.pattern + "': " + ex.Message);
					}
				}
			}
			_snapshot = new NormalizerSnapshot(list, dictionary);
		}

		public static string Normalize(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}
			NormalizerSnapshot snapshot = _snapshot;
			if (snapshot.Rules.Count == 0)
			{
				return text;
			}
			string text2 = text;
			foreach (RimTalkMemoryPatchSettings.ReplacementRule rule in snapshot.Rules)
			{
				if (string.IsNullOrEmpty(rule.pattern) || rule.replacement == null)
				{
					continue;
				}
				try
				{
					if (snapshot.Cache.TryGetValue(rule.pattern, out var value))
					{
						text2 = value.Replace(text2, rule.replacement);
					}
				}
				catch (Exception ex)
				{
					Log.Warning("[PromptNormalizer] Error applying rule '" + rule.pattern + "': " + ex.Message);
				}
			}
			return text2;
		}

		public static int GetActiveRuleCount()
		{
			return _snapshot.Rules.Count;
		}
	}
	public class RoundMemory : MemoryEntry, IExposable
	{
		public long RoundMemoryUniqueID;

		private long AbsTick;

		private PlanetTile planetTile;

		public bool IsHomeMap;

		public HashSet<Pawn> Pawns;

		public RoundMemory()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			RoundMemoryUniqueID = -1L;
			AbsTick = -1L;
			planetTile = PlanetTile.Invalid;
			Pawns = new HashSet<Pawn>();
			base..ctor();
		}

		public RoundMemory(HashSet<Pawn> pawns, string content = null)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			RoundMemoryUniqueID = -1L;
			AbsTick = -1L;
			planetTile = PlanetTile.Invalid;
			Pawns = new HashSet<Pawn>();
			base..ctor(string.Empty, MemoryType.Conversation, MemoryLayer.Active);
			Pawns = pawns ?? new HashSet<Pawn>();
			Pawns.RemoveWhere((Pawn p) => p == null);
			RoundMemoryUniqueID = RoundMemoryManager.GetNewRoundMemoryId();
			TickManager tickManager = Find.TickManager;
			AbsTick = ((tickManager != null) ? tickManager.TicksAbs : (-1));
			if (Pawns.Count == 0)
			{
				Log.Warning("[RoundMemory] 创建RoundMemory时发现不存在对话参与者");
				return;
			}
			Pawn obj = Pawns.FirstOrDefault();
			planetTile = ((obj != null) ? ((Thing)obj).Tile : PlanetTile.Invalid);
			Map obj2 = Pawns.Select((Pawn p) => ((Thing)p).Map).FirstOrDefault((Map m) => m != null);
			IsHomeMap = obj2 != null && obj2.IsPlayerHome;
			Content = "[对话参与者: " + GetParticipantsRoster() + "]" + ((content == null) ? string.Empty : ("\n" + content));
		}

		public void AppendLine(string line)
		{
			if (!string.IsNullOrWhiteSpace(line))
			{
				Content = Content + "\n" + line;
			}
		}

		public override MemoryEntry Privatize()
		{
			return new MemoryEntry(string.Empty, Type, Layer, base.Importance)
			{
				OriginId = Id,
				Content = Content,
				GameTick = GameTick,
				EndGameTick = base.EndGameTick,
				relatedPawnId = relatedPawnId,
				relatedPawnName = relatedPawnName,
				location = location,
				tags = new List<string>(tags ?? new List<string>()),
				keywords = new List<string>(keywords ?? new List<string>()),
				IsUserEdited = IsUserEdited,
				IsPinned = IsPinned,
				Notes = Notes
			};
		}

		public string GetDateAndTime()
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			if (!((PlanetTile)(ref planetTile)).Valid || AbsTick == -1)
			{
				return "Unknown Date";
			}
			WorldGrid worldGrid = Find.WorldGrid;
			Vector2 val = ((worldGrid != null) ? worldGrid.LongLatOf(planetTile) : Vector2.zero);
			return GenDate.DateFullStringAt(AbsTick, val) + " " + GenDateExtension.GetInGameHour12HString(AbsTick, val);
		}

		public string GetParticipantsRoster()
		{
			return string.Join(", ", from p in Pawns
				select (p == null) ? null : ((Entity)p).LabelShort into n
				where n != null
				select n);
		}

		public override void ExposeData()
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Invalid comparison between Unknown and I4
			base.ExposeData();
			Scribe_Values.Look<long>(ref RoundMemoryUniqueID, "RoundMemoryUniqueID", -1L, false);
			Scribe_Values.Look<long>(ref AbsTick, "AbsTick", -1L, false);
			Scribe_Values.Look<PlanetTile>(ref planetTile, "Tile", PlanetTile.Invalid, false);
			Scribe_Values.Look<bool>(ref IsHomeMap, "IsHomeMap", false, false);
			Scribe_Collections.Look<Pawn>(ref Pawns, "Pawns", (LookMode)3);
			if (Pawns == null)
			{
				Log.Warning($"[RoundMemory] ExposeData for RoundMemory: tick={AbsTick}时发现其Pawns为空");
				Pawns = new HashSet<Pawn>();
			}
			if ((int)Scribe.mode == 4)
			{
				Pawns.RemoveWhere((Pawn p) => p == null);
			}
		}

		public string GetUniqueLoadID()
		{
			return $"RoundMemory_{RoundMemoryUniqueID}";
		}
	}
	public class RoundMemoryManager : GameComponent
	{
		private static RoundMemoryManager _instance;

		private const int MaxRoundMemory = 256;

		private RimRingBuffer<RoundMemory> _roundMemories = new RimRingBuffer<RoundMemory>(256);

		private List<RoundMemory> _tmpRoundMemories;

		private long _nextRoundMemoryId;

		private string _playerDialogue = string.Empty;

		private readonly ConditionalWeakTable<object, RoundMemory> _dictToRoundMemory = new ConditionalWeakTable<object, RoundMemory>();

		public static RoundMemoryManager Instance
		{
			get
			{
				if (Current.Game == null)
				{
					_instance = null;
					Log.Error("[RoundMemory] 尝试在存档外访问RoundMemory中控台");
				}
				return _instance;
			}
		}

		public RimRingBuffer<RoundMemory> RoundMemories => _roundMemories;

		public RoundMemoryManager(Game game)
		{
			_instance = this;
		}

		public static long GetNewRoundMemoryId()
		{
			if (Instance == null)
			{
				Log.Error("[RoundMemory] 警告：发号时发现RoundMemory中控台不存在，返回-1");
				return -1L;
			}
			return Interlocked.Increment(ref Instance._nextRoundMemoryId);
		}

		public static void CapturePlayerDialogue(Pawn playerPawn, string playerDialogue)
		{
			if (Instance == null)
			{
				Log.Error("[RoundMemory] 警告：捕获玩家对话时发现RoundMemory中控台不存在");
				return;
			}
			string text = ((playerPawn != null) ? ((Entity)playerPawn).LabelShort : null);
			Instance._playerDialogue = (string.IsNullOrWhiteSpace(text) ? "Player" : text) + ": " + playerDialogue;
		}

		public static void StreamingBuildRoundMemory<T>(T talkRequest, string content, IEnumerable<Pawn> participants = null, bool isPlayerInitiate = false) where T : class
		{
			if (Instance == null)
			{
				Log.Error("[RoundMemory] 警告：构建轮次记忆时发现RoundMemory中控台不存在");
				return;
			}
			ConditionalWeakTable<object, RoundMemory> dictToRoundMemory = Instance._dictToRoundMemory;
			if (!dictToRoundMemory.TryGetValue(talkRequest, out var value))
			{
				if (participants == null || !participants.Any())
				{
					Log.Warning("[RoundMemory] 流式创建新轮次记忆时未提供参与者，创建失败");
					return;
				}
				string content2 = null;
				if (isPlayerInitiate)
				{
					RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
					if (settings == null || settings.IsPlayerDialogueInject)
					{
						content2 = Instance._playerDialogue;
					}
				}
				HashSet<Pawn> pawns = participants.ToHashSet();
				value = new RoundMemory(pawns, content2);
				dictToRoundMemory.AddOrUpdate((object)talkRequest, value);
				AddRoundMemory(pawns, value);
			}
			value?.AppendLine(content);
		}

		public static void BuildRoundMemory(HashSet<Pawn> pawns, string content)
		{
			if (Instance == null)
			{
				Log.Error("[RoundMemory] 警告：构建轮次记忆时发现RoundMemory中控台不存在");
				return;
			}
			RoundMemory roundMemory = new RoundMemory(pawns, content);
			AddRoundMemory(pawns, roundMemory);
		}

		private static void AddRoundMemory(HashSet<Pawn> pawns, RoundMemory roundMemory)
		{
			Instance._roundMemories.Add(roundMemory);
			if (pawns == null)
			{
				return;
			}
			foreach (Pawn pawn in pawns)
			{
				if (pawn != null)
				{
					ThingCompUtility.TryGetComp<FourLayerMemoryComp>((Thing)(object)pawn)?.ActiveMemories?.Add(roundMemory);
				}
			}
		}

		public override void ExposeData()
		{
			((GameComponent)this).ExposeData();
			Scribe_Values.Look<long>(ref _nextRoundMemoryId, "NextRoundMemoryId", 0L, false);
			LookRoundMemories();
		}

		private void LookRoundMemories()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Invalid comparison between Unknown and I4
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Invalid comparison between Unknown and I4
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Invalid comparison between Unknown and I4
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Invalid comparison between Unknown and I4
			if ((int)Scribe.mode == 1)
			{
				_tmpRoundMemories = new List<RoundMemory>();
				for (int i = 0; i < _roundMemories.Count; i++)
				{
					_tmpRoundMemories.Add(_roundMemories[i]);
				}
			}
			Scribe_Collections.Look<RoundMemory>(ref _tmpRoundMemories, "RoundMemories", (LookMode)2, Array.Empty<object>());
			if ((int)Scribe.mode == 4)
			{
				if (_tmpRoundMemories == null)
				{
					return;
				}
				foreach (RoundMemory tmpRoundMemory in _tmpRoundMemories)
				{
					_roundMemories.Add(tmpRoundMemory);
				}
			}
			if ((int)Scribe.mode == 1 || (int)Scribe.mode == 4)
			{
				_tmpRoundMemories = null;
			}
		}

		public override void FinalizeInit()
		{
			Dictionary<long, RoundMemory> dictionary = new Dictionary<long, RoundMemory>();
			if (_roundMemories == null)
			{
				Log.Warning("[RoundMemory] RoundMemory为空，无法进行指针修正");
				return;
			}
			for (int i = 0; i < _roundMemories.Count; i++)
			{
				RoundMemory roundMemory = _roundMemories[i];
				if (roundMemory == null)
				{
					Log.Warning("[RoundMemory] 检测到RoundMemory中有 null 条目，跳过");
				}
				else
				{
					dictionary[roundMemory.RoundMemoryUniqueID] = roundMemory;
				}
			}
			List<Pawn> all_AliveOrDead = PawnsFinder.All_AliveOrDead;
			if (all_AliveOrDead == null)
			{
				return;
			}
			foreach (Pawn item in all_AliveOrDead)
			{
				FourLayerMemoryComp fourLayerMemoryComp = ((item != null) ? ThingCompUtility.TryGetComp<FourLayerMemoryComp>((Thing)(object)item) : null);
				if (fourLayerMemoryComp == null)
				{
					continue;
				}
				List<MemoryEntry> activeMemories = fourLayerMemoryComp.ActiveMemories;
				if (activeMemories == null || activeMemories.Count == 0)
				{
					continue;
				}
				for (int j = 0; j < activeMemories.Count; j++)
				{
					MemoryEntry memoryEntry = activeMemories[j];
					if (memoryEntry != null && memoryEntry is RoundMemory roundMemory2 && dictionary.TryGetValue(roundMemory2.RoundMemoryUniqueID, out var value) && roundMemory2 != value)
					{
						activeMemories[j] = value;
					}
				}
			}
		}
	}
	public enum SceneType
	{
		Combat,
		Social,
		Work,
		Medical,
		Research,
		Event,
		Neutral
	}
	public static class SceneAnalyzer
	{
		private static readonly Dictionary<SceneType, List<string>> SceneKeywords = new Dictionary<SceneType, List<string>>
		{
			{
				SceneType.Combat,
				new List<string>
				{
					"袭击", "raid", "攻击", "attack", "战斗", "combat", "fight", "敌人", "enemy", "入侵",
					"invasion", "防御", "defense", "受伤", "injured", "伤势", "wound", "死亡", "death", "died",
					"killed", "流血", "bleeding", "倒下", "downed", "昏迷", "unconscious", "武器", "weapon", "枪",
					"gun", "射击", "shoot", "爆炸", "explosion", "掩体", "cover", "撤退", "retreat", "增援",
					"reinforcement"
				}
			},
			{
				SceneType.Social,
				new List<string>
				{
					"聊天", "chat", "talk", "对话", "conversation", "说", "said", "告诉", "told", "询问",
					"asked", "回答", "replied", "喜欢", "like", "爱", "love", "讨厌", "hate", "朋友",
					"friend", "恋人", "lover", "配偶", "spouse", "关系", "relationship", "友谊", "friendship", "争吵",
					"argument", "和解", "reconcile", "开心", "happy", "快乐", "joyful", "悲伤", "sad", "难过",
					"upset", "愤怒", "angry", "生气", "mad", "焦虑", "anxious", "心情", "mood", "感觉",
					"feel", "情绪", "emotion"
				}
			},
			{
				SceneType.Work,
				new List<string>
				{
					"建造", "construct", "建筑", "building", "制作", "craft", "制造", "manufacture", "修理", "repair",
					"拆除", "deconstruct", "种植", "plant", "收获", "harvest", "农作物", "crop", "田地", "field",
					"采矿", "mining", "挖掘", "dig", "搬运", "haul", "运输", "transport", "仓库", "storage",
					"清洁", "clean", "烹饪", "cook", "cooking", "做饭", "meal", "食物", "food"
				}
			},
			{
				SceneType.Medical,
				new List<string>
				{
					"治疗", "treat", "医疗", "medical", "手术", "surgery", "操作", "operation", "包扎", "bandage",
					"照顾", "tend", "护理", "care", "疾病", "disease", "illness", "感染", "infection", "发烧",
					"fever", "疼痛", "pain", "残疾", "disability", "健康", "health", "恢复", "recover", "痊愈",
					"heal", "康复", "rehabilitation"
				}
			},
			{
				SceneType.Research,
				new List<string>
				{
					"研究", "research", "科技", "technology", "发明", "invention", "突破", "breakthrough", "发现", "discovery",
					"实验", "experiment", "学习", "learn", "训练", "train", "练习", "practice", "技能", "skill",
					"提升", "improve", "掌握", "master", "知识", "knowledge", "教导", "teach"
				}
			},
			{
				SceneType.Event,
				new List<string>
				{
					"婚礼", "wedding", "结婚", "marry", "订婚", "engaged", "生日", "birthday", "庆祝", "celebrate",
					"派对", "party", "葬礼", "funeral", "埋葬", "burial", "纪念", "memorial", "仪式", "ceremony",
					"节日", "festival", "活动", "event"
				}
			}
		};

		private static readonly Dictionary<SceneType, float> ScenePriority = new Dictionary<SceneType, float>
		{
			{
				SceneType.Combat,
				1f
			},
			{
				SceneType.Medical,
				0.9f
			},
			{
				SceneType.Event,
				0.85f
			},
			{
				SceneType.Social,
				0.7f
			},
			{
				SceneType.Research,
				0.6f
			},
			{
				SceneType.Work,
				0.5f
			},
			{
				SceneType.Neutral,
				0.3f
			}
		};

		public static SceneAnalysisResult AnalyzeScene(string context)
		{
			if (string.IsNullOrEmpty(context))
			{
				return new SceneAnalysisResult
				{
					PrimaryScene = SceneType.Neutral,
					SceneScores = new Dictionary<SceneType, float> { 
					{
						SceneType.Neutral,
						1f
					} },
					Confidence = 0f
				};
			}
			string lowerContext = context.ToLower();
			Dictionary<SceneType, float> dictionary = new Dictionary<SceneType, float>();
			foreach (KeyValuePair<SceneType, List<string>> sceneKeyword in SceneKeywords)
			{
				SceneType key = sceneKeyword.Key;
				List<string> value = sceneKeyword.Value;
				int num = GenCollection.Count<string>(value, (Predicate<string>)((string kw) => lowerContext.Contains(kw.ToLower())));
				float num2 = ((num > 0) ? ((float)num / (float)value.Count) : 0f);
				num2 *= ScenePriority[key];
				if (num2 > 0f)
				{
					dictionary[key] = num2;
				}
			}
			if (dictionary.Count == 0)
			{
				return new SceneAnalysisResult
				{
					PrimaryScene = SceneType.Neutral,
					SceneScores = new Dictionary<SceneType, float> { 
					{
						SceneType.Neutral,
						1f
					} },
					Confidence = 0f
				};
			}
			float totalScore = dictionary.Values.Sum();
			Dictionary<SceneType, float> dictionary2 = dictionary.ToDictionary((KeyValuePair<SceneType, float> kvp) => kvp.Key, (KeyValuePair<SceneType, float> kvp) => kvp.Value / totalScore);
			KeyValuePair<SceneType, float> keyValuePair = dictionary2.OrderByDescending((KeyValuePair<SceneType, float> kvp) => kvp.Value).First();
			float value2 = keyValuePair.Value;
			return new SceneAnalysisResult
			{
				PrimaryScene = keyValuePair.Key,
				SceneScores = dictionary2,
				Confidence = value2
			};
		}

		public static DynamicWeights GetDynamicWeights(SceneType scene, float confidence = 1f)
		{
			DynamicWeights dynamicWeights = new DynamicWeights();
			switch (scene)
			{
			case SceneType.Combat:
				dynamicWeights.TimeDecay = 0.8f;
				dynamicWeights.Importance = 0.5f;
				dynamicWeights.KeywordMatch = 0.4f;
				dynamicWeights.RelationshipBonus = 0.1f;
				dynamicWeights.RecencyWindow = 15000;
				break;
			case SceneType.Social:
				dynamicWeights.TimeDecay = 0.05f;
				dynamicWeights.Importance = 0.2f;
				dynamicWeights.KeywordMatch = 0.25f;
				dynamicWeights.RelationshipBonus = 0.6f;
				dynamicWeights.RecencyWindow = 1800000;
				break;
			case SceneType.Work:
				dynamicWeights.TimeDecay = 0.3f;
				dynamicWeights.Importance = 0.3f;
				dynamicWeights.KeywordMatch = 0.35f;
				dynamicWeights.RelationshipBonus = 0.15f;
				dynamicWeights.RecencyWindow = 180000;
				break;
			case SceneType.Medical:
				dynamicWeights.TimeDecay = 0.15f;
				dynamicWeights.Importance = 0.45f;
				dynamicWeights.KeywordMatch = 0.35f;
				dynamicWeights.RelationshipBonus = 0.2f;
				dynamicWeights.RecencyWindow = 420000;
				break;
			case SceneType.Research:
				dynamicWeights.TimeDecay = 0.02f;
				dynamicWeights.Importance = 0.4f;
				dynamicWeights.KeywordMatch = 0.4f;
				dynamicWeights.RelationshipBonus = 0.1f;
				dynamicWeights.RecencyWindow = 3600000;
				break;
			case SceneType.Event:
				dynamicWeights.TimeDecay = 0.1f;
				dynamicWeights.Importance = 0.5f;
				dynamicWeights.KeywordMatch = 0.3f;
				dynamicWeights.RelationshipBonus = 0.4f;
				dynamicWeights.RecencyWindow = 900000;
				break;
			default:
				dynamicWeights.TimeDecay = 0.25f;
				dynamicWeights.Importance = 0.3f;
				dynamicWeights.KeywordMatch = 0.3f;
				dynamicWeights.RelationshipBonus = 0.25f;
				dynamicWeights.RecencyWindow = 240000;
				break;
			}
			if (confidence < 0.6f)
			{
				float t = 1f - confidence;
				DynamicWeights dynamicWeights2 = GetDynamicWeights(SceneType.Neutral);
				dynamicWeights.TimeDecay = Lerp(dynamicWeights.TimeDecay, dynamicWeights2.TimeDecay, t);
				dynamicWeights.Importance = Lerp(dynamicWeights.Importance, dynamicWeights2.Importance, t);
				dynamicWeights.KeywordMatch = Lerp(dynamicWeights.KeywordMatch, dynamicWeights2.KeywordMatch, t);
				dynamicWeights.RelationshipBonus = Lerp(dynamicWeights.RelationshipBonus, dynamicWeights2.RelationshipBonus, t);
			}
			return dynamicWeights;
		}

		private static float Lerp(float a, float b, float t)
		{
			return a + (b - a) * t;
		}

		public static string GetSceneDisplayName(SceneType scene)
		{
			return scene switch
			{
				SceneType.Combat => "战斗/紧急", 
				SceneType.Social => "社交/情感", 
				SceneType.Work => "工作/日常", 
				SceneType.Medical => "医疗/健康", 
				SceneType.Research => "研究/学习", 
				SceneType.Event => "特殊事件", 
				SceneType.Neutral => "中性/通用", 
				_ => "未知", 
			};
		}
	}
	public class SceneAnalysisResult
	{
		public SceneType PrimaryScene { get; set; }

		public Dictionary<SceneType, float> SceneScores { get; set; }

		public float Confidence { get; set; }

		public override string ToString()
		{
			return $"{SceneAnalyzer.GetSceneDisplayName(PrimaryScene)} (置信度: {Confidence:P0})";
		}
	}
	public class DynamicWeights
	{
		public float TimeDecay { get; set; }

		public float Importance { get; set; }

		public float KeywordMatch { get; set; }

		public float RelationshipBonus { get; set; }

		public int RecencyWindow { get; set; }

		public override string ToString()
		{
			return $"TimeDecay:{TimeDecay:F2} Importance:{Importance:F2} Keyword:{KeywordMatch:F2} " + $"Relationship:{RelationshipBonus:F2} Window:{RecencyWindow / 60000}days";
		}
	}
	public static class SemanticScoringSystem
	{
		public static List<ScoredItem<MemoryEntry>> ScoreMemoriesWithSemantics(List<MemoryEntry> memories, string context, Pawn speaker = null, Pawn listener = null)
		{
			if (memories == null || memories.Count == 0)
			{
				return new List<ScoredItem<MemoryEntry>>();
			}
			List<ScoredItem<MemoryEntry>> list = AdvancedScoringSystem.ScoreMemories(memories, context, speaker, listener);
			int count = Math.Max(10, list.Count / 2);
			List<ScoredItem<MemoryEntry>> topCandidates = list.Take(count).ToList();
			if (0 == 0 || !EmbeddingService.IsAvailable())
			{
				return topCandidates;
			}
			Task<List<ScoredItem<MemoryEntry>>> task = Task.Run(async () => await ApplySemanticScoringAsync(topCandidates, context));
			if (task.Wait(800))
			{
				List<ScoredItem<MemoryEntry>> result = task.Result;
				if (result != null && result.Count > 0)
				{
					return result;
				}
			}
			else if (Prefs.DevMode && Random.value < 0.1f)
			{
				Log.Warning("[Semantic Scoring] Timeout, using keyword fallback");
			}
			return topCandidates;
		}

		private static async Task<List<ScoredItem<MemoryEntry>>> ApplySemanticScoringAsync(List<ScoredItem<MemoryEntry>> candidates, string context)
		{
			_ = 1;
			try
			{
				float[] contextEmbedding = await EmbeddingService.GetEmbeddingAsync(context);
				if (contextEmbedding == null)
				{
					Log.Warning("[Semantic Scoring] Failed to get context embedding");
					return candidates;
				}
				foreach (ScoredItem<MemoryEntry> scored in candidates)
				{
					try
					{
						float[] array = await EmbeddingService.GetEmbeddingAsync(scored.Item.Content);
						if (array != null)
						{
							float num = EmbeddingService.CosineSimilarity(contextEmbedding, array);
							float score = scored.Score * 0.7f + num * 0.3f;
							scored.Score = score;
							if (scored.Breakdown != null)
							{
								scored.Breakdown.TypeBoost = num;
							}
						}
					}
					catch (Exception ex)
					{
						Log.Warning("[Semantic Scoring] Failed to score memory: " + ex.Message);
					}
				}
				return candidates.OrderByDescending((ScoredItem<MemoryEntry> s) => s.Score).ToList();
			}
			catch (Exception ex2)
			{
				Log.Error("[Semantic Scoring] Error: " + ex2.Message);
				return candidates;
			}
		}

		public static List<ScoredItem<CommonKnowledgeEntry>> ScoreKnowledgeWithSemantics(List<CommonKnowledgeEntry> knowledge, string context, Pawn speaker = null, Pawn listener = null)
		{
			if (knowledge == null || knowledge.Count == 0)
			{
				return new List<ScoredItem<CommonKnowledgeEntry>>();
			}
			List<ScoredItem<CommonKnowledgeEntry>> list = AdvancedScoringSystem.ScoreKnowledge(knowledge, context, speaker, listener);
			int count = Math.Max(5, list.Count / 2);
			List<ScoredItem<CommonKnowledgeEntry>> topCandidates = list.Take(count).ToList();
			if (0 == 0 || !EmbeddingService.IsAvailable())
			{
				return topCandidates;
			}
			Task<List<ScoredItem<CommonKnowledgeEntry>>> task = Task.Run(async () => await ApplySemanticScoringToKnowledgeAsync(topCandidates, context));
			if (task.Wait(800))
			{
				List<ScoredItem<CommonKnowledgeEntry>> result = task.Result;
				if (result != null && result.Count > 0)
				{
					return result;
				}
			}
			else if (Prefs.DevMode && Random.value < 0.1f)
			{
				Log.Warning("[Semantic Scoring] Timeout, using keyword fallback");
			}
			return topCandidates;
		}

		private static async Task<List<ScoredItem<CommonKnowledgeEntry>>> ApplySemanticScoringToKnowledgeAsync(List<ScoredItem<CommonKnowledgeEntry>> candidates, string context)
		{
			_ = 1;
			try
			{
				float[] contextEmbedding = await EmbeddingService.GetEmbeddingAsync(context);
				if (contextEmbedding == null)
				{
					return candidates;
				}
				foreach (ScoredItem<CommonKnowledgeEntry> scored in candidates)
				{
					try
					{
						float[] array = await EmbeddingService.GetEmbeddingAsync(scored.Item.content);
						if (array != null)
						{
							float num = EmbeddingService.CosineSimilarity(contextEmbedding, array);
							float score = scored.Score * 0.6f + num * 0.4f;
							scored.Score = score;
						}
					}
					catch (Exception ex)
					{
						Log.Warning("[Semantic Scoring] Failed to score knowledge: " + ex.Message);
					}
				}
				return candidates.OrderByDescending((ScoredItem<CommonKnowledgeEntry> s) => s.Score).ToList();
			}
			catch (Exception ex2)
			{
				Log.Error("[Semantic Scoring] Error: " + ex2.Message);
				return candidates;
			}
		}

		public static async Task PrewarmEmbeddingCacheAsync(FourLayerMemoryComp memoryComp)
		{
			if (memoryComp == null || !EmbeddingService.IsAvailable())
			{
				return;
			}
			try
			{
				List<MemoryEntry> list = new List<MemoryEntry>();
				list.AddRange(memoryComp.SituationalMemories.Where((MemoryEntry m) => m.Importance > 0.7f));
				list.AddRange(memoryComp.EventLogMemories.Where((MemoryEntry m) => m.Importance > 0.7f));
				list.AddRange(from m in memoryComp.ArchiveMemories.Take(10)
					where m.Importance > 0.7f
					select m);
				if (list.Count != 0)
				{
					await EmbeddingService.GetEmbeddingsBatchAsync((from m in list.Take(20)
						select m.Content).ToList());
				}
			}
			catch (Exception ex)
			{
				Log.Error("[Semantic Scoring] Prewarm error: " + ex.Message);
			}
		}
	}
}
namespace RimTalk.Memory.VectorDB
{
	public class VectorService
	{
		private static VectorService _instance;

		private static readonly object _instanceLock = new object();

		private Dictionary<string, float[]> _loreVectors = new Dictionary<string, float[]>();

		private Dictionary<string, string> _contentHashes = new Dictionary<string, string>();

		private HttpClient _httpClient;

		private bool _isInitialized;

		private bool _isSyncing;

		public static VectorService Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_instanceLock)
					{
						if (_instance == null)
						{
							_instance = new VectorService();
						}
					}
				}
				return _instance;
			}
		}

		private VectorService()
		{
			Initialize();
		}

		private void Initialize()
		{
			try
			{
				Log.Message("[RimTalk-ExpandMemory] VectorService: Initializing Cloud Embedding Service...");
				_httpClient = new HttpClient();
				_httpClient.Timeout = TimeSpan.FromSeconds(30.0);
				_isInitialized = true;
				Log.Message("[RimTalk-ExpandMemory] VectorService: Cloud Service Initialized.");
			}
			catch (Exception arg)
			{
				Log.Error($"[RimTalk-ExpandMemory] VectorService: Initialization failed: {arg}");
				_isInitialized = false;
			}
		}

		public async Task<List<(string id, float similarity)>> FindBestLoreIdsAsync(string userMessage, int topK = 5, float threshold = 0.7f)
		{
			List<(string id, float similarity)> results = new List<(string, float)>();
			try
			{
				if (!_isInitialized)
				{
					Log.Warning("[RimTalk-ExpandMemory] VectorService: Service not initialized.");
					return results;
				}
				if (string.IsNullOrWhiteSpace(userMessage))
				{
					return results;
				}
				float[] array = await GetEmbeddingAsync(userMessage).ConfigureAwait(continueOnCapturedContext: false);
				if (array == null || array.Length == 0)
				{
					return results;
				}
				List<(string, float)> list = new List<(string, float)>();
				lock (_loreVectors)
				{
					foreach (KeyValuePair<string, float[]> loreVector in _loreVectors)
					{
						float num = CosineSimilarity(array, loreVector.Value);
						if (num >= threshold)
						{
							list.Add((loreVector.Key, num));
						}
					}
				}
				results = list.OrderByDescending<(string, float), float>(((string id, float similarity) s) => s.similarity).Take(topK).ToList();
				return results;
			}
			catch (Exception arg)
			{
				Log.Error($"[RimTalk-ExpandMemory] VectorService: Error in FindBestLoreIdsAsync: {arg}");
				return results;
			}
		}

		[Obsolete("Use FindBestLoreIdsAsync instead to avoid blocking")]
		public List<(string id, float similarity)> FindBestLoreIds(string userMessage, int topK = 5, float threshold = 0.7f)
		{
			return FindBestLoreIdsAsync(userMessage, topK, threshold).GetAwaiter().GetResult();
		}

		public void SyncKnowledgeLibrary(CommonKnowledgeLibrary library)
		{
			Task.Run(async delegate
			{
				_ = 1;
				try
				{
					if (_isInitialized && library != null && library.Entries != null && !_isSyncing)
					{
						_isSyncing = true;
						List<CommonKnowledgeEntry> list = library.Entries.Where((CommonKnowledgeEntry e) => e != null && e.isEnabled && !string.IsNullOrWhiteSpace(e.content)).ToList();
						List<CommonKnowledgeEntry> entriesToUpdate = new List<CommonKnowledgeEntry>();
						List<string> entriesToRemove = new List<string>();
						lock (_loreVectors)
						{
							foreach (CommonKnowledgeEntry item in list)
							{
								string text = ComputeHash(item.content);
								if (!_contentHashes.ContainsKey(item.id) || _contentHashes[item.id] != text)
								{
									entriesToUpdate.Add(item);
								}
							}
							HashSet<string> hashSet = new HashSet<string>(list.Select((CommonKnowledgeEntry e) => e.id));
							foreach (string item2 in _loreVectors.Keys.ToList())
							{
								if (!hashSet.Contains(item2))
								{
									entriesToRemove.Add(item2);
								}
							}
						}
						if (entriesToUpdate.Count == 0 && entriesToRemove.Count == 0)
						{
							Log.Message("[RimTalk-ExpandMemory] VectorService: No changes detected, skipping sync.");
							_isSyncing = false;
						}
						else
						{
							LongEventHandler.ExecuteWhenFinished((Action)delegate
							{
								Messages.Message($"正在更新向量库... ({entriesToUpdate.Count} 条新增/修改, {entriesToRemove.Count} 条删除)", MessageTypeDefOf.NeutralEvent, false);
							});
							Log.Message($"[RimTalk-ExpandMemory] VectorService: Syncing {entriesToUpdate.Count} updated entries, removing {entriesToRemove.Count} entries...");
							lock (_loreVectors)
							{
								foreach (string item3 in entriesToRemove)
								{
									_loreVectors.Remove(item3);
									_contentHashes.Remove(item3);
								}
							}
							int batchSize = 10;
							int syncedCount = 0;
							for (int i = 0; i < entriesToUpdate.Count; i += batchSize)
							{
								List<CommonKnowledgeEntry> batch = entriesToUpdate.Skip(i).Take(batchSize).ToList();
								List<string> texts = batch.Select((CommonKnowledgeEntry e) => e.content).ToList();
								List<float[]> list2 = await GetEmbeddingsAsync(texts).ConfigureAwait(continueOnCapturedContext: false);
								if (list2 != null && list2.Count == batch.Count)
								{
									lock (_loreVectors)
									{
										for (int num = 0; num < batch.Count; num++)
										{
											_loreVectors[batch[num].id] = list2[num];
											_contentHashes[batch[num].id] = ComputeHash(batch[num].content);
										}
									}
									syncedCount += batch.Count;
								}
								await Task.Delay(200).ConfigureAwait(continueOnCapturedContext: false);
							}
							Log.Message($"[RimTalk-ExpandMemory] VectorService: Sync complete! {syncedCount}/{entriesToUpdate.Count} entries vectorized.");
							LongEventHandler.ExecuteWhenFinished((Action)delegate
							{
								Messages.Message($"向量库已更新 ({syncedCount} 条)", MessageTypeDefOf.PositiveEvent, false);
							});
						}
					}
				}
				catch (Exception ex)
				{
					Exception ex2 = ex;
					Exception ex3 = ex2;
					Log.Error($"[RimTalk-ExpandMemory] VectorService: Error syncing library: {ex3}");
					LongEventHandler.ExecuteWhenFinished((Action)delegate
					{
						Messages.Message("向量库更新失败: " + ex3.Message, MessageTypeDefOf.RejectInput, false);
					});
				}
				finally
				{
					_isSyncing = false;
				}
			});
		}

		public void UpdateKnowledgeVector(string id, string content)
		{
			Task.Run(async delegate
			{
				try
				{
					if (_isInitialized && !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(content))
					{
						string currentHash = ComputeHash(content);
						lock (_loreVectors)
						{
							if (_contentHashes.ContainsKey(id) && _contentHashes[id] == currentHash)
							{
								return;
							}
						}
						float[] array = await GetEmbeddingAsync(content).ConfigureAwait(continueOnCapturedContext: false);
						if (array != null)
						{
							lock (_loreVectors)
							{
								_loreVectors[id] = array;
								_contentHashes[id] = currentHash;
							}
						}
					}
				}
				catch (Exception arg)
				{
					Log.Error($"[RimTalk-ExpandMemory] VectorService: Error updating vector: {arg}");
				}
			});
		}

		public void RemoveKnowledgeVector(string id)
		{
			try
			{
				lock (_loreVectors)
				{
					if (_loreVectors.ContainsKey(id))
					{
						_loreVectors.Remove(id);
						_contentHashes.Remove(id);
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error($"[RimTalk-ExpandMemory] VectorService: Error removing vector: {arg}");
			}
		}

		public void ExportVectorsForSave(out List<string> ids, out List<List<float>> vectors, out List<string> hashes)
		{
			lock (_loreVectors)
			{
				ids = new List<string>(_loreVectors.Keys);
				vectors = new List<List<float>>();
				hashes = new List<string>();
				foreach (string id in ids)
				{
					vectors.Add(_loreVectors[id].ToList());
					hashes.Add(_contentHashes.ContainsKey(id) ? _contentHashes[id] : "");
				}
				Log.Message($"[RimTalk-ExpandMemory] VectorService: Exported {ids.Count} vectors for save");
			}
		}

		public void ImportVectorsFromLoad(List<string> ids, List<List<float>> vectors, List<string> hashes)
		{
			if (ids == null || vectors == null || hashes == null)
			{
				Log.Warning("[RimTalk-ExpandMemory] VectorService: Cannot import null vector data");
				return;
			}
			lock (_loreVectors)
			{
				_loreVectors.Clear();
				_contentHashes.Clear();
				for (int i = 0; i < ids.Count && i < vectors.Count; i++)
				{
					if (vectors[i] != null)
					{
						_loreVectors[ids[i]] = vectors[i].ToArray();
						if (i < hashes.Count && !string.IsNullOrEmpty(hashes[i]))
						{
							_contentHashes[ids[i]] = hashes[i];
						}
					}
				}
				Log.Message($"[RimTalk-ExpandMemory] VectorService: Imported {_loreVectors.Count} vectors from save");
			}
		}

		private static string ComputeHash(string content)
		{
			if (string.IsNullOrEmpty(content))
			{
				return string.Empty;
			}
			using SHA256 sHA = SHA256.Create();
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			return Convert.ToBase64String(sHA.ComputeHash(bytes));
		}

		private float[] GetEmbedding(string text)
		{
			return GetEmbeddingAsync(text).GetAwaiter().GetResult();
		}

		private async Task<float[]> GetEmbeddingAsync(string text)
		{
			return (await GetEmbeddingsAsync(new List<string> { text }).ConfigureAwait(continueOnCapturedContext: false))?.FirstOrDefault();
		}

		private async Task<List<float[]>> GetEmbeddingsAsync(List<string> texts)
		{
			_ = 2;
			try
			{
				RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
				string embeddingApiKey = settings.embeddingApiKey;
				string embeddingApiUrl = settings.embeddingApiUrl;
				string embeddingModel = settings.embeddingModel;
				if (string.IsNullOrEmpty(embeddingApiKey))
				{
					Log.Warning("[RimTalk-ExpandMemory] VectorService: API Key is missing. Please configure either Embedding API Key or Independent API Key.");
					return null;
				}
				StringContent content = new StringContent(JsonConvert.SerializeObject(new
				{
					input = texts,
					model = embeddingModel
				}), Encoding.UTF8, "application/json");
				using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, embeddingApiUrl);
				request.Headers.Add("Authorization", "Bearer " + embeddingApiKey);
				request.Content = content;
				HttpResponseMessage response = await _httpClient.SendAsync(request).ConfigureAwait(continueOnCapturedContext: false);
				if (!response.IsSuccessStatusCode)
				{
					string text = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
					Log.Error($"[RimTalk-ExpandMemory] VectorService: API request failed: {(int)response.StatusCode} ({response.StatusCode})");
					Log.Error("[RimTalk-ExpandMemory] VectorService: Error response: " + text);
					return null;
				}
				string text2 = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
				if (!(JObject.Parse(text2)["data"] is JArray jArray))
				{
					Log.Error("[RimTalk-ExpandMemory] VectorService: No 'data' field in response: " + text2);
					return null;
				}
				List<float[]> list = new List<float[]>();
				foreach (JToken item in jArray)
				{
					float[] array = item["embedding"]?.ToObject<float[]>();
					if (array != null)
					{
						list.Add(array);
					}
				}
				return list;
			}
			catch (HttpRequestException ex)
			{
				Log.Error("[RimTalk-ExpandMemory] VectorService: HTTP request failed: " + ex.Message);
				Log.Error("[RimTalk-ExpandMemory] VectorService: Stack trace: " + ex.StackTrace);
				return null;
			}
			catch (Exception ex2)
			{
				Log.Error("[RimTalk-ExpandMemory] VectorService: Unexpected error: " + ex2.Message);
				Log.Error("[RimTalk-ExpandMemory] VectorService: Stack trace: " + ex2.StackTrace);
				return null;
			}
		}

		private static float CosineSimilarity(float[] vec1, float[] vec2)
		{
			if (vec1.Length != vec2.Length)
			{
				return 0f;
			}
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			for (int i = 0; i < vec1.Length; i++)
			{
				num += vec1[i] * vec2[i];
				num2 += vec1[i] * vec1[i];
				num3 += vec2[i] * vec2[i];
			}
			if (num2 == 0f || num3 == 0f)
			{
				return 0f;
			}
			return num / (float)(Math.Sqrt(num2) * Math.Sqrt(num3));
		}

		public void Dispose()
		{
			try
			{
				_httpClient?.Dispose();
				_httpClient = null;
				_isInitialized = false;
				Log.Message("[RimTalk-ExpandMemory] VectorService: Disposed.");
			}
			catch (Exception arg)
			{
				Log.Error($"[RimTalk-ExpandMemory] VectorService: Error during disposal: {arg}");
			}
		}
	}
}
namespace RimTalk.Memory.Utils
{
	public static class GenDateExtension
	{
		public static string GetInGameHour12HString(long absTicks, Vector2 longLat)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			int num = GenDate.HourOfDay(absTicks, longLat.x);
			int num2 = num % 12;
			if (num2 == 0)
			{
				num2 = 12;
			}
			return string.Format("{0}{1}", num2, (num < 12) ? "am" : "pm");
		}
	}
	public static class JsonUtil
	{
		public static string SerializeToJson<T>(T obj)
		{
			using MemoryStream memoryStream = new MemoryStream();
			new DataContractJsonSerializer(typeof(T)).WriteObject(memoryStream, obj);
			return Encoding.UTF8.GetString(memoryStream.ToArray());
		}

		public static T DeserializeFromJson<T>(string json)
		{
			if (!TryDeserializeFromJson<T>(json, out var result, out var exception))
			{
				throw exception;
			}
			return result;
		}

		public static bool TryDeserializeFromJson<T>(string json, out T result, out Exception exception)
		{
			result = default(T);
			exception = null;
			if (string.IsNullOrWhiteSpace(json))
			{
				return false;
			}
			try
			{
				using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(T));
				result = (T)dataContractJsonSerializer.ReadObject(stream);
				return true;
			}
			catch (Exception ex)
			{
				exception = ex;
				return false;
			}
		}
	}
	public static class MessageUtil
	{
		public static void MessageAndError(string message)
		{
			Messages.Message("[RimTalk.Memory.AI] " + message, MessageTypeDefOf.RejectInput, false);
			Log.Error("[RimTalk.Memory.AI] " + message);
		}
	}
	public class RimRingBuffer<T>
	{
		private readonly T[] _buffer;

		private readonly int _capacity;

		private readonly int _capacityMask;

		private int _head;

		private int _count;

		public int Count => _count;

		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= _count)
				{
					throw new IndexOutOfRangeException($"索引 {index} 超出边界 (Count: {_count})");
				}
				return _buffer[(_head + index) & _capacityMask];
			}
		}

		public RimRingBuffer(int newCapacity)
		{
			_capacity = CeilPow2(newCapacity);
			_buffer = new T[_capacity];
			_capacityMask = _capacity - 1;
		}

		private static int CeilPow2(int x)
		{
			if (x <= 1)
			{
				return 1;
			}
			x--;
			x |= x >> 1;
			x |= x >> 2;
			x |= x >> 4;
			x |= x >> 8;
			x |= x >> 16;
			return x + 1;
		}

		public void Add(T item)
		{
			_buffer[(_head + _count) & _capacityMask] = item;
			if (_count < _capacity)
			{
				_count++;
			}
			else
			{
				_head = (_head + 1) & _capacityMask;
			}
		}

		public void Clear()
		{
			_head = 0;
			_count = 0;
			Array.Clear(_buffer, 0, _buffer.Length);
		}
	}
}
namespace RimTalk.Memory.Patches
{
	[HarmonyPatch(typeof(IncidentWorker), "TryExecute")]
	public static class IncidentPatch
	{
		private class RaidEventInfo
		{
			public string entryId;

			public string factionName;

			public string raidType;

			public int startTick;

			public string initialText;
		}

		private static Dictionary<int, RaidEventInfo> activeRaids = new Dictionary<int, RaidEventInfo>();

		private static bool raidCheckActive = false;

		[HarmonyPostfix]
		public static void Postfix(IncidentWorker __instance, IncidentParms parms, bool __result)
		{
			if (!__result || !RimTalkMemoryPatchMod.Settings.enableEventRecordKnowledge)
			{
				return;
			}
			try
			{
				IncidentDef def = __instance.def;
				if (def == null)
				{
					return;
				}
				if (IsRaidIncident(def))
				{
					HandleRaidStart(def, parms);
					return;
				}
				float num = CalculateIncidentImportance(def);
				if (!(num < 0.5f))
				{
					string text = GenerateEventDescription(def, parms);
					if (!string.IsNullOrEmpty(text))
					{
						AddOrUpdateKnowledge(null, text, num);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("[EventRecord] Error in IncidentPatch: " + ex.Message);
			}
		}

		private static bool IsRaidIncident(IncidentDef incidentDef)
		{
			string defName = ((Def)incidentDef).defName;
			if (!defName.Contains("Raid") && !defName.Contains("Siege") && (!defName.Contains("Mech") || !defName.Contains("Cluster")))
			{
				return incidentDef.category == IncidentCategoryDefOf.ThreatBig;
			}
			return true;
		}

		private static void HandleRaidStart(IncidentDef incidentDef, IncidentParms parms)
		{
			int ticksGame = GenTicks.TicksGame;
			string text = "未知敌人";
			if (parms.faction != null && !string.IsNullOrEmpty(parms.faction.Name))
			{
				text = parms.faction.Name;
			}
			string raidType = GetRaidType(incidentDef);
			string text2 = "今天" + text + "发动了" + raidType;
			CommonKnowledgeEntry commonKnowledgeEntry = AddOrUpdateKnowledge(null, text2, 0.9f);
			if (commonKnowledgeEntry != null)
			{
				activeRaids[ticksGame] = new RaidEventInfo
				{
					entryId = commonKnowledgeEntry.id,
					factionName = text,
					raidType = raidType,
					startTick = GenTicks.TicksGame,
					initialText = text2
				};
				if (!raidCheckActive)
				{
					raidCheckActive = true;
				}
			}
		}

		private static string GetRaidType(IncidentDef incidentDef)
		{
			string defName = ((Def)incidentDef).defName;
			if (defName.Contains("Siege"))
			{
				return "围城";
			}
			if (defName.Contains("Mech"))
			{
				return "机械族攻击";
			}
			if (defName.Contains("Sapper"))
			{
				return "工兵袭击";
			}
			if (defName.Contains("Breacher"))
			{
				return "破坏者袭击";
			}
			return "袭击";
		}

		public static void CheckRaidStatus()
		{
			if (!raidCheckActive || activeRaids.Count == 0)
			{
				return;
			}
			try
			{
				CommonKnowledgeLibrary commonKnowledge = MemoryManager.GetCommonKnowledge();
				if (commonKnowledge == null)
				{
					return;
				}
				int ticksGame = GenTicks.TicksGame;
				List<int> list = new List<int>();
				foreach (KeyValuePair<int, RaidEventInfo> activeRaid in activeRaids)
				{
					int key = activeRaid.Key;
					RaidEventInfo value = activeRaid.Value;
					int num = ticksGame - value.startTick;
					if (num > 10000)
					{
						UpdateRaidOutcome(commonKnowledge, value, defeated: true);
						list.Add(key);
					}
					else if (!CheckForEnemies() && num > 1000)
					{
						UpdateRaidOutcome(commonKnowledge, value, defeated: true);
						list.Add(key);
					}
				}
				foreach (int item in list)
				{
					activeRaids.Remove(item);
				}
				if (activeRaids.Count == 0)
				{
					raidCheckActive = false;
				}
			}
			catch (Exception ex)
			{
				Log.Error("[EventRecord] Error checking raid status: " + ex.Message);
			}
		}

		private static bool CheckForEnemies()
		{
			if (Find.CurrentMap == null)
			{
				return false;
			}
			foreach (Pawn item in Find.CurrentMap.mapPawns.AllPawnsSpawned)
			{
				if (GenHostility.HostileTo((Thing)(object)item, Faction.OfPlayer) && !item.Dead && !item.Downed)
				{
					return true;
				}
			}
			return false;
		}

		private static void UpdateRaidOutcome(CommonKnowledgeLibrary library, RaidEventInfo raidInfo, bool defeated)
		{
			CommonKnowledgeEntry commonKnowledgeEntry = GenCollection.FirstOrDefault<CommonKnowledgeEntry>(library.Entries, (Predicate<CommonKnowledgeEntry>)((CommonKnowledgeEntry e) => e.id == raidInfo.entryId));
			if (commonKnowledgeEntry != null)
			{
				if (defeated)
				{
					commonKnowledgeEntry.content = raidInfo.initialText + "，殖民地成功击退了进攻";
					commonKnowledgeEntry.importance = 0.95f;
				}
				else
				{
					commonKnowledgeEntry.content = raidInfo.initialText + "，造成了严重损失";
					commonKnowledgeEntry.importance = 1f;
				}
			}
		}

		private static CommonKnowledgeEntry AddOrUpdateKnowledge(string existingId, string eventText, float importance)
		{
			CommonKnowledgeLibrary commonKnowledge = MemoryManager.GetCommonKnowledge();
			if (commonKnowledge == null)
			{
				return null;
			}
			CommonKnowledgeEntry commonKnowledgeEntry = null;
			if (!string.IsNullOrEmpty(existingId))
			{
				commonKnowledgeEntry = GenCollection.FirstOrDefault<CommonKnowledgeEntry>(commonKnowledge.Entries, (Predicate<CommonKnowledgeEntry>)((CommonKnowledgeEntry e) => e.id == existingId));
				if (commonKnowledgeEntry != null)
				{
					commonKnowledgeEntry.content = eventText;
					commonKnowledgeEntry.importance = importance;
					return commonKnowledgeEntry;
				}
			}
			if (!GenCollection.Any<CommonKnowledgeEntry>(commonKnowledge.Entries, (Predicate<CommonKnowledgeEntry>)((CommonKnowledgeEntry e) => e.content.Contains(eventText.Substring(0, Math.Min(15, eventText.Length))))))
			{
				commonKnowledgeEntry = new CommonKnowledgeEntry("事件,历史", eventText)
				{
					importance = importance,
					isEnabled = true,
					isUserEdited = false
				};
				commonKnowledge.AddEntry(commonKnowledgeEntry);
			}
			return commonKnowledgeEntry;
		}

		private static float CalculateIncidentImportance(IncidentDef incidentDef)
		{
			string defName = ((Def)incidentDef).defName;
			string label = ((Def)incidentDef).label;
			if (defName.Contains("Death") || defName.Contains("Dead") || label.Contains("死") || label.Contains("death"))
			{
				return 1f;
			}
			if (defName.Contains("Marriage") || defName.Contains("Wedding") || label.Contains("结婚") || label.Contains("婚"))
			{
				return 0.85f;
			}
			if (defName.Contains("Funeral") || defName.Contains("Burial") || label.Contains("葬礼") || label.Contains("葬") || label.Contains("埋葬"))
			{
				return 0.9f;
			}
			if (defName.Contains("Birthday") || label.Contains("生日"))
			{
				return 0.7f;
			}
			if (defName.Contains("Breakthrough") || (defName.Contains("Research") && defName.Contains("Complete")) || label.Contains("突破") || label.Contains("完成研究"))
			{
				return 0.8f;
			}
			if (defName.Contains("Anniversary") || label.Contains("周年"))
			{
				return 0.7f;
			}
			if (defName.Contains("Join") || defName.Contains("Refugee") || defName.Contains("WandererJoin") || label.Contains("加入") || label.Contains("难民"))
			{
				return 0.8f;
			}
			if (defName.Contains("Infestation") || label.Contains("虫"))
			{
				return 0.85f;
			}
			if (defName.Contains("Fire") || defName.Contains("Explosion") || defName.Contains("Tornado") || defName.Contains("Eclipse") || label.Contains("火") || label.Contains("爆炸") || label.Contains("龙卷风"))
			{
				return 0.85f;
			}
			if (defName.Contains("Caravan") || defName.Contains("Visitor") || defName.Contains("Trade") || label.Contains("贸易") || label.Contains("访客"))
			{
				return 0.6f;
			}
			if (defName.Contains("Disease") || label.Contains("疾病") || label.Contains("瘟疫"))
			{
				return 0.75f;
			}
			if (defName.Contains("Quest") || label.Contains("任务"))
			{
				return 0.65f;
			}
			return 0.3f;
		}

		private static string GenerateEventDescription(IncidentDef incidentDef, IncidentParms parms)
		{
			string label = ((Def)incidentDef).label;
			string defName = ((Def)incidentDef).defName;
			string text = "今天";
			if (defName.Contains("Marriage") || defName.Contains("Wedding"))
			{
				return text + "举行了婚礼";
			}
			if (defName.Contains("Funeral") || defName.Contains("Burial"))
			{
				return text + "举行了葬礼";
			}
			if (defName.Contains("Birthday"))
			{
				return text + "庆祝了生日";
			}
			if (defName.Contains("Breakthrough") || (defName.Contains("Research") && defName.Contains("Complete")))
			{
				return text + "取得了研究突破";
			}
			if (defName.Contains("Anniversary"))
			{
				return text + "庆祝了周年纪念";
			}
			if (defName.Contains("WandererJoin") || defName.Contains("RefugeeJoin"))
			{
				return text + "有新成员加入殖民地";
			}
			if (defName.Contains("Infestation"))
			{
				return text + "发生了虫族入侵";
			}
			if (defName.Contains("Fire"))
			{
				return text + "发生了火灾";
			}
			if (defName.Contains("Explosion"))
			{
				return text + "发生了爆炸";
			}
			if (defName.Contains("Tornado"))
			{
				return text + "遭遇了龙卷风";
			}
			if (defName.Contains("Eclipse"))
			{
				return text + "发生了日食";
			}
			if (defName.Contains("TraderCaravan") || defName.Contains("VisitorGroup"))
			{
				return null;
			}
			if (!string.IsNullOrEmpty(label))
			{
				return text + label;
			}
			return null;
		}
	}
	[HarmonyPatch]
	public static class Patch_GenerateAndProcessTalkAsync
	{
		[HarmonyTargetMethod]
		public static MethodBase TargetMethod()
		{
			Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.GetName().Name == "RimTalk");
			if (assembly == null)
			{
				Log.Warning("[RimTalk Memory] RimTalk assembly not found for GenerateAndProcessTalkAsync patch");
				return null;
			}
			Type type = assembly.GetType("RimTalk.Service.TalkService");
			if (type == null)
			{
				Log.Warning("[RimTalk Memory] TalkService type not found");
				return null;
			}
			MethodInfo method = type.GetMethod("GenerateAndProcessTalkAsync", BindingFlags.Static | BindingFlags.NonPublic);
			if (method == null)
			{
				Log.Warning("[RimTalk Memory] GenerateAndProcessTalkAsync method not found");
				return null;
			}
			Log.Message("[RimTalk Memory] ✓ Found GenerateAndProcessTalkAsync for patching");
			return method;
		}

		[HarmonyPrefix]
		public static void Prefix(object talkRequest)
		{
			try
			{
				RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
				if (!settings.enableVectorEnhancement)
				{
					return;
				}
				KnowledgeVariableProvider.KnowledgeInjectionContext lastContext = KnowledgeVariableProvider.GetLastContext();
				if (lastContext == null)
				{
					return;
				}
				Type type = talkRequest.GetType();
				PropertyInfo property = type.GetProperty("PromptMessages");
				PropertyInfo property2 = type.GetProperty("Context");
				string dialogueType = lastContext.DialogueType;
				string matchText = lastContext.MatchText;
				Pawn speaker = lastContext.Speaker;
				Pawn listener = lastContext.Listener;
				string keywordKnowledge = lastContext.KeywordKnowledge;
				if (string.IsNullOrEmpty(dialogueType))
				{
					return;
				}
				string text = null;
				try
				{
					List<(string, float)> result = VectorService.Instance.FindBestLoreIdsAsync(dialogueType, settings.maxVectorResults, settings.vectorSimilarityThreshold).Result;
					if (result != null && result.Count > 0)
					{
						World world = Find.World;
						MemoryManager memoryManager = ((world != null) ? world.GetComponent<MemoryManager>() : null);
						if (memoryManager != null)
						{
							HashSet<string> hashSet = new HashSet<string>();
							try
							{
								memoryManager.CommonKnowledge.InjectKnowledgeWithDetails(matchText, settings.maxVectorResults, out var scores, speaker, listener);
								if (scores != null)
								{
									foreach (KnowledgeScore item2 in scores)
									{
										hashSet.Add(item2.Entry.id);
									}
								}
							}
							catch
							{
							}
							List<CommonKnowledgeEntry> list = memoryManager.CommonKnowledge.Entries.ToList();
							List<(CommonKnowledgeEntry, float, float)> list2 = new List<(CommonKnowledgeEntry, float, float)>();
							foreach (var item3 in result)
							{
								var (id, num) = item3;
								if (!hashSet.Contains(id))
								{
									CommonKnowledgeEntry commonKnowledgeEntry = GenCollection.FirstOrDefault<CommonKnowledgeEntry>(list, (Predicate<CommonKnowledgeEntry>)((CommonKnowledgeEntry e) => e.id == id));
									if (commonKnowledgeEntry != null)
									{
										float item = num + commonKnowledgeEntry.importance * 0.2f;
										list2.Add((commonKnowledgeEntry, num, item));
									}
								}
							}
							List<(CommonKnowledgeEntry, float, float)> list3 = list2.OrderByDescending<(CommonKnowledgeEntry, float, float), float>(((CommonKnowledgeEntry Entry, float Similarity, float Score) x) => x.Score).ToList();
							if (list3.Count > 0)
							{
								StringBuilder stringBuilder = new StringBuilder();
								stringBuilder.AppendLine();
								stringBuilder.AppendLine("#### Vector Enhanced:");
								int num2 = 1;
								foreach (var item4 in list3)
								{
									stringBuilder.AppendLine($"{num2}. [{item4.Item1.tag}] {item4.Item1.content}");
									num2++;
								}
								text = stringBuilder.ToString();
							}
						}
					}
				}
				catch (Exception ex)
				{
					Log.Warning("[RimTalk Memory] Vector search error: " + ex.Message);
				}
				if (string.IsNullOrEmpty(text))
				{
					return;
				}
				bool flag = false;
				if (property2 != null && !flag)
				{
					try
					{
						string text2 = property2.GetValue(talkRequest) as string;
						if (!string.IsNullOrEmpty(text2) && text2.Contains("(No matching knowledge found)"))
						{
							string value = text2.Replace("(No matching knowledge found)", "(No matching knowledge found)" + text);
							property2.SetValue(talkRequest, value);
							flag = true;
						}
						else if (!string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(keywordKnowledge) && text2.Contains(keywordKnowledge))
						{
							int startIndex = text2.IndexOf(keywordKnowledge) + keywordKnowledge.Length;
							string value2 = text2.Insert(startIndex, text);
							property2.SetValue(talkRequest, value2);
							flag = true;
						}
					}
					catch
					{
					}
				}
				if (property != null && !flag)
				{
					try
					{
						object value3 = property.GetValue(talkRequest);
						if (value3 != null && value3 is IList { Count: >0 } list4)
						{
							Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.GetName().Name == "RimTalk");
							Type type2 = assembly?.GetType("RimTalk.Source.Data.Role");
							if (type2 == null)
							{
								type2 = assembly?.GetType("RimTalk.Data.Role");
							}
							if (type2 != null)
							{
								object obj3 = Enum.Parse(type2, "System");
								int num3 = -1;
								string text3 = null;
								object obj4 = null;
								bool flag2 = false;
								bool flag3 = false;
								string value4 = ((!string.IsNullOrEmpty(keywordKnowledge) && keywordKnowledge.Length > 10) ? keywordKnowledge.Substring(0, Math.Min(50, keywordKnowledge.Length)) : null);
								for (int num4 = 0; num4 < list4.Count; num4++)
								{
									object obj5 = list4[num4];
									Type type3 = obj5.GetType();
									FieldInfo fieldInfo = type3.GetField("Item2") ?? type3.GetField("content");
									FieldInfo fieldInfo2 = type3.GetField("Item1") ?? type3.GetField("role");
									if (fieldInfo != null)
									{
										string text4 = (fieldInfo.GetValue(obj5) as string) ?? "";
										if (!string.IsNullOrEmpty(value4) && text4.Contains(value4))
										{
											num3 = num4;
											text3 = text4;
											obj4 = fieldInfo2?.GetValue(obj5);
											flag2 = true;
											break;
										}
										if (text4.Contains("(No matching knowledge found)"))
										{
											num3 = num4;
											text3 = text4;
											obj4 = fieldInfo2?.GetValue(obj5);
											flag3 = true;
										}
									}
								}
								if (flag2 && num3 >= 0 && text3 != null)
								{
									int num5 = text3.IndexOf(keywordKnowledge) + keywordKnowledge.Length;
									if (num5 > 0 && num5 <= text3.Length)
									{
										string text5 = text3.Insert(num5, text);
										object value5 = Activator.CreateInstance(typeof(ValueTuple<, >).MakeGenericType(type2, typeof(string)), obj4 ?? obj3, text5);
										list4[num3] = value5;
										flag = true;
									}
								}
								else if (flag3 && num3 >= 0 && text3 != null)
								{
									string text6 = text3.Replace("(No matching knowledge found)", "(No matching knowledge found)" + text);
									object value6 = Activator.CreateInstance(typeof(ValueTuple<, >).MakeGenericType(type2, typeof(string)), obj4 ?? obj3, text6);
									list4[num3] = value6;
									flag = true;
								}
								else
								{
									int num6 = list4.Count - 1;
									if (num6 >= 0)
									{
										object obj6 = list4[num6];
										Type type4 = obj6.GetType();
										FieldInfo fieldInfo3 = type4.GetField("Item2") ?? type4.GetField("content");
										FieldInfo fieldInfo4 = type4.GetField("Item1") ?? type4.GetField("role");
										if (fieldInfo3 != null)
										{
											string obj7 = (fieldInfo3.GetValue(obj6) as string) ?? "";
											object obj8 = fieldInfo4?.GetValue(obj6) ?? obj3;
											string text7 = obj7 + "\n" + text;
											object value7 = Activator.CreateInstance(typeof(ValueTuple<, >).MakeGenericType(type2, typeof(string)), obj8, text7);
											list4[num6] = value7;
											flag = true;
										}
									}
								}
							}
						}
					}
					catch
					{
					}
				}
				KnowledgeVariableProvider.ClearContext();
			}
			catch (Exception arg)
			{
				Log.Error($"[RimTalk Memory] Error in GenerateAndProcessTalkAsync Prefix: {arg}");
			}
		}
	}
	[HarmonyPatch(typeof(PromptContext), "FromTalkRequest")]
	public static class PromptContext_FromTalkRequest_Patch
	{
		[HarmonyPrefix]
		private static void Prefix(TalkRequest request, List<Pawn> pawns)
		{
			if (request != null)
			{
				request.Participants = pawns;
			}
		}
	}
	[HarmonyPatch(typeof(Settings), "WriteSettings")]
	public static class RimTalk_Settings_WriteSettings_Patch
	{
		[HarmonyPostfix]
		private static void Postfix()
		{
			AIService.ResetClientPool();
		}
	}
	[HarmonyPatch(typeof(RimTalkSettings), "TryNextConfig")]
	public static class RimTalk_Settings_TryNextConfig_Patch
	{
		[HarmonyPostfix]
		private static void Postfix()
		{
			AIService.ResetClientPool();
		}
	}
	[HarmonyPatch(typeof(ThingWithComps), "InitializeComps")]
	public static class ThingWithComps_InitializeComps_Patch
	{
		private static readonly CompProperties_PawnMemory _pawnMemoryProps = new CompProperties_PawnMemory();

		[HarmonyPostfix]
		public static void Postfix(ThingWithComps __instance, ref List<ThingComp> ___comps)
		{
			Pawn val = (Pawn)(object)((__instance is Pawn) ? __instance : null);
			if (val == null)
			{
				return;
			}
			RaceProperties raceProps = val.RaceProps;
			if (raceProps != null && raceProps.Humanlike && ((ThingWithComps)val).GetComp<PawnMemoryComp>() == null)
			{
				PawnMemoryComp pawnMemoryComp = new PawnMemoryComp();
				((ThingComp)pawnMemoryComp).parent = __instance;
				if (___comps == null)
				{
					___comps = new List<ThingComp>();
				}
				___comps.Add((ThingComp)(object)pawnMemoryComp);
				((ThingComp)pawnMemoryComp).Initialize((CompProperties)(object)_pawnMemoryProps);
			}
		}
	}
}
namespace RimTalk.Memory.Patches.RimChat
{
	[HarmonyPatch(/*Could not decode attribute arguments.*/)]
	public static class RpgNpcDialogueArchiveManager_FinalizeSession_Patch
	{
		private static Type chatMessageDataType;

		private static FieldRef<object, string> roleRef;

		private static FieldRef<object, string> contentRef;

		private static bool IsEnable => RimTalkMemoryPatchMod.Settings?.IsRoundMemoryActive ?? false;

		[HarmonyPrepare]
		private static bool Prepare()
		{
			try
			{
				chatMessageDataType = AccessTools.TypeByName("RimChat.AI.ChatMessageData");
				if ((object)chatMessageDataType == null)
				{
					Log.Message("[RimTalk.Memory.Patches.RimChat]: 无法找到ChatMessageData类型，补丁将被禁用。");
					return false;
				}
				roleRef = AccessTools.FieldRefAccess<string>(chatMessageDataType, "role");
				contentRef = AccessTools.FieldRefAccess<string>(chatMessageDataType, "content");
				Log.Message("[RimTalk.Memory.Patches.RimChat]: 字段访问器初始化成功。");
				return true;
			}
			catch
			{
				Log.Error("[RimTalk.Memory.Patches.RimChat]: 初始化RimChat补丁时出现异常");
				return false;
			}
		}

		[HarmonyPrefix]
		private static void Prefix(Pawn initiator, Pawn targetNpc, IList chatHistory)
		{
			if (!IsEnable || chatHistory == null || chatHistory.Count == 0)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			string value = ((initiator != null) ? ((Entity)initiator).LabelShort : null) ?? "???";
			string value2 = ((targetNpc != null) ? ((Entity)targetNpc).LabelShort : null) ?? "???";
			foreach (object item in chatHistory)
			{
				if (item == null)
				{
					continue;
				}
				string text = roleRef.Invoke(item);
				if (!(text == "user"))
				{
					if (text == "assistant")
					{
						stringBuilder.Append(value2).Append(": ").AppendLine(contentRef.Invoke(item));
					}
				}
				else
				{
					stringBuilder.Append(value).Append(": ").AppendLine(contentRef.Invoke(item));
				}
			}
			if (stringBuilder.Length != 0)
			{
				string content = stringBuilder.ToString().TrimEnd();
				RoundMemoryManager.BuildRoundMemory(new HashSet<Pawn> { initiator, targetNpc }, content);
			}
		}
	}
	[HarmonyPatch(/*Could not decode attribute arguments.*/)]
	public static class RpgNpcDialogueArchiveManager_RecordDiplomacySummary_Patch
	{
		private static Type dialogueMessageDataType;

		private static FieldRef<object, bool> isPlayerRef;

		private static FieldRef<object, string> messageRef;

		private static FastInvokeHandler isSystemMessageInvoker;

		private static bool IsEnable => RimTalkMemoryPatchMod.Settings?.IsRoundMemoryActive ?? false;

		[HarmonyPrepare]
		private static bool Prepare()
		{
			try
			{
				dialogueMessageDataType = AccessTools.TypeByName("RimChat.Memory.DialogueMessageData");
				if ((object)dialogueMessageDataType == null)
				{
					Log.Message("[RimTalk.Memory.Patches.RimChat]: 无法找到DialogueMessageData类型，补丁将被禁用。");
					return false;
				}
				MethodInfo methodInfo = AccessTools.Method(dialogueMessageDataType, "IsSystemMessage", (Type[])null, (Type[])null);
				if ((object)methodInfo == null)
				{
					Log.Message("[RimTalk.Memory.Patches.RimChat]: 无法找到IsSystemMessage方法，补丁将被禁用。");
					return false;
				}
				isSystemMessageInvoker = MethodInvoker.GetHandler(methodInfo, false);
				isPlayerRef = AccessTools.FieldRefAccess<bool>(dialogueMessageDataType, "isPlayer");
				messageRef = AccessTools.FieldRefAccess<string>(dialogueMessageDataType, "message");
				Log.Message("[RimTalk.Memory.Patches.RimChat]: 所有访问器初始化成功。");
				return true;
			}
			catch
			{
				Log.Error("[RimTalk.Memory.Patches.RimChat]: 初始化RimChat补丁时出现异常");
				return false;
			}
		}

		[HarmonyPrefix]
		private static void Prefix(Pawn negotiator, Faction faction, IList allMessages)
		{
			if (!IsEnable || allMessages == null || allMessages.Count == 0)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			string value = ((negotiator != null) ? ((Entity)negotiator).LabelShort : null) ?? "???";
			string value2 = ((faction != null) ? faction.Name : null) ?? "???";
			foreach (object allMessage in allMessages)
			{
				if (allMessage != null && !(bool)isSystemMessageInvoker.Invoke(allMessage, Array.Empty<object>()))
				{
					if (isPlayerRef.Invoke(allMessage))
					{
						stringBuilder.Append(value).Append(": ").AppendLine(messageRef.Invoke(allMessage));
					}
					else
					{
						stringBuilder.Append(value2).Append(": ").AppendLine(messageRef.Invoke(allMessage));
					}
				}
			}
			if (stringBuilder.Length != 0)
			{
				string content = stringBuilder.ToString().TrimEnd();
				RoundMemoryManager.BuildRoundMemory(new HashSet<Pawn> { negotiator }, content);
			}
		}
	}
}
namespace RimTalk.Memory.Patches.Injection
{
	[HarmonyPatch(typeof(PromptManager), "BuildMessagesFromPreset")]
	public static class PromptManager_BuildMessagesFromPreset_Patch
	{
		[HarmonyPrefix]
		private static void Prefix()
		{
			ABMCollector.ResetDuplicateCache();
		}
	}
	[HarmonyPatch(typeof(PresetPreviewGenerator), "GeneratePreview")]
	public static class PresetPreviewGenerator_GeneratePreview_Patch
	{
		[HarmonyPrefix]
		private static void Prefix()
		{
			ABMCollector.ResetDuplicateCache();
		}
	}
}
namespace RimTalk.Memory.Patches.Capture
{
	[HarmonyPatch(typeof(Pawn), "Notify_UsedVerb")]
	public static class Pawn_Notify_UsedVerb_Patch
	{
		[HarmonyPostfix]
		public static void Postfix(Pawn __instance, Verb verb)
		{
			CombatMemoryCapturer.CaptureAttackEnter(__instance, verb);
		}
	}
	[HarmonyPatch(typeof(CustomDialogueService), "ExecuteDialogue", new Type[]
	{
		typeof(Pawn),
		typeof(Pawn),
		typeof(string),
		typeof(bool),
		typeof(string)
	})]
	public static class CustomDialogueService_ExecuteDialogue_Patch
	{
		[HarmonyPostfix]
		private static void Postfix(Pawn initiator, string message)
		{
			RoundMemoryManager.CapturePlayerDialogue(initiator, message);
		}
	}
	[HarmonyPatch(typeof(Pawn_JobTracker), "StartJob")]
	public static class Pawn_JobTracker_StartJob_Patch
	{
		[HarmonyPostfix]
		public static void Postfix(Job ___curJob, Pawn ___pawn)
		{
			JobMemoryCapturer.ExtractJobInfoEnter(___curJob, ___pawn);
		}
	}
	[HarmonyPatch(typeof(Pawn_JobTracker), "CleanupCurrentJob")]
	public static class Pawn_JobTracker_CleanupCurrentJob_Patch
	{
		[HarmonyPrefix]
		public static void Prefix(JobCondition condition, Job ___curJob, Pawn ___pawn)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			JobMemoryCapturer.BuildJobMemoryEnter(condition, ___curJob, ___pawn);
		}
	}
	[HarmonyPatch(typeof(TalkService), "CreateInteraction")]
	public static class RimTalkConversationCapturePatch
	{
		[HarmonyPostfix]
		private static void Postfix(Pawn pawn, TalkResponse talk)
		{
			RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
			if (settings == null || !settings.enableConversationMemory)
			{
				return;
			}
			RimTalkMemoryPatchSettings settings2 = RimTalkMemoryPatchMod.Settings;
			if (settings2 == null || settings2.IsRoundMemoryActive || talk == null)
			{
				return;
			}
			string text = talk.Text;
			if (text == null || string.IsNullOrEmpty(text) || pawn == null)
			{
				return;
			}
			FourLayerMemoryComp fourLayerMemoryComp = ThingCompUtility.TryGetComp<FourLayerMemoryComp>((Thing)(object)pawn);
			if (fourLayerMemoryComp != null)
			{
				Pawn target = talk.GetTarget();
				bool flag = target == null || target == pawn;
				fourLayerMemoryComp.ActiveMemories.Add(new MemoryEntry("Said to " + (flag ? "self" : ((Entity)target).LabelShort) + ": " + text, MemoryType.Conversation, MemoryLayer.Active));
				if (!flag)
				{
					ThingCompUtility.TryGetComp<FourLayerMemoryComp>((Thing)(object)target)?.ActiveMemories.Add(new MemoryEntry(((Entity)pawn).LabelShort + " said: " + text, MemoryType.Conversation, MemoryLayer.Active));
				}
			}
		}
	}
	[HarmonyPatch(typeof(TalkService), "CreateInteraction")]
	public static class TalkService_CreateInteraction_Patch
	{
		[HarmonyPostfix]
		private static void Postfix(TalkResponse talk)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Invalid comparison between Unknown and I4
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
			if (settings == null || !settings.enableConversationMemory)
			{
				return;
			}
			RimTalkMemoryPatchSettings settings2 = RimTalkMemoryPatchMod.Settings;
			if (settings2 == null || !settings2.IsRoundMemoryActive || talk == null)
			{
				return;
			}
			ApiLog apiLog = ApiHistory.GetApiLog(talk.Id);
			if (apiLog != null && (int)apiLog.Channel != 2 && apiLog.SpokenTick > 0)
			{
				TalkRequest talkRequest = apiLog.TalkRequest;
				if (talkRequest != null)
				{
					string name = talk.Name;
					RoundMemoryManager.StreamingBuildRoundMemory<TalkRequest>(talkRequest, (string.IsNullOrWhiteSpace(name) ? "???" : name) + ": " + talk.Text, talkRequest.Participants, TalkTypeExtensions.IsFromUser(talk.TalkType));
				}
			}
		}
	}
}
namespace RimTalk.Memory.Debug
{
	public class Dialog_InjectionPreview : Window
	{
		private Pawn selectedPawn;

		private Pawn targetPawn;

		private Vector2 scrollPositionLeft;

		private Vector2 scrollPositionRight;

		private Vector2 scrollPositionMatchSource;

		private Vector2 scrollPositionParsedText;

		private List<(string name, string description, bool isPawnProperty)> availableMatchingSources;

		private string parsedMatchText = "";

		private List<KnowledgeScore> matchedKnowledge = new List<KnowledgeScore>();

		private string memoryPreviewText = "";

		private int cachedMemoryCount;

		private bool showMatchSourcePanel = true;

		private bool showKnowledgePanel = true;

		private bool showMemoryPanel = true;

		private static Type _scribanParserType;

		private static Type _promptContextType;

		private static Type _promptManagerType;

		private static MethodInfo _renderMethod;

		private static PropertyInfo _lastContextProperty;

		private static bool _rimTalkTypesResolved;

		public override Vector2 InitialSize
		{
			get
			{
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				return new Vector2(1200f, 800f);
			}
		}

		public Dialog_InjectionPreview()
			: base((IWindowDrawing)null)
		{
			base.doCloseX = true;
			base.doCloseButton = true;
			base.closeOnClickedOutside = false;
			base.absorbInputAroundWindow = true;
			if (Find.CurrentMap != null)
			{
				selectedPawn = Find.CurrentMap.mapPawns.FreeColonists.FirstOrDefault();
			}
			LoadAvailableMatchingSources();
		}

		public override void DoWindowContents(Rect inRect)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			float num = 0f;
			Text.Font = (GameFont)2;
			GUI.color = new Color(1f, 0.9f, 0.7f);
			Widgets.Label(new Rect(0f, num, 600f, 35f), Translator.Translate("RimTalk_Preview_Title"));
			GUI.color = Color.white;
			Text.Font = (GameFont)1;
			num += 40f;
			DrawPawnSelectors(new Rect(0f, num, ((Rect)(ref inRect)).width, 80f));
			num += 85f;
			if (selectedPawn == null)
			{
				Text.Anchor = (TextAnchor)4;
				GUI.color = Color.gray;
				Widgets.Label(new Rect(0f, ((Rect)(ref inRect)).height / 2f - 20f, ((Rect)(ref inRect)).width, 40f), Translator.Translate("RimTalk_Preview_NoColonist"));
				GUI.color = Color.white;
				Text.Anchor = (TextAnchor)0;
				return;
			}
			if (Widgets.ButtonText(new Rect(((Rect)(ref inRect)).width - 240f, num, 110f, 30f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Preview_Help")), true, true, true, (TextAnchor?)null))
			{
				ShowHelpDialog();
			}
			if (Widgets.ButtonText(new Rect(((Rect)(ref inRect)).width - 120f, num, 110f, 30f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Preview_Refresh")), true, true, true, (TextAnchor?)null))
			{
				RefreshPreview();
			}
			num += 35f;
			float num2 = ((Rect)(ref inRect)).height - num - 50f;
			float num3 = (((Rect)(ref inRect)).width - 15f) / 2f;
			Rect rect = default(Rect);
			((Rect)(ref rect))..ctor(0f, num, num3, num2);
			DrawKnowledgePanel(rect);
			Rect rect2 = default(Rect);
			((Rect)(ref rect2))..ctor(num3 + 15f, num, num3, num2);
			DrawMemoryPanel(rect2);
		}

		private void DrawPawnSelectors(Rect rect)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_0164: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_020f: Unknown result type (might be due to invalid IL or missing references)
			//IL_023f: Unknown result type (might be due to invalid IL or missing references)
			//IL_024b: Unknown result type (might be due to invalid IL or missing references)
			//IL_026d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0277: Unknown result type (might be due to invalid IL or missing references)
			GUI.color = new Color(0.8f, 0.9f, 1f);
			Widgets.Label(new Rect(((Rect)(ref rect)).x, ((Rect)(ref rect)).y, 120f, 30f), Translator.Translate("RimTalk_Preview_CurrentPawn"));
			GUI.color = Color.white;
			Rect val = new Rect(((Rect)(ref rect)).x + 130f, ((Rect)(ref rect)).y, 200f, 30f);
			string text = ((selectedPawn != null) ? ((Entity)selectedPawn).LabelShort : TaggedString.op_Implicit(Translator.Translate("RimTalk_Preview_None")));
			if (Widgets.ButtonText(val, text, true, true, true, (TextAnchor?)null))
			{
				ShowPawnSelectionMenu(isPrimary: true);
			}
			if (selectedPawn != null)
			{
				GUI.color = Color.gray;
				string text2 = ((Def)((Thing)selectedPawn).def).label + " | " + GenderUtility.GetLabel(selectedPawn.gender, false);
				Widgets.Label(new Rect(((Rect)(ref rect)).x + 340f, ((Rect)(ref rect)).y + 5f, 300f, 30f), text2);
				GUI.color = Color.white;
			}
			float num = ((Rect)(ref rect)).y + 35f;
			GUI.color = new Color(1f, 0.9f, 0.8f);
			Widgets.Label(new Rect(((Rect)(ref rect)).x, num, 120f, 30f), Translator.Translate("RimTalk_Preview_TargetPawn"));
			GUI.color = Color.white;
			Rect val2 = new Rect(((Rect)(ref rect)).x + 130f, num, 200f, 30f);
			string text3 = ((targetPawn != null) ? ((Entity)targetPawn).LabelShort : TaggedString.op_Implicit(Translator.Translate("RimTalk_Preview_NoneClickToSelect")));
			if (Widgets.ButtonText(val2, text3, true, true, true, (TextAnchor?)null))
			{
				ShowPawnSelectionMenu(isPrimary: false);
			}
			if (targetPawn != null)
			{
				GUI.color = Color.gray;
				string text4 = ((Def)((Thing)targetPawn).def).label + " | " + GenderUtility.GetLabel(targetPawn.gender, false);
				Widgets.Label(new Rect(((Rect)(ref rect)).x + 340f, num + 5f, 250f, 30f), text4);
				GUI.color = Color.white;
				if (Widgets.ButtonText(new Rect(((Rect)(ref rect)).x + 600f, num, 80f, 30f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Preview_Clear")), true, true, true, (TextAnchor?)null))
				{
					targetPawn = null;
					RefreshPreview();
				}
			}
		}

		private void ShowPawnSelectionMenu(bool isPrimary)
		{
			//IL_025b: Unknown result type (might be due to invalid IL or missing references)
			//IL_024b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0255: Expected O, but got Unknown
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_021b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0225: Expected O, but got Unknown
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			if (Find.CurrentMap != null)
			{
				foreach (Pawn item in Find.CurrentMap.mapPawns.AllPawnsSpawned.Where((Pawn p) => p.RaceProps.Humanlike).OrderBy(delegate(Pawn p)
				{
					if (p.IsColonist)
					{
						return 1;
					}
					if (p.IsPrisoner)
					{
						return 2;
					}
					if (p.IsSlaveOfColony)
					{
						return 3;
					}
					return (p.HostFaction == Faction.OfPlayer) ? 4 : 5;
				}).ThenBy((Pawn p) => ((Entity)p).LabelShort))
				{
					Pawn localPawn = item;
					string text = ((Entity)item).LabelShort;
					if (item.IsColonist)
					{
						text = TaggedString.op_Implicit(text + (" " + Translator.Translate("RimTalk_Preview_Colonist")));
					}
					else if (item.IsPrisoner)
					{
						text = TaggedString.op_Implicit(text + (" " + Translator.Translate("RimTalk_Preview_Prisoner")));
					}
					else if (item.IsSlaveOfColony)
					{
						text = TaggedString.op_Implicit(text + (" " + Translator.Translate("RimTalk_Preview_Slave")));
					}
					else if (item.HostFaction == Faction.OfPlayer)
					{
						text = TaggedString.op_Implicit(text + (" " + Translator.Translate("RimTalk_Preview_Guest")));
					}
					else if (((Thing)item).Faction != null && ((Thing)item).Faction != Faction.OfPlayer)
					{
						text = text + " (" + ((Thing)item).Faction.Name + ")";
					}
					if (!isPrimary && selectedPawn != null && item == selectedPawn)
					{
						text = TaggedString.op_Implicit(text + (" " + Translator.Translate("RimTalk_Preview_SameAsCurrent")));
					}
					list.Add(new FloatMenuOption(text, (Action)delegate
					{
						if (isPrimary)
						{
							selectedPawn = localPawn;
							if (targetPawn == localPawn)
							{
								targetPawn = null;
							}
						}
						else
						{
							targetPawn = localPawn;
						}
						RefreshPreview();
					}, (MenuOptionPriority)4, (Action<Rect>)null, (Thing)null, 0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0));
				}
			}
			if (list.Count > 0)
			{
				Find.WindowStack.Add((Window)new FloatMenu(list));
			}
			else
			{
				Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_Preview_NoHumanlikes")), MessageTypeDefOf.RejectInput, false);
			}
		}

		private void DrawKnowledgePanel(Rect rect)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0201: Unknown result type (might be due to invalid IL or missing references)
			//IL_0232: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_0295: Unknown result type (might be due to invalid IL or missing references)
			//IL_029c: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_030e: Unknown result type (might be due to invalid IL or missing references)
			//IL_032b: Unknown result type (might be due to invalid IL or missing references)
			//IL_033b: Unknown result type (might be due to invalid IL or missing references)
			//IL_025f: Unknown result type (might be due to invalid IL or missing references)
			//IL_026f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0276: Unknown result type (might be due to invalid IL or missing references)
			//IL_0286: Unknown result type (might be due to invalid IL or missing references)
			//IL_0369: Unknown result type (might be due to invalid IL or missing references)
			//IL_037d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0397: Unknown result type (might be due to invalid IL or missing references)
			//IL_039c: Unknown result type (might be due to invalid IL or missing references)
			//IL_03af: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
			Widgets.DrawBoxSolid(rect, new Color(0.15f, 0.15f, 0.15f, 0.8f));
			Rect val = new Rect(((Rect)(ref rect)).x + 5f, ((Rect)(ref rect)).y + 5f, ((Rect)(ref rect)).width - 10f, 25f);
			Text.Font = (GameFont)2;
			GUI.color = new Color(1f, 1f, 0.8f);
			string text = (showKnowledgePanel ? "▼ " : "▶ ");
			text = TaggedString.op_Implicit(text + Translator.Translate("RimTalk_Preview_KnowledgeSection"));
			if (Widgets.ButtonText(val, text, false, true, true, (TextAnchor?)null))
			{
				showKnowledgePanel = !showKnowledgePanel;
			}
			Text.Font = (GameFont)1;
			GUI.color = Color.white;
			if (showKnowledgePanel)
			{
				float num = ((Rect)(ref rect)).y + 35f;
				float num2 = ((Rect)(ref rect)).width - 10f;
				Rect val2 = new Rect(((Rect)(ref rect)).x + 5f, num, num2, 25f);
				GUI.color = new Color(0.8f, 0.9f, 1f);
				string text2 = (showMatchSourcePanel ? "▼ " : "▶ ");
				text2 = TaggedString.op_Implicit(text2 + Translator.Translate("RimTalk_Preview_MatchingSource"));
				if (Widgets.ButtonText(val2, text2, false, true, true, (TextAnchor?)null))
				{
					showMatchSourcePanel = !showMatchSourcePanel;
				}
				GUI.color = Color.white;
				num += 28f;
				if (showMatchSourcePanel)
				{
					float num3 = 150f;
					Rect rect2 = default(Rect);
					((Rect)(ref rect2))..ctor(((Rect)(ref rect)).x + 5f, num, num2, num3);
					DrawMatchingSourceSelector(rect2);
					num += num3 + 5f;
				}
				Rect val3 = new Rect(((Rect)(ref rect)).x + 5f, num, num2, 25f);
				GUI.color = new Color(0.9f, 1f, 0.8f);
				Widgets.Label(val3, Translator.Translate("RimTalk_Preview_ParsedMatchText"));
				GUI.color = Color.white;
				num += 28f;
				float num4 = 180f;
				Rect val4 = default(Rect);
				((Rect)(ref val4))..ctor(((Rect)(ref rect)).x + 5f, num, num2, num4);
				Widgets.DrawBoxSolid(val4, new Color(0.1f, 0.1f, 0.1f, 0.5f));
				if (string.IsNullOrEmpty(parsedMatchText))
				{
					GUI.color = Color.gray;
					Text.Anchor = (TextAnchor)4;
					Widgets.Label(val4, Translator.Translate("RimTalk_Preview_NoParsedText"));
					Text.Anchor = (TextAnchor)0;
					GUI.color = Color.white;
				}
				else
				{
					Rect val5 = GenUI.ContractedBy(val4, 5f);
					float num5 = Text.CalcHeight(parsedMatchText, ((Rect)(ref val5)).width - 20f);
					Rect val6 = default(Rect);
					((Rect)(ref val6))..ctor(0f, 0f, ((Rect)(ref val5)).width - 20f, Mathf.Max(num5 + 10f, ((Rect)(ref val5)).height));
					Widgets.BeginScrollView(val5, ref scrollPositionParsedText, val6, true);
					GUI.color = new Color(0.85f, 0.85f, 0.85f);
					Widgets.Label(new Rect(0f, 0f, ((Rect)(ref val6)).width, num5), parsedMatchText);
					GUI.color = Color.white;
					Widgets.EndScrollView();
				}
				num += num4 + 5f;
				Rect val7 = new Rect(((Rect)(ref rect)).x + 5f, num, num2, 25f);
				GUI.color = new Color(1f, 0.9f, 0.7f);
				string text3 = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Preview_MatchedKnowledge", NamedArgument.op_Implicit(matchedKnowledge.Count)));
				Widgets.Label(val7, text3);
				GUI.color = Color.white;
				num += 28f;
				Rect rect3 = default(Rect);
				((Rect)(ref rect3))..ctor(((Rect)(ref rect)).x + 5f, num, num2, ((Rect)(ref rect)).yMax - num - 10f);
				DrawMatchedKnowledgeList(rect3);
			}
		}

		private void DrawMatchingSourceSelector(Rect rect)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			Widgets.DrawBoxSolid(rect, new Color(0.12f, 0.12f, 0.12f, 0.6f));
			if (availableMatchingSources == null || availableMatchingSources.Count == 0)
			{
				GUI.color = Color.gray;
				Widgets.Label(GenUI.ContractedBy(rect, 5f), Translator.Translate("RimTalk_Preview_NoMatchingSources"));
				GUI.color = Color.white;
				return;
			}
			RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
			if (settings == null)
			{
				return;
			}
			Rect val = GenUI.ContractedBy(rect, 5f);
			float num = 22f;
			int num2 = 3;
			float num3 = ((Rect)(ref val)).width / (float)num2;
			float num4 = Mathf.Ceil((float)availableMatchingSources.Count / (float)num2) * num;
			Rect val2 = default(Rect);
			((Rect)(ref val2))..ctor(0f, 0f, ((Rect)(ref val)).width - 20f, num4);
			Widgets.BeginScrollView(val, ref scrollPositionMatchSource, val2, true);
			int num5 = 0;
			foreach (var availableMatchingSource in availableMatchingSources)
			{
				int num6 = num5 % num2;
				int num7 = num5 / num2;
				Rect val3 = new Rect((float)num6 * num3, (float)num7 * num, num3 - 5f, num);
				bool flag = settings.knowledgeMatchingSources.Contains(availableMatchingSource.name);
				string text;
				if (!availableMatchingSource.isPawnProperty)
				{
					(text, _, _) = availableMatchingSource;
				}
				else
				{
					text = "[P] " + availableMatchingSource.name;
				}
				string text2 = text;
				TooltipHandler.TipRegion(val3, TipSignal.op_Implicit(availableMatchingSource.description));
				bool flag2 = flag;
				Widgets.CheckboxLabeled(val3, text2, ref flag2, false, (Texture2D)null, (Texture2D)null, false, false);
				if (flag2 != flag)
				{
					if (flag2)
					{
						if (!settings.knowledgeMatchingSources.Contains(availableMatchingSource.name))
						{
							settings.knowledgeMatchingSources.Add(availableMatchingSource.name);
						}
					}
					else
					{
						settings.knowledgeMatchingSources.Remove(availableMatchingSource.name);
					}
					RefreshParsedMatchText();
				}
				num5++;
			}
			Widgets.EndScrollView();
		}

		private void DrawMatchedKnowledgeList(Rect rect)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_022f: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			Widgets.DrawBoxSolid(rect, new Color(0.1f, 0.1f, 0.1f, 0.5f));
			if (matchedKnowledge.Count == 0)
			{
				GUI.color = Color.gray;
				Text.Anchor = (TextAnchor)4;
				Widgets.Label(rect, Translator.Translate("RimTalk_Preview_NoMatchedKnowledge"));
				Text.Anchor = (TextAnchor)0;
				GUI.color = Color.white;
				return;
			}
			Rect val = GenUI.ContractedBy(rect, 5f);
			float num = 60f;
			float num2 = (float)matchedKnowledge.Count * num;
			Rect val2 = default(Rect);
			((Rect)(ref val2))..ctor(0f, 0f, ((Rect)(ref val)).width - 20f, num2);
			Widgets.BeginScrollView(val, ref scrollPositionLeft, val2, true);
			Rect val3 = default(Rect);
			for (int i = 0; i < matchedKnowledge.Count; i++)
			{
				KnowledgeScore knowledgeScore = matchedKnowledge[i];
				((Rect)(ref val3))..ctor(0f, (float)i * num, ((Rect)(ref val2)).width, num - 5f);
				Widgets.DrawBoxSolid(val3, new Color(0.15f, 0.18f, 0.15f, 0.4f));
				GUI.color = new Color(0.9f, 0.9f, 0.6f);
				string text = TaggedString.op_Implicit($"[{i + 1}] [{knowledgeScore.Entry.tag}] " + TranslatorFormattedStringExtensions.Translate("RimTalk_Preview_Score", NamedArgument.op_Implicit(knowledgeScore.Score.ToString("F2"))));
				Widgets.Label(new Rect(((Rect)(ref val3)).x + 5f, ((Rect)(ref val3)).y + 2f, ((Rect)(ref val3)).width - 10f, 20f), text);
				GUI.color = Color.white;
				GUI.color = new Color(0.85f, 0.85f, 0.85f);
				string text2 = ((knowledgeScore.Entry.content.Length > 100) ? (knowledgeScore.Entry.content.Substring(0, 100) + "...") : knowledgeScore.Entry.content);
				Widgets.Label(new Rect(((Rect)(ref val3)).x + 5f, ((Rect)(ref val3)).y + 22f, ((Rect)(ref val3)).width - 10f, 35f), text2);
				GUI.color = Color.white;
			}
			Widgets.EndScrollView();
		}

		private void DrawMemoryPanel(Rect rect)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			Widgets.DrawBoxSolid(rect, new Color(0.15f, 0.15f, 0.18f, 0.8f));
			Rect val = new Rect(((Rect)(ref rect)).x + 5f, ((Rect)(ref rect)).y + 5f, ((Rect)(ref rect)).width - 10f, 25f);
			Text.Font = (GameFont)2;
			GUI.color = new Color(0.8f, 0.9f, 1f);
			string text = (showMemoryPanel ? "▼ " : "▶ ");
			text = TaggedString.op_Implicit(text + TranslatorFormattedStringExtensions.Translate("RimTalk_Preview_MemorySection", NamedArgument.op_Implicit(cachedMemoryCount)));
			if (Widgets.ButtonText(val, text, false, true, true, (TextAnchor?)null))
			{
				showMemoryPanel = !showMemoryPanel;
			}
			Text.Font = (GameFont)1;
			GUI.color = Color.white;
			if (!showMemoryPanel)
			{
				return;
			}
			float num = ((Rect)(ref rect)).y + 35f;
			if (selectedPawn != null)
			{
				FourLayerMemoryComp fourLayerMemoryComp = ThingCompUtility.TryGetComp<FourLayerMemoryComp>((Thing)(object)selectedPawn);
				if (fourLayerMemoryComp != null)
				{
					Rect rect2 = default(Rect);
					((Rect)(ref rect2))..ctor(((Rect)(ref rect)).x + 5f, num, ((Rect)(ref rect)).width - 10f, 50f);
					DrawMemoryStats(rect2, fourLayerMemoryComp);
					num += 55f;
				}
			}
			Rect rect3 = default(Rect);
			((Rect)(ref rect3))..ctor(((Rect)(ref rect)).x + 5f, num, ((Rect)(ref rect)).width - 10f, ((Rect)(ref rect)).yMax - num - 10f);
			DrawMemoryPreviewContent(rect3);
		}

		private void DrawMemoryStats(Rect rect, FourLayerMemoryComp memoryComp)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			Widgets.DrawBoxSolid(rect, new Color(0.1f, 0.1f, 0.12f, 0.5f));
			float num = ((Rect)(ref rect)).x + 5f;
			float num2 = 22f;
			GUI.color = new Color(0.7f, 0.7f, 1f);
			Widgets.Label(new Rect(num, ((Rect)(ref rect)).y + 3f, 100f, num2), Translator.Translate("RimTalk_Preview_MemoryLayers"));
			GUI.color = Color.white;
			float num3 = num + 100f;
			Widgets.Label(new Rect(num3, ((Rect)(ref rect)).y + 3f, 120f, num2), $"ABM: {memoryComp.ActiveMemories.Count}");
			float num4 = num3 + 100f;
			Widgets.Label(new Rect(num4, ((Rect)(ref rect)).y + 3f, 120f, num2), $"SCM: {memoryComp.SituationalMemories.Count}");
			float num5 = num4 + 100f;
			Widgets.Label(new Rect(num5, ((Rect)(ref rect)).y + 3f, 120f, num2), $"ELS: {memoryComp.EventLogMemories.Count}");
			Widgets.Label(new Rect(num5 + 100f, ((Rect)(ref rect)).y + 3f, 120f, num2), $"CLPA: {memoryComp.ArchiveMemories.Count}");
			RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
			if (settings != null)
			{
				float num6 = ((Rect)(ref rect)).x + 5f;
				GUI.color = new Color(0.8f, 0.8f, 1f);
				string text = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Preview_InjectionConfig", NamedArgument.op_Implicit(settings.maxInjectedMemories), NamedArgument.op_Implicit(settings.maxABMInjectionRounds)));
				Widgets.Label(new Rect(num6, ((Rect)(ref rect)).y + 25f, ((Rect)(ref rect)).width - 10f, num2), text);
				GUI.color = Color.white;
			}
		}

		private void DrawMemoryPreviewContent(Rect rect)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			Widgets.DrawBoxSolid(rect, new Color(0.08f, 0.08f, 0.1f, 0.6f));
			if (string.IsNullOrEmpty(memoryPreviewText))
			{
				GUI.color = Color.gray;
				Text.Anchor = (TextAnchor)4;
				Widgets.Label(rect, Translator.Translate("RimTalk_Preview_ClickRefresh"));
				Text.Anchor = (TextAnchor)0;
				GUI.color = Color.white;
			}
			else
			{
				Rect val = GenUI.ContractedBy(rect, 5f);
				float num = Text.CalcHeight(memoryPreviewText, ((Rect)(ref val)).width - 20f);
				Rect val2 = default(Rect);
				((Rect)(ref val2))..ctor(0f, 0f, ((Rect)(ref val)).width - 20f, num + 20f);
				Widgets.BeginScrollView(val, ref scrollPositionRight, val2, true);
				GUI.color = new Color(0.9f, 0.9f, 0.9f);
				Widgets.Label(new Rect(0f, 0f, ((Rect)(ref val2)).width, num), memoryPreviewText);
				GUI.color = Color.white;
				Widgets.EndScrollView();
			}
		}

		private void LoadAvailableMatchingSources()
		{
			availableMatchingSources = MustacheVariableHelper.GetMatchingPropertyCategories();
			if (availableMatchingSources == null || availableMatchingSources.Count == 0)
			{
				availableMatchingSources = new List<(string, string, bool)>
				{
					("prompt", "Dialogue prompt", false),
					("fullname", "Pawn full name", true),
					("role", "Pawn role", true),
					("age", "Pawn age", true),
					("gender", "Pawn gender", true),
					("backstory", "Pawn backstory", true),
					("traits", "Pawn traits", true),
					("skills", "Skills", true),
					("relations", "Relations", true)
				};
			}
			ResolveRimTalkTypes();
		}

		private void ResolveRimTalkTypes()
		{
			if (_rimTalkTypesResolved)
			{
				return;
			}
			_rimTalkTypesResolved = true;
			try
			{
				Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.GetName().Name == "RimTalk");
				if (!(assembly == null))
				{
					_scribanParserType = assembly.GetType("RimTalk.Prompt.ScribanParser");
					_promptContextType = assembly.GetType("RimTalk.Prompt.PromptContext");
					_promptManagerType = assembly.GetType("RimTalk.Prompt.PromptManager");
					if (_scribanParserType != null)
					{
						_renderMethod = _scribanParserType.GetMethod("Render", BindingFlags.Static | BindingFlags.Public);
					}
					if (_promptManagerType != null)
					{
						_lastContextProperty = _promptManagerType.GetProperty("LastContext", BindingFlags.Static | BindingFlags.Public);
					}
				}
			}
			catch
			{
			}
		}

		private void RefreshPreview()
		{
			RefreshParsedMatchText();
			RefreshMatchedKnowledge();
			RefreshMemoryPreview();
		}

		private void RefreshParsedMatchText()
		{
			RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
			if (selectedPawn == null || settings == null || settings.knowledgeMatchingSources.Count == 0)
			{
				parsedMatchText = "";
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			List<Pawn> list = new List<Pawn> { selectedPawn };
			if (targetPawn != null && targetPawn != selectedPawn)
			{
				list.Add(targetPawn);
			}
			foreach (string sourceName in settings.knowledgeMatchingSources)
			{
				try
				{
					if (GenCollection.Any<(string, string, bool)>(availableMatchingSources, (Predicate<(string, string, bool)>)(((string name, string description, bool isPawnProperty) s) => s.name == sourceName && s.isPawnProperty)))
					{
						List<string> list2 = new List<string>();
						List<string> list3 = new List<string>();
						foreach (Pawn item in list)
						{
							string text = RenderWithScriban("{{ pawn." + sourceName + " }}", item, null);
							if (!string.IsNullOrEmpty(text) && !text.Contains("{{"))
							{
								list2.Add(text);
								list3.Add("@" + ((Entity)item).LabelShort);
							}
						}
						if (list2.Count > 0)
						{
							if (stringBuilder.Length > 0)
							{
								stringBuilder.AppendLine();
							}
							stringBuilder.AppendLine("[" + sourceName + " " + string.Join(" ", list3) + "]");
							stringBuilder.Append(string.Join(",\n", list2));
						}
						continue;
					}
					string text2 = RenderWithScriban("{{ " + sourceName + " }}", selectedPawn, targetPawn, isPawnProperty: false);
					if (!string.IsNullOrEmpty(text2) && !text2.Contains("{{"))
					{
						if (stringBuilder.Length > 0)
						{
							stringBuilder.AppendLine();
						}
						stringBuilder.AppendLine("[" + sourceName + "]");
						stringBuilder.Append(text2);
					}
				}
				catch
				{
				}
			}
			parsedMatchText = stringBuilder.ToString();
		}

		private string RenderWithScriban(string template, Pawn pawn, Pawn recipient, bool isPawnProperty = true)
		{
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			if (_scribanParserType == null || _renderMethod == null)
			{
				return template;
			}
			try
			{
				object obj;
				if (isPawnProperty)
				{
					if (_promptContextType == null)
					{
						return template;
					}
					obj = Activator.CreateInstance(_promptContextType, pawn, null);
					if (obj == null)
					{
						return template;
					}
					if (recipient != null)
					{
						PropertyInfo property = _promptContextType.GetProperty("AllPawns");
						if (property != null)
						{
							List<Pawn> value = new List<Pawn> { pawn, recipient };
							property.SetValue(obj, value);
						}
					}
					PropertyInfo property2 = _promptContextType.GetProperty("IsPreview");
					if (property2 != null)
					{
						property2.SetValue(obj, true);
					}
				}
				else
				{
					if (_lastContextProperty == null)
					{
						return template;
					}
					obj = _lastContextProperty.GetValue(null);
					if (obj == null)
					{
						return TaggedString.op_Implicit(Translator.Translate("RimTalk_Preview_NoContext"));
					}
					PropertyInfo propertyInfo = _promptContextType?.GetProperty("IsPreview");
					if (propertyInfo != null)
					{
						propertyInfo.SetValue(obj, true);
					}
				}
				return (_renderMethod.Invoke(null, new object[3] { template, obj, false }) as string) ?? template;
			}
			catch
			{
				return template;
			}
		}

		private void RefreshMatchedKnowledge()
		{
			matchedKnowledge.Clear();
			if (selectedPawn == null || string.IsNullOrEmpty(parsedMatchText))
			{
				return;
			}
			World world = Find.World;
			MemoryManager memoryManager = ((world != null) ? world.GetComponent<MemoryManager>() : null);
			if (memoryManager?.CommonKnowledge == null)
			{
				return;
			}
			RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
			if (settings == null)
			{
				return;
			}
			try
			{
				memoryManager.CommonKnowledge.InjectKnowledgeWithDetails(parsedMatchText, settings.maxInjectedKnowledge, out var scores, selectedPawn, targetPawn);
				if (scores != null)
				{
					matchedKnowledge = scores;
				}
			}
			catch
			{
			}
		}

		private void RefreshMemoryPreview()
		{
			//IL_029f: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			memoryPreviewText = "";
			cachedMemoryCount = 0;
			if (selectedPawn == null)
			{
				return;
			}
			FourLayerMemoryComp fourLayerMemoryComp = ThingCompUtility.TryGetComp<FourLayerMemoryComp>((Thing)(object)selectedPawn);
			if (fourLayerMemoryComp == null)
			{
				memoryPreviewText = TaggedString.op_Implicit(Translator.Translate("RimTalk_Preview_NoMemoryComp"));
				return;
			}
			RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
			if (settings == null)
			{
				return;
			}
			try
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (settings.useDynamicInjection)
				{
					DynamicMemoryInjection.InjectMemoriesWithDetails(fourLayerMemoryComp, parsedMatchText, settings.maxInjectedMemories, out var scores);
					if (scores != null && scores.Count > 0)
					{
						cachedMemoryCount = scores.Count;
						stringBuilder.AppendLine(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Preview_SelectedMemories", NamedArgument.op_Implicit(scores.Count))));
						stringBuilder.AppendLine();
						for (int i = 0; i < scores.Count; i++)
						{
							DynamicMemoryInjection.MemoryScore memoryScore = scores[i];
							MemoryEntry memory = memoryScore.Memory;
							string layerTag = GetLayerTag(memory.Layer);
							stringBuilder.AppendLine($"[{i + 1}] {layerTag} {GetTypeTag(memory.Type)}");
							stringBuilder.AppendLine(TaggedString.op_Implicit("    " + TranslatorFormattedStringExtensions.Translate("RimTalk_Preview_ScoreDetail", NamedArgument.op_Implicit(memoryScore.TotalScore.ToString("F3")))));
							stringBuilder.AppendLine("    " + memory.Content);
							stringBuilder.AppendLine();
						}
					}
					else
					{
						stringBuilder.AppendLine(TaggedString.op_Implicit(Translator.Translate("RimTalk_Preview_NoMemoriesAboveThreshold")));
					}
				}
				else
				{
					stringBuilder.AppendLine(TaggedString.op_Implicit(Translator.Translate("RimTalk_Preview_StaticMode")));
					stringBuilder.AppendLine();
					int num = 0;
					foreach (MemoryEntry item in fourLayerMemoryComp.EventLogMemories.Take(5))
					{
						num++;
						stringBuilder.AppendLine($"[ELS-{num}] {item.Content}");
					}
					foreach (MemoryEntry item2 in fourLayerMemoryComp.ArchiveMemories.Take(5))
					{
						num++;
						stringBuilder.AppendLine($"[CLPA-{num}] {item2.Content}");
					}
					cachedMemoryCount = num;
				}
				memoryPreviewText = stringBuilder.ToString();
			}
			catch (Exception ex)
			{
				memoryPreviewText = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Preview_Error", NamedArgument.op_Implicit(ex.Message)));
			}
		}

		private string GetLayerTag(MemoryLayer layer)
		{
			return layer switch
			{
				MemoryLayer.Active => "[ABM]", 
				MemoryLayer.Situational => "[SCM]", 
				MemoryLayer.EventLog => "[ELS]", 
				MemoryLayer.Archive => "[CLPA]", 
				_ => "[???]", 
			};
		}

		private string GetTypeTag(MemoryType type)
		{
			return type switch
			{
				MemoryType.Conversation => "\ud83d\udcac", 
				MemoryType.Action => "⚡", 
				_ => "\ud83d\udcdd", 
			};
		}

		private void ShowHelpDialog()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			string text = TaggedString.op_Implicit(Translator.Translate("RimTalk_Preview_HelpContent"));
			Find.WindowStack.Add((Window)new Dialog_MessageBox(TaggedString.op_Implicit(text), TaggedString.op_Implicit(Translator.Translate("RimTalk_Close")), (Action)null, (string)null, (Action)null, (string)null, false, (Action)null, (Action)null, (WindowLayer)1));
		}
	}
}
namespace RimTalk.Memory.UI
{
	public static class CommonKnowledgeTranslationKeys
	{
		public const string Categories = "RimTalk_Knowledge_Categories";

		public const string CategoryAll = "RimTalk_Knowledge_CategoryAll";

		public const string CategoryInstructions = "RimTalk_Knowledge_CategoryInstructions";

		public const string CategoryLore = "RimTalk_Knowledge_CategoryLore";

		public const string CategoryPawnStatus = "RimTalk_Knowledge_CategoryPawnStatus";

		public const string CategoryHistory = "RimTalk_Knowledge_CategoryHistory";

		public const string CategoryOther = "RimTalk_Knowledge_CategoryOther";

		public const string CategoryUnknown = "RimTalk_Knowledge_CategoryUnknown";

		public const string New = "RimTalk_Knowledge_New";

		public const string Import = "RimTalk_Knowledge_Import";

		public const string Export = "RimTalk_Knowledge_Export";

		public const string ExportAll = "RimTalk_Knowledge_ExportAll";

		public const string ExportCount = "RimTalk_Knowledge_ExportCount";

		public const string DeleteCount = "RimTalk_Knowledge_DeleteCount";

		public const string ClearAll = "RimTalk_Knowledge_ClearAll";

		public const string Total = "RimTalk_Knowledge_Total";

		public const string Enabled = "RimTalk_Knowledge_Enabled";

		public const string Selected = "RimTalk_Knowledge_Selected";

		public const string Showing = "RimTalk_Knowledge_Showing";

		public const string AutoGenerate = "RimTalk_Knowledge_AutoGenerate";

		public const string PawnStatus = "RimTalk_Knowledge_PawnStatus";

		public const string EventRecord = "RimTalk_Knowledge_EventRecord";

		public const string GenerateNow = "RimTalk_Knowledge_GenerateNow";

		public const string GeneratedPawnStatus = "RimTalk_Knowledge_GeneratedPawnStatus";

		public const string EventScanTriggered = "RimTalk_Knowledge_EventScanTriggered";

		public const string NoColonists = "RimTalk_Knowledge_NoColonists";

		public const string GenerationFailed = "RimTalk_Knowledge_GenerationFailed";

		public const string Details = "RimTalk_Knowledge_Details";

		public const string Edit = "RimTalk_Knowledge_Edit";

		public const string Tag = "RimTalk_Knowledge_Tag";

		public const string Importance = "RimTalk_Knowledge_Importance";

		public const string Status = "RimTalk_Knowledge_Status";

		public const string StatusEnabled = "RimTalk_Knowledge_StatusEnabled";

		public const string StatusDisabled = "RimTalk_Knowledge_StatusDisabled";

		public const string Visibility = "RimTalk_Knowledge_Visibility";

		public const string Content = "RimTalk_Knowledge_Content";

		public const string Delete = "RimTalk_Knowledge_Delete";

		public const string Global = "RimTalk_Knowledge_Global";

		public const string GlobalAll = "RimTalk_Knowledge_GlobalAll";

		public const string ExclusiveTo = "RimTalk_Knowledge_ExclusiveTo";

		public const string VisibilityGlobal = "RimTalk_Knowledge_VisibilityGlobal";

		public const string VisibilityExclusive = "RimTalk_Knowledge_VisibilityExclusive";

		public const string VisibilityDeleted = "RimTalk_Knowledge_VisibilityDeleted";

		public const string NewEntry = "RimTalk_Knowledge_NewEntry";

		public const string EditEntry = "RimTalk_Knowledge_EditEntry";

		public const string Save = "RimTalk_Knowledge_Save";

		public const string Cancel = "RimTalk_Knowledge_Cancel";

		public const string ItemsSelected = "RimTalk_Knowledge_ItemsSelected";

		public const string EnabledCount = "RimTalk_Knowledge_EnabledCount";

		public const string DisabledCount = "RimTalk_Knowledge_DisabledCount";

		public const string AvgImportance = "RimTalk_Knowledge_AvgImportance";

		public const string EnableAll = "RimTalk_Knowledge_EnableAll";

		public const string DisableAll = "RimTalk_Knowledge_DisableAll";

		public const string ExportItems = "RimTalk_Knowledge_ExportItems";

		public const string DeleteItems = "RimTalk_Knowledge_DeleteItems";

		public const string SelectOrCreate = "RimTalk_Knowledge_SelectOrCreate";

		public const string ImportTitle = "RimTalk_Knowledge_ImportTitle";

		public const string ImportDescription = "RimTalk_Knowledge_ImportDescription";

		public const string Imported = "RimTalk_Knowledge_Imported";

		public const string ExportedToClipboard = "RimTalk_Knowledge_ExportedToClipboard";

		public const string DeleteConfirm = "RimTalk_Knowledge_DeleteConfirm";

		public const string Deleted = "RimTalk_Knowledge_Deleted";

		public const string ClearConfirm = "RimTalk_Knowledge_ClearConfirm";

		public const string AllCleared = "RimTalk_Knowledge_AllCleared";

		public const string TagContentEmpty = "RimTalk_Knowledge_TagContentEmpty";

		public const string EntrySaved = "RimTalk_Knowledge_EntrySaved";

		public const string Help = "RimTalk_Knowledge_Help";

		public const string HelpTitle = "RimTalk_Knowledge_HelpTitle";

		public const string HelpContent = "RimTalk_Knowledge_HelpContent";

		public const string TagTest = "RimTalk_Knowledge_TagTest";

		public const string TagTestTitle = "RimTalk_Knowledge_TagTestTitle";

		public const string TagTestInputTag = "RimTalk_Knowledge_TagTestInputTag";

		public const string TagTestInputContext = "RimTalk_Knowledge_TagTestInputContext";

		public const string TagTestSelectPawn = "RimTalk_Knowledge_TagTestSelectPawn";

		public const string TagTestNoPawn = "RimTalk_Knowledge_TagTestNoPawn";

		public const string TagTestResult = "RimTalk_Knowledge_TagTestResult";

		public const string TagTestMatched = "RimTalk_Knowledge_TagTestMatched";

		public const string TagTestNotMatched = "RimTalk_Knowledge_TagTestNotMatched";

		public const string TagTestMatchText = "RimTalk_Knowledge_TagTestMatchText";

		public const string TagTestClear = "RimTalk_Knowledge_TagTestClear";
	}
	public static class CommonKnowledgeUIHelpers
	{
		private static readonly Color ColorInstructions;

		private static readonly Color ColorLore;

		private static readonly Color ColorPawnStatus;

		private static readonly Color ColorHistory;

		private static readonly Color ColorOther;

		public static string GetCategoryLabel(KnowledgeCategory category)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			return category switch
			{
				KnowledgeCategory.All => TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_CategoryAll")), 
				KnowledgeCategory.Instructions => TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_CategoryInstructions")), 
				KnowledgeCategory.Lore => TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_CategoryLore")), 
				KnowledgeCategory.PawnStatus => TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_CategoryPawnStatus")), 
				KnowledgeCategory.History => TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_CategoryHistory")), 
				KnowledgeCategory.Other => TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_CategoryOther")), 
				_ => TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_CategoryUnknown")), 
			};
		}

		public static Color GetCategoryColor(CommonKnowledgeEntry entry)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			return (Color)(GetEntryCategory(entry) switch
			{
				KnowledgeCategory.Instructions => ColorInstructions, 
				KnowledgeCategory.Lore => ColorLore, 
				KnowledgeCategory.PawnStatus => ColorPawnStatus, 
				KnowledgeCategory.History => ColorHistory, 
				_ => ColorOther, 
			});
		}

		public static KnowledgeCategory GetEntryCategory(CommonKnowledgeEntry entry)
		{
			if (entry.category != KnowledgeEntryCategory.None)
			{
				return ExplicitCategoryToKnowledgeCategory(entry.category);
			}
			if (string.IsNullOrEmpty(entry.tag))
			{
				return KnowledgeCategory.Other;
			}
			string text = entry.tag.ToLower();
			if (text.Contains("规则") || text.Contains("instructions") || text.Contains("instruction") || text.Contains("rule"))
			{
				return KnowledgeCategory.Instructions;
			}
			if (text.Contains("殖民者状态") || text.Contains("pawnstatus") || text.Contains("colonist") || text.Contains("状态"))
			{
				return KnowledgeCategory.PawnStatus;
			}
			if (text.Contains("历史") || text.Contains("history") || text.Contains("past") || text.Contains("记录"))
			{
				return KnowledgeCategory.History;
			}
			if (text.Contains("世界观") || text.Contains("lore") || text.Contains("background") || text.Contains("背景") || text.Contains("设定"))
			{
				return KnowledgeCategory.Lore;
			}
			return KnowledgeCategory.Other;
		}

		public static KnowledgeCategory ExplicitCategoryToKnowledgeCategory(KnowledgeEntryCategory cat)
		{
			return cat switch
			{
				KnowledgeEntryCategory.Instructions => KnowledgeCategory.Instructions, 
				KnowledgeEntryCategory.Lore => KnowledgeCategory.Lore, 
				KnowledgeEntryCategory.PawnStatus => KnowledgeCategory.PawnStatus, 
				KnowledgeEntryCategory.History => KnowledgeCategory.History, 
				KnowledgeEntryCategory.Other => KnowledgeCategory.Other, 
				_ => KnowledgeCategory.Other, 
			};
		}

		public static KnowledgeEntryCategory KnowledgeCategoryToExplicit(KnowledgeCategory cat)
		{
			return cat switch
			{
				KnowledgeCategory.Instructions => KnowledgeEntryCategory.Instructions, 
				KnowledgeCategory.Lore => KnowledgeEntryCategory.Lore, 
				KnowledgeCategory.PawnStatus => KnowledgeEntryCategory.PawnStatus, 
				KnowledgeCategory.History => KnowledgeEntryCategory.History, 
				KnowledgeCategory.Other => KnowledgeEntryCategory.Other, 
				_ => KnowledgeEntryCategory.None, 
			};
		}

		public static string GetExplicitCategoryLabel(KnowledgeEntryCategory cat)
		{
			return cat switch
			{
				KnowledgeEntryCategory.None => "自动推断", 
				KnowledgeEntryCategory.Instructions => "指令规则", 
				KnowledgeEntryCategory.Lore => "世界观设定", 
				KnowledgeEntryCategory.PawnStatus => "殖民者状态", 
				KnowledgeEntryCategory.History => "历史记录", 
				KnowledgeEntryCategory.Other => "其他", 
				_ => "未知", 
			};
		}

		public static string GetVisibilityText(CommonKnowledgeEntry entry)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			if (entry.targetPawnId == -1)
			{
				return TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_VisibilityGlobal"));
			}
			Pawn val = Find.Maps?.SelectMany((Map m) => m.mapPawns.FreeColonists).FirstOrDefault((Pawn p) => ((Thing)p).thingIDNumber == entry.targetPawnId);
			return TaggedString.op_Implicit((val != null) ? TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_VisibilityExclusive", NamedArgument.op_Implicit(((Entity)val).LabelShort)) : TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_VisibilityDeleted", NamedArgument.op_Implicit(entry.targetPawnId)));
		}

		public static void DrawDetailField(Rect rect, string label, string value)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			float num = 100f;
			Text.Font = (GameFont)0;
			GUI.color = new Color(0.7f, 0.7f, 0.7f);
			Widgets.Label(new Rect(((Rect)(ref rect)).x, ((Rect)(ref rect)).y, num, ((Rect)(ref rect)).height), EnsureColon(label));
			GUI.color = Color.white;
			Text.Font = (GameFont)1;
			Widgets.Label(new Rect(((Rect)(ref rect)).x + num, ((Rect)(ref rect)).y, ((Rect)(ref rect)).width - num, ((Rect)(ref rect)).height), value);
		}

		public static string EnsureColon(string label)
		{
			if (string.IsNullOrEmpty(label))
			{
				return ":";
			}
			if (!label.EndsWith(":") && !label.EndsWith("："))
			{
				return label + ":";
			}
			return label;
		}

		public static void DrawColoredCheckbox(Rect rect, string label, ref bool value, Color color)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			Widgets.DrawBoxSolid(new Rect(((Rect)(ref rect)).x, ((Rect)(ref rect)).y + 2f, 3f, ((Rect)(ref rect)).height - 4f), color);
			Widgets.CheckboxLabeled(new Rect(((Rect)(ref rect)).x + 8f, ((Rect)(ref rect)).y, ((Rect)(ref rect)).width - 8f, ((Rect)(ref rect)).height), label, ref value, false, (Texture2D)null, (Texture2D)null, false, false);
		}

		public static bool DrawCategoryButton(Rect rect, KnowledgeCategory category, bool isSelected, int count)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			string categoryLabel = GetCategoryLabel(category);
			string text = $"{categoryLabel} ({count})";
			if (isSelected)
			{
				Widgets.DrawHighlightSelected(rect);
			}
			else if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			return Widgets.ButtonText(rect, text, false, true, true, (TextAnchor?)null);
		}

		public static void DrawAutoGenerateSettings(Rect rect, Action onGeneratePawnStatus, Action onGenerateEventRecord)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			Widgets.DrawBoxSolid(rect, new Color(0.1f, 0.1f, 0.1f, 0.3f));
			Rect val = GenUI.ContractedBy(rect, 5f);
			float y = ((Rect)(ref val)).y;
			RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
			bool enablePawnStatusKnowledge = settings.enablePawnStatusKnowledge;
			Widgets.CheckboxLabeled(new Rect(((Rect)(ref val)).x, y, ((Rect)(ref val)).width, 25f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_PawnStatus")), ref enablePawnStatusKnowledge, false, (Texture2D)null, (Texture2D)null, false, false);
			settings.enablePawnStatusKnowledge = enablePawnStatusKnowledge;
			y += 30f;
			if (Widgets.ButtonText(new Rect(((Rect)(ref val)).x, y, ((Rect)(ref val)).width, 25f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_GenerateNow")), true, true, true, (TextAnchor?)null))
			{
				onGeneratePawnStatus?.Invoke();
			}
			y += 30f;
			bool enableEventRecordKnowledge = settings.enableEventRecordKnowledge;
			Widgets.CheckboxLabeled(new Rect(((Rect)(ref val)).x, y, ((Rect)(ref val)).width, 25f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_EventRecord")), ref enableEventRecordKnowledge, false, (Texture2D)null, (Texture2D)null, false, false);
			settings.enableEventRecordKnowledge = enableEventRecordKnowledge;
			y += 30f;
			if (Widgets.ButtonText(new Rect(((Rect)(ref val)).x, y, ((Rect)(ref val)).width, 25f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_GenerateNow")), true, true, true, (TextAnchor?)null))
			{
				onGenerateEventRecord?.Invoke();
			}
		}

		public static void ShowPawnSelectionMenu(Action<int> onSelected)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Expected O, but got Unknown
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Expected O, but got Unknown
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_GlobalAll")), (Action)delegate
			{
				onSelected(-1);
			}, (MenuOptionPriority)4, (Action<Rect>)null, (Thing)null, 0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0));
			List<Pawn> list2 = Find.Maps?.SelectMany((Map m) => m.mapPawns.FreeColonists).ToList();
			if (list2 != null && list2.Count > 0)
			{
				foreach (Pawn item in list2.OrderBy((Pawn p) => ((Entity)p).LabelShort))
				{
					int pawnId = ((Thing)item).thingIDNumber;
					list.Add(new FloatMenuOption(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_ExclusiveTo", NamedArgument.op_Implicit(((Entity)item).LabelShort))), (Action)delegate
					{
						onSelected(pawnId);
					}, (MenuOptionPriority)4, (Action<Rect>)null, (Thing)null, 0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0));
				}
			}
			Find.WindowStack.Add((Window)new FloatMenu(list));
		}

		static CommonKnowledgeUIHelpers()
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			ColorInstructions = new Color(0.3f, 0.8f, 0.3f);
			ColorLore = new Color(0.8f, 0.6f, 0.3f);
			ColorPawnStatus = new Color(0.3f, 0.6f, 0.9f);
			ColorHistory = new Color(0.7f, 0.5f, 0.7f);
			ColorOther = Color.white;
		}
	}
	public class Dialog_CommonKnowledge : Window
	{
		private CommonKnowledgeLibrary library;

		private HashSet<CommonKnowledgeEntry> selectedEntries;

		private CommonKnowledgeEntry lastSelectedEntry;

		private bool isDragging;

		private bool isMouseDown;

		private Vector2 dragStartPos;

		private Vector2 dragCurrentPos;

		private Vector2 mouseDownScreenPos;

		private const float DRAG_THRESHOLD = 5f;

		private bool isDragReordering;

		private int dragReorderInsertIndex;

		private bool mouseDownOnSelected;

		private Vector2 listScrollPosition;

		private Vector2 detailScrollPosition;

		private string searchFilter;

		private bool editMode;

		private bool showRightPanel;

		private KnowledgeCategory currentCategory;

		private bool showAutoGenerateSettings;

		private VirtualListView<CommonKnowledgeEntry> virtualList;

		private string editTag;

		private string editContent;

		private float editImportance;

		private int editTargetPawnId;

		private KeywordMatchMode editMatchMode;

		private KnowledgeEntryCategory editCategory;

		private const float TOOLBAR_HEIGHT = 45f;

		private const float SIDEBAR_WIDTH = 240f;

		private const float RIGHT_PANEL_WIDTH = 340f;

		private const float SPACING = 10f;

		private const float BUTTON_HEIGHT = 32f;

		private const float ENTRY_HEIGHT = 70f;

		public override Vector2 InitialSize
		{
			get
			{
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				return new Vector2(1200f, 800f);
			}
		}

		public Dialog_CommonKnowledge(CommonKnowledgeLibrary library)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			selectedEntries = new HashSet<CommonKnowledgeEntry>();
			dragStartPos = Vector2.zero;
			dragCurrentPos = Vector2.zero;
			mouseDownScreenPos = Vector2.zero;
			dragReorderInsertIndex = -1;
			listScrollPosition = Vector2.zero;
			detailScrollPosition = Vector2.zero;
			searchFilter = "";
			showRightPanel = true;
			editTag = "";
			editContent = "";
			editImportance = 0.5f;
			editTargetPawnId = -1;
			((Window)this)..ctor((IWindowDrawing)null);
			this.library = library;
			base.doCloseX = true;
			base.doCloseButton = false;
			base.closeOnClickedOutside = false;
			base.absorbInputAroundWindow = true;
			base.forcePause = false;
			virtualList = new VirtualListView<CommonKnowledgeEntry>((CommonKnowledgeEntry entry) => 70f, delegate(Rect rect, CommonKnowledgeEntry entry, int index)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				DrawEntryRow(rect, entry);
			});
			virtualList.EmptyLabel = TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_SelectOrCreate"));
		}

		public override void DoWindowContents(Rect inRect)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Invalid comparison between Unknown and I4
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Unknown result type (might be due to invalid IL or missing references)
			Rect rect = default(Rect);
			((Rect)(ref rect))..ctor(0f, 0f, ((Rect)(ref inRect)).width, 45f);
			DrawToolbar(rect);
			float num = 55f;
			float num2 = ((Rect)(ref inRect)).height - num;
			Rect rect2 = default(Rect);
			((Rect)(ref rect2))..ctor(0f, num, 240f, num2);
			DrawSidebar(rect2);
			float num3 = 250f;
			float num4 = (showRightPanel ? (((Rect)(ref inRect)).width - 240f - 340f - 30f) : (((Rect)(ref inRect)).width - 240f - 20f));
			Rect rect3 = default(Rect);
			((Rect)(ref rect3))..ctor(num3, num, num4, num2);
			DrawCenterList(rect3);
			if (showRightPanel)
			{
				float num5 = num3 + num4 + 10f;
				Rect rect4 = default(Rect);
				((Rect)(ref rect4))..ctor(num5, num, 340f, num2);
				DrawRightPanel(rect4);
			}
			if ((int)Event.current.type != 1 || Event.current.button != 0)
			{
				return;
			}
			if (isMouseDown && isDragReordering && dragReorderInsertIndex >= 0)
			{
				ExecuteDragReorder();
			}
			else if (isMouseDown && !isDragging)
			{
				float num6 = 250f;
				float num7 = (showRightPanel ? (((Rect)(ref inRect)).width - 240f - 340f - 30f) : (((Rect)(ref inRect)).width - 240f - 20f));
				Rect val = GenUI.ContractedBy(new Rect(num6, num, num7, num2), 5f);
				if (((Rect)(ref val)).Contains(Event.current.mousePosition))
				{
					float num8 = Event.current.mousePosition.y - ((Rect)(ref val)).y + listScrollPosition.y;
					List<CommonKnowledgeEntry> filteredEntries = GetFilteredEntries();
					int num9 = (int)(num8 / 70f);
					if (num9 >= 0 && num9 < filteredEntries.Count)
					{
						HandleEntryClick(filteredEntries[num9]);
					}
					else
					{
						selectedEntries.Clear();
						lastSelectedEntry = null;
					}
				}
			}
			isMouseDown = false;
			isDragging = false;
			isDragReordering = false;
			dragReorderInsertIndex = -1;
			mouseDownOnSelected = false;
			Event.current.Use();
		}

		private void DrawToolbar(Rect rect)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Unknown result type (might be due to invalid IL or missing references)
			//IL_025e: Unknown result type (might be due to invalid IL or missing references)
			GUI.Box(rect, "");
			Rect val = GenUI.ContractedBy(rect, 5f);
			float x = ((Rect)(ref val)).x;
			float num = 300f;
			Rect val2 = default(Rect);
			((Rect)(ref val2))..ctor(x, ((Rect)(ref val)).y + 5f, num, 30f);
			searchFilter = Widgets.TextField(val2, searchFilter);
			float num2 = 100f;
			float num3 = 5f;
			float num4 = ((Rect)(ref val)).xMax - num2;
			if (Widgets.ButtonText(new Rect(num4, ((Rect)(ref val)).y + 5f, num2, 32f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_ClearAll")), true, true, true, (TextAnchor?)null))
			{
				ClearAllEntries();
			}
			float num5 = num4 - (num2 + num3);
			GUI.enabled = selectedEntries.Count > 0;
			if (Widgets.ButtonText(new Rect(num5, ((Rect)(ref val)).y + 5f, num2, 32f), TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_DeleteCount", NamedArgument.op_Implicit(selectedEntries.Count))), true, true, true, (TextAnchor?)null))
			{
				DeleteSelectedEntries();
			}
			GUI.enabled = true;
			float num6 = num5 - (num2 + num3);
			string text = TaggedString.op_Implicit((selectedEntries.Count > 0) ? TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_ExportCount", NamedArgument.op_Implicit(selectedEntries.Count)) : Translator.Translate("RimTalk_Knowledge_ExportAll"));
			if (Widgets.ButtonText(new Rect(num6, ((Rect)(ref val)).y + 5f, num2, 32f), text, true, true, true, (TextAnchor?)null))
			{
				ExportToFile();
			}
			float num7 = num6 - (num2 + num3);
			if (Widgets.ButtonText(new Rect(num7, ((Rect)(ref val)).y + 5f, num2, 32f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_Import")), true, true, true, (TextAnchor?)null))
			{
				ShowImportDialog();
			}
			float num8 = num7 - (num2 + num3);
			if (Widgets.ButtonText(new Rect(num8, ((Rect)(ref val)).y + 5f, num2, 32f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_New")), true, true, true, (TextAnchor?)null))
			{
				CreateNewEntry();
			}
			float num9 = num8 - (40f + num3);
			string text2 = (showRightPanel ? "◀" : "▶");
			if (Widgets.ButtonText(new Rect(num9, ((Rect)(ref val)).y + 5f, 40f, 32f), text2, true, true, true, (TextAnchor?)null))
			{
				showRightPanel = !showRightPanel;
			}
		}

		private void DrawSidebar(Rect rect)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_021d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0253: Unknown result type (might be due to invalid IL or missing references)
			//IL_0267: Unknown result type (might be due to invalid IL or missing references)
			//IL_027b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0280: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_035c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0368: Unknown result type (might be due to invalid IL or missing references)
			//IL_036d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0392: Unknown result type (might be due to invalid IL or missing references)
			//IL_039e: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
			Widgets.DrawMenuSection(rect);
			Rect val = GenUI.ContractedBy(rect, 10f);
			float y = ((Rect)(ref val)).y;
			Text.Font = (GameFont)2;
			Widgets.Label(new Rect(((Rect)(ref val)).x, y, ((Rect)(ref val)).width, 30f), Translator.Translate("RimTalk_Knowledge_Categories"));
			Text.Font = (GameFont)1;
			y += 35f;
			float num = 35f;
			foreach (KnowledgeCategory value in Enum.GetValues(typeof(KnowledgeCategory)))
			{
				bool isSelected = currentCategory == value;
				Rect rect2 = new Rect(((Rect)(ref val)).x, y, ((Rect)(ref val)).width, num);
				int categoryCount = GetCategoryCount(value);
				if (CommonKnowledgeUIHelpers.DrawCategoryButton(rect2, value, isSelected, categoryCount))
				{
					currentCategory = value;
					selectedEntries.Clear();
				}
				y += num + 5f;
			}
			y += 20f;
			Rect val2 = new Rect(((Rect)(ref val)).x, y, ((Rect)(ref val)).width, 30f);
			string arg = (showAutoGenerateSettings ? "▼" : "▶");
			if (Widgets.ButtonText(val2, string.Format("{0} {1}", arg, Translator.Translate("RimTalk_Knowledge_AutoGenerate")), true, true, true, (TextAnchor?)null))
			{
				showAutoGenerateSettings = !showAutoGenerateSettings;
			}
			y += 35f;
			if (showAutoGenerateSettings)
			{
				CommonKnowledgeUIHelpers.DrawAutoGenerateSettings(new Rect(((Rect)(ref val)).x, y, ((Rect)(ref val)).width, 120f), GeneratePawnStatusKnowledge, GenerateEventRecordKnowledge);
				y += 125f;
			}
			Rect val3 = new Rect(((Rect)(ref val)).x, y, ((Rect)(ref val)).width, 32f);
			GUI.color = new Color(0.4f, 0.8f, 0.4f);
			if (Widgets.ButtonText(val3, TaggedString.op_Implicit("\ud83d\udd0d " + Translator.Translate("RimTalk_Knowledge_TagTest")), true, true, true, (TextAnchor?)null))
			{
				ShowTagTestDialog();
			}
			GUI.color = Color.white;
			y += 42f;
			float num2 = ((Rect)(ref val)).yMax - 100f;
			Rect val4 = new Rect(((Rect)(ref val)).x, num2, ((Rect)(ref val)).width, 32f);
			GUI.color = new Color(0.4f, 0.7f, 1f);
			if (Widgets.ButtonText(val4, TaggedString.op_Implicit("\ud83d\udcd8 " + Translator.Translate("RimTalk_Knowledge_Help")), true, true, true, (TextAnchor?)null))
			{
				ShowHelpDialog();
			}
			GUI.color = Color.white;
			float num3 = ((Rect)(ref val)).yMax - 60f;
			Widgets.DrawLineHorizontal(((Rect)(ref val)).x, num3 - 10f, ((Rect)(ref val)).width);
			Text.Font = (GameFont)0;
			GUI.color = new Color(0.7f, 0.7f, 0.7f);
			int count = library.Entries.Count;
			int num4 = GenCollection.Count<CommonKnowledgeEntry>(library.Entries, (Predicate<CommonKnowledgeEntry>)((CommonKnowledgeEntry e) => e.isEnabled));
			int count2 = selectedEntries.Count;
			Widgets.Label(new Rect(((Rect)(ref val)).x, num3, ((Rect)(ref val)).width, 20f), TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_Total", NamedArgument.op_Implicit(count)));
			Widgets.Label(new Rect(((Rect)(ref val)).x, num3 + 20f, ((Rect)(ref val)).width, 20f), TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_Enabled", NamedArgument.op_Implicit(num4)));
			Widgets.Label(new Rect(((Rect)(ref val)).x, num3 + 40f, ((Rect)(ref val)).width, 20f), TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_Selected", NamedArgument.op_Implicit(count2)));
			GUI.color = Color.white;
			Text.Font = (GameFont)1;
		}

		private void DrawAutoGenerateSettings(Rect rect)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			Widgets.DrawBoxSolid(rect, new Color(0.1f, 0.1f, 0.1f, 0.3f));
			Rect val = GenUI.ContractedBy(rect, 5f);
			float y = ((Rect)(ref val)).y;
			RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
			bool enablePawnStatusKnowledge = settings.enablePawnStatusKnowledge;
			Widgets.CheckboxLabeled(new Rect(((Rect)(ref val)).x, y, ((Rect)(ref val)).width, 25f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_PawnStatus")), ref enablePawnStatusKnowledge, false, (Texture2D)null, (Texture2D)null, false, false);
			settings.enablePawnStatusKnowledge = enablePawnStatusKnowledge;
			y += 30f;
			if (Widgets.ButtonText(new Rect(((Rect)(ref val)).x, y, ((Rect)(ref val)).width, 25f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_GenerateNow")), true, true, true, (TextAnchor?)null))
			{
				GeneratePawnStatusKnowledge();
			}
			y += 30f;
			bool enableEventRecordKnowledge = settings.enableEventRecordKnowledge;
			Widgets.CheckboxLabeled(new Rect(((Rect)(ref val)).x, y, ((Rect)(ref val)).width, 25f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_EventRecord")), ref enableEventRecordKnowledge, false, (Texture2D)null, (Texture2D)null, false, false);
			settings.enableEventRecordKnowledge = enableEventRecordKnowledge;
			y += 30f;
			if (Widgets.ButtonText(new Rect(((Rect)(ref val)).x, y, ((Rect)(ref val)).width, 25f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_GenerateNow")), true, true, true, (TextAnchor?)null))
			{
				GenerateEventRecordKnowledge();
			}
		}

		private void DrawCenterList(Rect rect)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0200: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0224: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			Widgets.DrawMenuSection(rect);
			Rect val = GenUI.ContractedBy(rect, 5f);
			List<CommonKnowledgeEntry> filteredEntries = GetFilteredEntries();
			virtualList.SetItems(filteredEntries);
			virtualList.ScrollPosition = listScrollPosition;
			Rect val2 = default(Rect);
			((Rect)(ref val2))..ctor(0f, 0f, ((Rect)(ref val)).width - 16f, (float)filteredEntries.Count * 70f);
			HandleDragSelectionEvents(val, val2);
			Widgets.BeginScrollView(val, ref listScrollPosition, val2, true);
			float num = 0f;
			Rect rect2 = default(Rect);
			foreach (CommonKnowledgeEntry item in filteredEntries)
			{
				((Rect)(ref rect2))..ctor(0f, num, ((Rect)(ref val2)).width, 70f);
				DrawEntryRow(rect2, item);
				num += 70f;
			}
			if (isDragging)
			{
				DrawSelectionBox();
			}
			else if (isDragReordering && dragReorderInsertIndex >= 0)
			{
				float num2 = (float)dragReorderInsertIndex * 70f;
				Widgets.DrawBoxSolid(new Rect(0f, num2 - 2f, ((Rect)(ref val2)).width, 4f), new Color(0.3f, 0.8f, 1f, 0.8f));
				Widgets.DrawBoxSolid(new Rect(-2f, num2 - 6f, 12f, 12f), new Color(0.3f, 0.8f, 1f, 0.9f));
			}
			Widgets.EndScrollView();
			virtualList.ScrollPosition = listScrollPosition;
			if (!string.IsNullOrEmpty(searchFilter) || currentCategory != KnowledgeCategory.All)
			{
				Rect val3 = new Rect(((Rect)(ref val)).x, ((Rect)(ref val)).yMax - 25f, ((Rect)(ref val)).width, 20f);
				Text.Font = (GameFont)0;
				GUI.color = new Color(0.7f, 0.7f, 0.7f);
				Widgets.Label(val3, TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_Showing", NamedArgument.op_Implicit(filteredEntries.Count), NamedArgument.op_Implicit(library.Entries.Count)));
				GUI.color = Color.white;
				Text.Font = (GameFont)1;
			}
		}

		private void DrawEntryRow(Rect rect, CommonKnowledgeEntry entry)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_025a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0298: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0200: Unknown result type (might be due to invalid IL or missing references)
			//IL_0278: Unknown result type (might be due to invalid IL or missing references)
			//IL_026c: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0284: Unknown result type (might be due to invalid IL or missing references)
			//IL_0288: Unknown result type (might be due to invalid IL or missing references)
			bool num = selectedEntries.Contains(entry);
			if (num)
			{
				Widgets.DrawHighlight(rect);
			}
			else if (Mouse.IsOver(rect))
			{
				Widgets.DrawLightHighlight(rect);
			}
			Rect val = GenUI.ContractedBy(rect, 5f);
			float x = ((Rect)(ref val)).x;
			Rect val2 = default(Rect);
			((Rect)(ref val2))..ctor(x, ((Rect)(ref val)).y + 5f, 24f, 24f);
			_ = entry.isEnabled;
			Widgets.Checkbox(((Rect)(ref val2)).position, ref entry.isEnabled, 24f, false, false, (Texture2D)null, (Texture2D)null);
			x += 30f;
			Rect val3 = new Rect(x, ((Rect)(ref val)).y, 150f, 20f);
			Text.Font = (GameFont)1;
			GUI.color = CommonKnowledgeUIHelpers.GetCategoryColor(entry);
			Widgets.Label(val3, "[" + entry.tag + "]");
			GUI.color = Color.white;
			x += 155f;
			Widgets.Label(new Rect(x, ((Rect)(ref val)).y, 50f, 20f), entry.importance.ToString("F1"));
			x += 55f;
			Rect val4 = new Rect(((Rect)(ref val)).x + 30f, ((Rect)(ref val)).y + 22f, ((Rect)(ref val)).width - 95f, 40f);
			Text.Font = (GameFont)0;
			string text = ((entry.content.Length > 120) ? (entry.content.Substring(0, 120) + "...") : entry.content);
			Widgets.Label(val4, text);
			Text.Font = (GameFont)1;
			float num2 = ((Rect)(ref rect)).xMax - 40f - 5f - 10f;
			float num3 = ((Rect)(ref rect)).y + (((Rect)(ref rect)).height - 20f) / 2f;
			bool flag = ExtendedKnowledgeEntry.CanBeExtracted(entry);
			Rect val5 = default(Rect);
			((Rect)(ref val5))..ctor(num2, num3, 20f, 20f);
			if (Mouse.IsOver(val5))
			{
				string text2 = TaggedString.op_Implicit(flag ? Translator.Translate("RimTalk_CanBeExtractedEnabled") : Translator.Translate("RimTalk_CanBeExtractedDisabled"));
				TooltipHandler.TipRegion(val5, TipSignal.op_Implicit(text2));
			}
			bool flag2 = flag;
			Widgets.Checkbox(((Rect)(ref val5)).position, ref flag2, 20f, false, false, (Texture2D)null, (Texture2D)null);
			if (flag2 != flag)
			{
				ExtendedKnowledgeEntry.SetCanBeExtracted(entry, flag2);
			}
			num2 += 25f;
			bool flag3 = ExtendedKnowledgeEntry.CanBeMatched(entry);
			Rect val6 = default(Rect);
			((Rect)(ref val6))..ctor(num2, num3, 20f, 20f);
			if (Mouse.IsOver(val6))
			{
				string text3 = TaggedString.op_Implicit(flag3 ? Translator.Translate("RimTalk_CanBeMatchedEnabled") : Translator.Translate("RimTalk_CanBeMatchedDisabled"));
				TooltipHandler.TipRegion(val6, TipSignal.op_Implicit(text3));
			}
			bool flag4 = flag3;
			Widgets.Checkbox(((Rect)(ref val6)).position, ref flag4, 20f, false, false, (Texture2D)null, (Texture2D)null);
			if (flag4 != flag3)
			{
				ExtendedKnowledgeEntry.SetCanBeMatched(entry, flag4);
			}
			if (num)
			{
				Widgets.DrawBoxSolid(new Rect(((Rect)(ref rect)).x, ((Rect)(ref rect)).y, 3f, ((Rect)(ref rect)).height), new Color(0.3f, 0.6f, 1f));
			}
		}

		private void HandleEntryClick(CommonKnowledgeEntry entry)
		{
			bool control = Event.current.control;
			bool shift = Event.current.shift;
			if (control)
			{
				if (selectedEntries.Contains(entry))
				{
					selectedEntries.Remove(entry);
				}
				else
				{
					selectedEntries.Add(entry);
				}
				lastSelectedEntry = entry;
			}
			else if (shift && lastSelectedEntry != null)
			{
				List<CommonKnowledgeEntry> filteredEntries = GetFilteredEntries();
				int num = filteredEntries.IndexOf(lastSelectedEntry);
				int num2 = filteredEntries.IndexOf(entry);
				if (num >= 0 && num2 >= 0)
				{
					int num3 = Math.Min(num, num2);
					int num4 = Math.Max(num, num2);
					for (int i = num3; i <= num4; i++)
					{
						selectedEntries.Add(filteredEntries[i]);
					}
				}
				lastSelectedEntry = entry;
			}
			else
			{
				selectedEntries.Clear();
				selectedEntries.Add(entry);
				lastSelectedEntry = entry;
				editMode = false;
			}
		}

		private void ExecuteDragReorder()
		{
			List<CommonKnowledgeEntry> filteredEntries = GetFilteredEntries();
			if (filteredEntries.Count == 0 || selectedEntries.Count == 0)
			{
				return;
			}
			int num;
			if (dragReorderInsertIndex >= filteredEntries.Count)
			{
				CommonKnowledgeEntry item = filteredEntries[filteredEntries.Count - 1];
				num = library.Entries.IndexOf(item) + 1;
			}
			else if (dragReorderInsertIndex <= 0)
			{
				CommonKnowledgeEntry item2 = filteredEntries[0];
				num = library.Entries.IndexOf(item2);
			}
			else
			{
				CommonKnowledgeEntry item3 = filteredEntries[dragReorderInsertIndex];
				num = library.Entries.IndexOf(item3);
			}
			List<CommonKnowledgeEntry> list = new List<CommonKnowledgeEntry>();
			foreach (CommonKnowledgeEntry entry in library.Entries)
			{
				if (selectedEntries.Contains(entry))
				{
					list.Add(entry);
				}
			}
			if (list.Count == 0)
			{
				return;
			}
			foreach (CommonKnowledgeEntry item5 in list)
			{
				library.Entries.Remove(item5);
			}
			if (dragReorderInsertIndex < filteredEntries.Count)
			{
				CommonKnowledgeEntry item4 = filteredEntries[dragReorderInsertIndex];
				int num2 = library.Entries.IndexOf(item4);
				num = ((num2 < 0) ? library.Entries.Count : num2);
			}
			else
			{
				num = library.Entries.Count;
			}
			num = Mathf.Clamp(num, 0, library.Entries.Count);
			library.Entries.InsertRange(num, list);
		}

		private void DrawSelectionBox()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			Rect selectionBox = GetSelectionBox();
			Widgets.DrawBox(selectionBox, 1, (Texture2D)null);
			Widgets.DrawBoxSolid(selectionBox, new Color(0.3f, 0.6f, 1f, 0.2f));
		}

		private Rect GetSelectionBox()
		{
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			float num = Mathf.Min(dragStartPos.x, dragCurrentPos.x);
			float num2 = Mathf.Min(dragStartPos.y, dragCurrentPos.y);
			float num3 = Mathf.Max(dragStartPos.x, dragCurrentPos.x);
			float num4 = Mathf.Max(dragStartPos.y, dragCurrentPos.y);
			return new Rect(num, num2, num3 - num, num4 - num2);
		}

		private void HandleDragSelectionEvents(Rect listRect, Rect viewRect)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Invalid comparison between Unknown and I4
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_023d: Unknown result type (might be due to invalid IL or missing references)
			Event current = Event.current;
			if ((int)current.type == 0 && current.button == 0 && ((Rect)(ref listRect)).Contains(current.mousePosition))
			{
				isMouseDown = true;
				mouseDownScreenPos = current.mousePosition;
				dragStartPos = current.mousePosition - ((Rect)(ref listRect)).position;
				dragCurrentPos = dragStartPos;
				float num = (current.mousePosition - ((Rect)(ref listRect)).position).y + listScrollPosition.y;
				List<CommonKnowledgeEntry> filteredEntries = GetFilteredEntries();
				int num2 = (int)(num / 70f);
				mouseDownOnSelected = false;
				if (num2 >= 0 && num2 < filteredEntries.Count)
				{
					mouseDownOnSelected = selectedEntries.Contains(filteredEntries[num2]);
				}
			}
			if (!isMouseDown || (int)current.type != 3 || current.button != 0)
			{
				return;
			}
			float num3 = Vector2.Distance(mouseDownScreenPos, current.mousePosition);
			if (!isDragging && !isDragReordering && num3 >= 5f)
			{
				if (mouseDownOnSelected && selectedEntries.Count > 0)
				{
					isDragReordering = true;
				}
				else
				{
					isDragging = true;
				}
			}
			if (isDragReordering)
			{
				float num4 = (current.mousePosition - ((Rect)(ref listRect)).position).y + listScrollPosition.y;
				List<CommonKnowledgeEntry> filteredEntries2 = GetFilteredEntries();
				dragReorderInsertIndex = Mathf.Clamp(Mathf.RoundToInt(num4 / 70f), 0, filteredEntries2.Count);
				current.Use();
			}
			else
			{
				if (!isDragging)
				{
					return;
				}
				dragCurrentPos = current.mousePosition - ((Rect)(ref listRect)).position;
				Rect selectionBox = GetSelectionBox();
				Rect val = default(Rect);
				((Rect)(ref val))..ctor(((Rect)(ref selectionBox)).x, ((Rect)(ref selectionBox)).y + listScrollPosition.y, ((Rect)(ref selectionBox)).width, ((Rect)(ref selectionBox)).height);
				List<CommonKnowledgeEntry> filteredEntries3 = GetFilteredEntries();
				if (!Event.current.control)
				{
					selectedEntries.Clear();
				}
				float num5 = 0f;
				Rect val2 = default(Rect);
				foreach (CommonKnowledgeEntry item in filteredEntries3)
				{
					((Rect)(ref val2))..ctor(0f, num5, ((Rect)(ref viewRect)).width, 70f);
					if (((Rect)(ref val)).Overlaps(val2))
					{
						selectedEntries.Add(item);
					}
					num5 += 70f;
				}
				current.Use();
			}
		}

		private void DrawRightPanel(Rect rect)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			Widgets.DrawMenuSection(rect);
			Rect rect2 = GenUI.ContractedBy(rect, 10f);
			if (editMode)
			{
				DrawEditPanel(rect2);
			}
			else if (selectedEntries.Count == 0)
			{
				DrawEmptyPanel(rect2);
			}
			else if (selectedEntries.Count == 1)
			{
				DrawDetailPanel(rect2, selectedEntries.First());
			}
			else
			{
				DrawMultiSelectionPanel(rect2);
			}
		}

		private void DrawEmptyPanel(Rect rect)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			Text.Anchor = (TextAnchor)4;
			GUI.color = new Color(0.6f, 0.6f, 0.6f);
			Widgets.Label(rect, Translator.Translate("RimTalk_Knowledge_SelectOrCreate"));
			GUI.color = Color.white;
			Text.Anchor = (TextAnchor)0;
		}

		private void DrawMultiSelectionPanel(Rect rect)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_026b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0280: Unknown result type (might be due to invalid IL or missing references)
			//IL_0285: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02de: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_031c: Unknown result type (might be due to invalid IL or missing references)
			//IL_035e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0368: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0450: Unknown result type (might be due to invalid IL or missing references)
			//IL_045a: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
			float y = ((Rect)(ref rect)).y;
			Text.Font = (GameFont)2;
			Widgets.Label(new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width, 30f), TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_ItemsSelected", NamedArgument.op_Implicit(selectedEntries.Count)));
			Text.Font = (GameFont)1;
			y += 40f;
			int num = selectedEntries.Count((CommonKnowledgeEntry e) => e.isEnabled);
			int num2 = selectedEntries.Count - num;
			float num3 = selectedEntries.Average((CommonKnowledgeEntry e) => e.importance);
			Widgets.Label(new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width, 25f), TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_EnabledCount", NamedArgument.op_Implicit(num)));
			y += 25f;
			Widgets.Label(new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width, 25f), TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_DisabledCount", NamedArgument.op_Implicit(num2)));
			y += 25f;
			Widgets.Label(new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width, 25f), TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_AvgImportance", NamedArgument.op_Implicit(num3.ToString("F2"))));
			y += 40f;
			if (Widgets.ButtonText(new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width, 32f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_EnableAll")), true, true, true, (TextAnchor?)null))
			{
				foreach (CommonKnowledgeEntry selectedEntry in selectedEntries)
				{
					selectedEntry.isEnabled = true;
				}
			}
			y += 37f;
			if (Widgets.ButtonText(new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width, 32f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_DisableAll")), true, true, true, (TextAnchor?)null))
			{
				foreach (CommonKnowledgeEntry selectedEntry2 in selectedEntries)
				{
					selectedEntry2.isEnabled = false;
				}
			}
			y += 37f;
			if (Widgets.ButtonText(new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width, 32f), TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_ExportItems", NamedArgument.op_Implicit(selectedEntries.Count))), true, true, true, (TextAnchor?)null))
			{
				ExportToFile();
			}
			y += 37f;
			GUI.color = new Color(1f, 0.4f, 0.4f);
			if (Widgets.ButtonText(new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width, 32f), TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_DeleteItems", NamedArgument.op_Implicit(selectedEntries.Count))), true, true, true, (TextAnchor?)null))
			{
				DeleteSelectedEntries();
			}
			GUI.color = Color.white;
			y += 42f;
			Widgets.DrawLineHorizontal(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width);
			y += 10f;
			if (Widgets.ButtonText(new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width, 32f), TaggedString.op_Implicit(Translator.Translate("RimTalk_EnableAllExtract")), true, true, true, (TextAnchor?)null))
			{
				foreach (CommonKnowledgeEntry selectedEntry3 in selectedEntries)
				{
					ExtendedKnowledgeEntry.SetCanBeExtracted(selectedEntry3, value: true);
				}
			}
			y += 37f;
			if (Widgets.ButtonText(new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width, 32f), TaggedString.op_Implicit(Translator.Translate("RimTalk_DisableAllExtract")), true, true, true, (TextAnchor?)null))
			{
				foreach (CommonKnowledgeEntry selectedEntry4 in selectedEntries)
				{
					ExtendedKnowledgeEntry.SetCanBeExtracted(selectedEntry4, value: false);
				}
			}
			y += 37f;
			if (Widgets.ButtonText(new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width, 32f), TaggedString.op_Implicit(Translator.Translate("RimTalk_EnableAllMatch")), true, true, true, (TextAnchor?)null))
			{
				foreach (CommonKnowledgeEntry selectedEntry5 in selectedEntries)
				{
					ExtendedKnowledgeEntry.SetCanBeMatched(selectedEntry5, value: true);
				}
			}
			y += 37f;
			if (!Widgets.ButtonText(new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width, 32f), TaggedString.op_Implicit(Translator.Translate("RimTalk_DisableAllMatch")), true, true, true, (TextAnchor?)null))
			{
				return;
			}
			foreach (CommonKnowledgeEntry selectedEntry6 in selectedEntries)
			{
				ExtendedKnowledgeEntry.SetCanBeMatched(selectedEntry6, value: false);
			}
		}

		private void DrawDetailPanel(Rect rect, CommonKnowledgeEntry entry)
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0229: Unknown result type (might be due to invalid IL or missing references)
			//IL_0233: Unknown result type (might be due to invalid IL or missing references)
			//IL_025e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0268: Unknown result type (might be due to invalid IL or missing references)
			//IL_0272: Unknown result type (might be due to invalid IL or missing references)
			//IL_0296: Unknown result type (might be due to invalid IL or missing references)
			//IL_029b: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_0306: Unknown result type (might be due to invalid IL or missing references)
			//IL_0322: Unknown result type (might be due to invalid IL or missing references)
			//IL_032c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0336: Unknown result type (might be due to invalid IL or missing references)
			//IL_0363: Unknown result type (might be due to invalid IL or missing references)
			//IL_036d: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_040c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0430: Unknown result type (might be due to invalid IL or missing references)
			float y = ((Rect)(ref rect)).y;
			Text.Font = (GameFont)2;
			Widgets.Label(new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width - 100f, 30f), Translator.Translate("RimTalk_Knowledge_Details"));
			Text.Font = (GameFont)1;
			if (Widgets.ButtonText(new Rect(((Rect)(ref rect)).xMax - 90f, y, 90f, 28f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_Edit")), true, true, true, (TextAnchor?)null))
			{
				StartEdit();
			}
			y += 40f;
			Rect val = new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width, ((Rect)(ref rect)).height - y - 50f);
			float num = 500f;
			Rect val2 = default(Rect);
			((Rect)(ref val2))..ctor(0f, 0f, ((Rect)(ref rect)).width - 16f, num);
			Widgets.BeginScrollView(val, ref detailScrollPosition, val2, true);
			float num2 = 0f;
			CommonKnowledgeUIHelpers.DrawDetailField(new Rect(0f, num2, ((Rect)(ref val2)).width, 50f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_Tag")), entry.tag);
			num2 += 55f;
			string text = CommonKnowledgeUIHelpers.GetCategoryLabel(CommonKnowledgeUIHelpers.GetEntryCategory(entry));
			if (entry.category == KnowledgeEntryCategory.None)
			{
				text += " (自动)";
			}
			CommonKnowledgeUIHelpers.DrawDetailField(new Rect(0f, num2, ((Rect)(ref val2)).width, 25f), "分类", text);
			num2 += 30f;
			CommonKnowledgeUIHelpers.DrawDetailField(new Rect(0f, num2, ((Rect)(ref val2)).width, 25f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_Importance")), entry.importance.ToString("F1"));
			num2 += 30f;
			string value = TaggedString.op_Implicit(entry.isEnabled ? Translator.Translate("RimTalk_Knowledge_StatusEnabled") : Translator.Translate("RimTalk_Knowledge_StatusDisabled"));
			CommonKnowledgeUIHelpers.DrawDetailField(new Rect(0f, num2, ((Rect)(ref val2)).width, 25f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_Status")), value);
			num2 += 30f;
			string visibilityText = CommonKnowledgeUIHelpers.GetVisibilityText(entry);
			CommonKnowledgeUIHelpers.DrawDetailField(new Rect(0f, num2, ((Rect)(ref val2)).width, 50f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_Visibility")), visibilityText);
			num2 += 55f;
			Widgets.Label(new Rect(0f, num2, ((Rect)(ref val2)).width, 20f), Translator.Translate("RimTalk_Knowledge_Content") + ":");
			num2 += 22f;
			Rect val3 = new Rect(0f, num2, ((Rect)(ref val2)).width, 200f);
			Widgets.DrawBoxSolid(val3, new Color(0.1f, 0.1f, 0.1f, 0.3f));
			Widgets.Label(GenUI.ContractedBy(val3, 5f), entry.content);
			num2 += 205f;
			Widgets.DrawLineHorizontal(0f, num2, ((Rect)(ref val2)).width);
			num2 += 10f;
			Text.Font = (GameFont)1;
			GUI.color = new Color(0.8f, 0.8f, 0.8f);
			Widgets.Label(new Rect(0f, num2, ((Rect)(ref val2)).width, 25f), Translator.Translate("RimTalk_ExtendedProperties"));
			GUI.color = Color.white;
			num2 += 25f;
			bool value2 = ExtendedKnowledgeEntry.CanBeExtracted(entry);
			DrawPropertyRow(new Rect(0f, num2, ((Rect)(ref val2)).width, 25f), TaggedString.op_Implicit(Translator.Translate("RimTalk_CanBeExtracted")), value2);
			num2 += 25f;
			bool value3 = ExtendedKnowledgeEntry.CanBeMatched(entry);
			DrawPropertyRow(new Rect(0f, num2, ((Rect)(ref val2)).width, 25f), TaggedString.op_Implicit(Translator.Translate("RimTalk_CanBeMatched")), value3);
			num2 += 25f;
			Widgets.EndScrollView();
			Rect val4 = new Rect(((Rect)(ref rect)).x, ((Rect)(ref rect)).yMax - 40f, ((Rect)(ref rect)).width, 32f);
			GUI.color = new Color(1f, 0.4f, 0.4f);
			if (Widgets.ButtonText(val4, TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_Delete")), true, true, true, (TextAnchor?)null))
			{
				DeleteSelectedEntries();
			}
			GUI.color = Color.white;
		}

		private void DrawPropertyRow(Rect rect, string label, bool value)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			Text.Font = (GameFont)0;
			GUI.color = new Color(0.7f, 0.7f, 0.7f);
			Widgets.Label(new Rect(((Rect)(ref rect)).x, ((Rect)(ref rect)).y, 120f, ((Rect)(ref rect)).height), label);
			GUI.color = Color.white;
			Text.Font = (GameFont)1;
			string text = TaggedString.op_Implicit(value ? Translator.Translate("RimTalk_Yes") : Translator.Translate("RimTalk_No"));
			GUI.color = (value ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.8f, 0.3f, 0.3f));
			Widgets.Label(new Rect(((Rect)(ref rect)).x + 120f, ((Rect)(ref rect)).y, ((Rect)(ref rect)).width - 120f, ((Rect)(ref rect)).height), text);
			GUI.color = Color.white;
		}

		private void DrawEditPanel(Rect rect)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_0231: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0270: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0327: Unknown result type (might be due to invalid IL or missing references)
			//IL_032c: Unknown result type (might be due to invalid IL or missing references)
			//IL_035c: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e6: Expected O, but got Unknown
			//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_0506: Unknown result type (might be due to invalid IL or missing references)
			//IL_050b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0520: Unknown result type (might be due to invalid IL or missing references)
			//IL_052f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0534: Unknown result type (might be due to invalid IL or missing references)
			//IL_0537: Unknown result type (might be due to invalid IL or missing references)
			//IL_020d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0217: Expected O, but got Unknown
			//IL_058c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0596: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_046b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0475: Expected O, but got Unknown
			//IL_049c: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a6: Expected O, but got Unknown
			float y = ((Rect)(ref rect)).y;
			Text.Font = (GameFont)2;
			Widgets.Label(new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width, 30f), (lastSelectedEntry == null) ? Translator.Translate("RimTalk_Knowledge_NewEntry") : Translator.Translate("RimTalk_Knowledge_EditEntry"));
			Text.Font = (GameFont)1;
			y += 40f;
			Widgets.Label(new Rect(((Rect)(ref rect)).x, y, 100f, 25f), CommonKnowledgeUIHelpers.EnsureColon(TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_Tag"))));
			editTag = Widgets.TextField(new Rect(((Rect)(ref rect)).x + 100f, y, ((Rect)(ref rect)).width - 100f, 25f), editTag);
			y += 30f;
			Widgets.Label(new Rect(((Rect)(ref rect)).x, y, 100f, 25f), "分类:");
			string text = CommonKnowledgeUIHelpers.GetExplicitCategoryLabel(editCategory);
			if (editCategory == KnowledgeEntryCategory.None && !string.IsNullOrEmpty(editTag))
			{
				string categoryLabel = CommonKnowledgeUIHelpers.GetCategoryLabel(CommonKnowledgeUIHelpers.GetEntryCategory(new CommonKnowledgeEntry(editTag, "")
				{
					category = KnowledgeEntryCategory.None
				}));
				text = "自动推断 → " + categoryLabel;
			}
			if (Widgets.ButtonText(new Rect(((Rect)(ref rect)).x + 100f, y, ((Rect)(ref rect)).width - 100f, 25f), text, true, true, true, (TextAnchor?)null))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (KnowledgeEntryCategory value in Enum.GetValues(typeof(KnowledgeEntryCategory)))
				{
					KnowledgeEntryCategory localCat = value;
					list.Add(new FloatMenuOption(CommonKnowledgeUIHelpers.GetExplicitCategoryLabel(value), (Action)delegate
					{
						editCategory = localCat;
					}, (MenuOptionPriority)4, (Action<Rect>)null, (Thing)null, 0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0));
				}
				Find.WindowStack.Add((Window)new FloatMenu(list));
			}
			y += 30f;
			Widgets.Label(new Rect(((Rect)(ref rect)).x, y, 100f, 25f), CommonKnowledgeUIHelpers.EnsureColon(TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_Importance"))));
			editImportance = Widgets.HorizontalSlider(new Rect(((Rect)(ref rect)).x + 100f, y, ((Rect)(ref rect)).width - 150f, 25f), editImportance, 0f, 1f, false, (string)null, (string)null, (string)null, -1f);
			Widgets.Label(new Rect(((Rect)(ref rect)).xMax - 40f, y, 40f, 25f), editImportance.ToString("F1"));
			y += 30f;
			Widgets.Label(new Rect(((Rect)(ref rect)).x, y, 100f, 25f), CommonKnowledgeUIHelpers.EnsureColon(TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_Visibility"))));
			string text2 = ((editTargetPawnId == -1) ? ((object)Translator.Translate("RimTalk_Knowledge_Global")/*cast due to constrained. prefix*/).ToString() : $"Pawn #{editTargetPawnId}");
			if (Widgets.ButtonText(new Rect(((Rect)(ref rect)).x + 100f, y, ((Rect)(ref rect)).width - 100f, 25f), text2, true, true, true, (TextAnchor?)null))
			{
				CommonKnowledgeUIHelpers.ShowPawnSelectionMenu(delegate(int pawnId)
				{
					editTargetPawnId = pawnId;
				});
			}
			y += 35f;
			Widgets.Label(new Rect(((Rect)(ref rect)).x, y, 100f, 25f), "匹配模式:");
			if (Widgets.ButtonText(new Rect(((Rect)(ref rect)).x + 100f, y, ((Rect)(ref rect)).width - 100f, 25f), editMatchMode.ToString(), true, true, true, (TextAnchor?)null))
			{
				List<FloatMenuOption> list2 = new List<FloatMenuOption>();
				foreach (KeywordMatchMode mode in Enum.GetValues(typeof(KeywordMatchMode)))
				{
					list2.Add(new FloatMenuOption(mode.ToString(), (Action)delegate
					{
						editMatchMode = mode;
					}, (MenuOptionPriority)4, (Action<Rect>)null, (Thing)null, 0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0));
				}
				Find.WindowStack.Add((Window)new FloatMenu(list2));
			}
			y += 35f;
			Widgets.Label(new Rect(((Rect)(ref rect)).x, y, 100f, 25f), CommonKnowledgeUIHelpers.EnsureColon(TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_Content"))));
			y += 27f;
			float num = ((Rect)(ref rect)).yMax - y - 80f;
			Rect val = new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width, num);
			Widgets.DrawBoxSolid(val, new Color(0.1f, 0.1f, 0.1f, 0.8f));
			Rect val2 = GenUI.ContractedBy(val, 5f);
			editContent = GUI.TextArea(val2, editContent ?? "");
			y += num + 10f;
			float num2 = ((Rect)(ref rect)).yMax - 65f;
			if (Widgets.ButtonText(new Rect(((Rect)(ref rect)).x, num2, (((Rect)(ref rect)).width - 5f) / 2f, 32f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_Save")), true, true, true, (TextAnchor?)null))
			{
				SaveEntry();
			}
			if (Widgets.ButtonText(new Rect(((Rect)(ref rect)).x + (((Rect)(ref rect)).width + 5f) / 2f, num2, (((Rect)(ref rect)).width - 5f) / 2f, 32f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_Cancel")), true, true, true, (TextAnchor?)null))
			{
				editMode = false;
			}
		}

		private List<CommonKnowledgeEntry> GetFilteredEntries()
		{
			IEnumerable<CommonKnowledgeEntry> source = library.Entries.AsEnumerable();
			if (currentCategory != KnowledgeCategory.All)
			{
				source = source.Where((CommonKnowledgeEntry e) => CommonKnowledgeUIHelpers.GetEntryCategory(e) == currentCategory);
			}
			if (!string.IsNullOrEmpty(searchFilter))
			{
				string lower = searchFilter.ToLower();
				source = source.Where((CommonKnowledgeEntry e) => e.tag.ToLower().Contains(lower) || e.content.ToLower().Contains(lower));
			}
			return source.ToList();
		}

		private KnowledgeCategory GetEntryCategory(CommonKnowledgeEntry entry)
		{
			if (entry.tag.Contains("规则") || entry.tag.Contains("Instructions"))
			{
				return KnowledgeCategory.Instructions;
			}
			if (entry.tag.Contains("世界观") || entry.tag.Contains("Lore"))
			{
				return KnowledgeCategory.Lore;
			}
			if (entry.tag.Contains("殖民者状态") || entry.tag.Contains("PawnStatus"))
			{
				return KnowledgeCategory.PawnStatus;
			}
			if (entry.tag.Contains("历史") || entry.tag.Contains("History"))
			{
				return KnowledgeCategory.History;
			}
			return KnowledgeCategory.Other;
		}

		private string GetCategoryLabel(KnowledgeCategory category)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			return category switch
			{
				KnowledgeCategory.All => TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_CategoryAll")), 
				KnowledgeCategory.Instructions => TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_CategoryInstructions")), 
				KnowledgeCategory.Lore => TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_CategoryLore")), 
				KnowledgeCategory.PawnStatus => TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_CategoryPawnStatus")), 
				KnowledgeCategory.History => TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_CategoryHistory")), 
				KnowledgeCategory.Other => TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_CategoryOther")), 
				_ => TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_CategoryUnknown")), 
			};
		}

		private int GetCategoryCount(KnowledgeCategory category)
		{
			if (category == KnowledgeCategory.All)
			{
				return library.Entries.Count;
			}
			return GenCollection.Count<CommonKnowledgeEntry>(library.Entries, (Predicate<CommonKnowledgeEntry>)((CommonKnowledgeEntry e) => CommonKnowledgeUIHelpers.GetEntryCategory(e) == category));
		}

		private Color GetCategoryColor(CommonKnowledgeEntry entry)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			return (Color)(CommonKnowledgeUIHelpers.GetEntryCategory(entry) switch
			{
				KnowledgeCategory.Instructions => new Color(0.3f, 0.8f, 0.3f), 
				KnowledgeCategory.Lore => new Color(0.8f, 0.6f, 0.3f), 
				KnowledgeCategory.PawnStatus => new Color(0.3f, 0.6f, 0.9f), 
				KnowledgeCategory.History => new Color(0.7f, 0.5f, 0.7f), 
				_ => Color.white, 
			});
		}

		private void CreateNewEntry()
		{
			editTag = "";
			editContent = "";
			editImportance = 0.5f;
			editTargetPawnId = -1;
			editMatchMode = KeywordMatchMode.Any;
			editCategory = KnowledgeEntryCategory.None;
			lastSelectedEntry = null;
			selectedEntries.Clear();
			editMode = true;
		}

		private void StartEdit()
		{
			if (selectedEntries.Count == 1)
			{
				CommonKnowledgeEntry commonKnowledgeEntry = selectedEntries.First();
				editTag = commonKnowledgeEntry.tag;
				editContent = commonKnowledgeEntry.content;
				editImportance = commonKnowledgeEntry.importance;
				editTargetPawnId = commonKnowledgeEntry.targetPawnId;
				editMatchMode = commonKnowledgeEntry.matchMode;
				editCategory = commonKnowledgeEntry.category;
				lastSelectedEntry = commonKnowledgeEntry;
				editMode = true;
			}
		}

		private void SaveEntry()
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(editTag) || string.IsNullOrEmpty(editContent))
			{
				Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_TagContentEmpty")), MessageTypeDefOf.RejectInput, false);
				return;
			}
			if (lastSelectedEntry == null)
			{
				CommonKnowledgeEntry commonKnowledgeEntry = new CommonKnowledgeEntry(editTag, editContent)
				{
					importance = editImportance,
					targetPawnId = editTargetPawnId,
					isUserEdited = true,
					matchMode = editMatchMode,
					category = editCategory
				};
				library.AddEntry(commonKnowledgeEntry);
				selectedEntries.Clear();
				selectedEntries.Add(commonKnowledgeEntry);
				lastSelectedEntry = commonKnowledgeEntry;
			}
			else
			{
				lastSelectedEntry.tag = editTag;
				lastSelectedEntry.content = editContent;
				lastSelectedEntry.importance = editImportance;
				lastSelectedEntry.targetPawnId = editTargetPawnId;
				lastSelectedEntry.matchMode = editMatchMode;
				lastSelectedEntry.category = editCategory;
				lastSelectedEntry.InvalidateCache();
			}
			editMode = false;
			Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_EntrySaved")), MessageTypeDefOf.PositiveEvent, false);
		}

		private void DeleteSelectedEntries()
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			if (selectedEntries.Count == 0)
			{
				return;
			}
			int count = selectedEntries.Count;
			Find.WindowStack.Add((Window)(object)Dialog_MessageBox.CreateConfirmation(TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_DeleteConfirm", NamedArgument.op_Implicit(count)), (Action)delegate
			{
				//IL_0071: Unknown result type (might be due to invalid IL or missing references)
				//IL_0076: Unknown result type (might be due to invalid IL or missing references)
				foreach (CommonKnowledgeEntry item in selectedEntries.ToList())
				{
					library.RemoveEntry(item);
				}
				selectedEntries.Clear();
				lastSelectedEntry = null;
				Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_Deleted", NamedArgument.op_Implicit(count))), MessageTypeDefOf.PositiveEvent, false);
			}, false, (string)null, (WindowLayer)1));
		}

		private void ExportToFile()
		{
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			string systemCopyBuffer;
			if (selectedEntries.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (CommonKnowledgeEntry selectedEntry in selectedEntries)
				{
					stringBuilder.AppendLine(selectedEntry.FormatForExport());
				}
				systemCopyBuffer = stringBuilder.ToString();
			}
			else
			{
				systemCopyBuffer = library.ExportToText();
			}
			GUIUtility.systemCopyBuffer = systemCopyBuffer;
			int num = ((selectedEntries.Count > 0) ? selectedEntries.Count : library.Entries.Count);
			Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_ExportedToClipboard", NamedArgument.op_Implicit(num))), MessageTypeDefOf.PositiveEvent, false);
		}

		private void ShowImportDialog()
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			Find.WindowStack.Add((Window)(object)new Dialog_TextInput(TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_ImportTitle")), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_ImportDescription")), "", delegate(string text)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				int num = library.ImportFromText(text);
				Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_Imported", NamedArgument.op_Implicit(num))), MessageTypeDefOf.PositiveEvent, false);
			}, null, multiline: true));
		}

		private void ClearAllEntries()
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			Find.WindowStack.Add((Window)(object)Dialog_MessageBox.CreateConfirmation(Translator.Translate("RimTalk_Knowledge_ClearConfirm"), (Action)delegate
			{
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				library.Clear();
				selectedEntries.Clear();
				lastSelectedEntry = null;
				Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_AllCleared")), MessageTypeDefOf.PositiveEvent, false);
			}, false, (string)null, (WindowLayer)1));
		}

		private void GeneratePawnStatusKnowledge()
		{
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				Map currentMap = Find.CurrentMap;
				object obj;
				if (currentMap == null)
				{
					obj = null;
				}
				else
				{
					MapPawns mapPawns = currentMap.mapPawns;
					obj = ((mapPawns != null) ? mapPawns.FreeColonists : null);
				}
				List<Pawn> list = (List<Pawn>)obj;
				if (list == null || list.Count() == 0)
				{
					Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_NoColonists")), MessageTypeDefOf.RejectInput, false);
					return;
				}
				int ticksGame = Find.TickManager.TicksGame;
				int num = 0;
				foreach (Pawn item in list)
				{
					try
					{
						PawnStatusKnowledgeGenerator.UpdatePawnStatusKnowledge(item, library, ticksGame);
						num++;
					}
					catch (Exception ex)
					{
						Log.Warning($"[CommonKnowledge] Failed to generate for {item.Name}: {ex.Message}");
					}
				}
				Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_GeneratedPawnStatus", NamedArgument.op_Implicit(num))), MessageTypeDefOf.PositiveEvent, false);
			}
			catch (Exception ex2)
			{
				Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_GenerationFailed", NamedArgument.op_Implicit(ex2.Message))), MessageTypeDefOf.RejectInput, false);
			}
		}

		private void GenerateEventRecordKnowledge()
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				EventRecordKnowledgeGenerator.ScanRecentPlayLog();
				Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_EventScanTriggered")), MessageTypeDefOf.PositiveEvent, false);
			}
			catch (Exception ex)
			{
				Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Knowledge_GenerationFailed", NamedArgument.op_Implicit(ex.Message))), MessageTypeDefOf.RejectInput, false);
			}
		}

		private void ShowHelpDialog()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Expected O, but got Unknown
			string text = TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_HelpTitle"));
			string text2 = TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_HelpContent"));
			Find.WindowStack.Add((Window)new Dialog_MessageBox(TaggedString.op_Implicit(text2), (string)null, (Action)null, (string)null, (Action)null, text, false, (Action)null, (Action)null, (WindowLayer)1));
		}

		private void ShowTagTestDialog()
		{
			Find.WindowStack.Add((Window)(object)new Dialog_TagTest());
		}
	}
	public enum KnowledgeCategory
	{
		All,
		Instructions,
		Lore,
		PawnStatus,
		History,
		Other
	}
	public class Dialog_CreateMemory : Window
	{
		private readonly Pawn pawn;

		private readonly FourLayerMemoryComp memoryComp;

		private readonly MemoryLayer targetLayer;

		private readonly MemoryType memoryType;

		private string contentText;

		private string notesText;

		private string tagsText;

		private float importance;

		private bool isPinned;

		private Vector2 scrollPosition;

		public override Vector2 InitialSize
		{
			get
			{
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				return new Vector2(600f, 450f);
			}
		}

		public Dialog_CreateMemory(Pawn pawn, FourLayerMemoryComp memoryComp, MemoryLayer targetLayer, MemoryType memoryType)
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			contentText = "";
			notesText = "";
			tagsText = "";
			importance = 0.7f;
			scrollPosition = Vector2.zero;
			((Window)this)..ctor((IWindowDrawing)null);
			this.pawn = pawn;
			this.memoryComp = memoryComp;
			this.targetLayer = targetLayer;
			this.memoryType = memoryType;
			base.doCloseButton = false;
			base.doCloseX = true;
			base.forcePause = true;
			base.absorbInputAroundWindow = true;
			base.closeOnClickedOutside = false;
		}

		public override void DoWindowContents(Rect inRect)
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0212: Unknown result type (might be due to invalid IL or missing references)
			//IL_0251: Unknown result type (might be due to invalid IL or missing references)
			//IL_025b: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0338: Unknown result type (might be due to invalid IL or missing references)
			//IL_0342: Unknown result type (might be due to invalid IL or missing references)
			//IL_0303: Unknown result type (might be due to invalid IL or missing references)
			float num = 0f;
			Text.Font = (GameFont)2;
			string layerDisplayName = GetLayerDisplayName(targetLayer);
			string typeDisplayName = GetTypeDisplayName(memoryType);
			Widgets.Label(new Rect(0f, num, ((Rect)(ref inRect)).width, 35f), TranslatorFormattedStringExtensions.Translate("RimTalk_Memory_AddToLayer", NamedArgument.op_Implicit(typeDisplayName), NamedArgument.op_Implicit(layerDisplayName), NamedArgument.op_Implicit(((Entity)pawn).LabelShort)));
			Text.Font = (GameFont)1;
			num += 40f;
			Widgets.DrawLineHorizontal(0f, num, ((Rect)(ref inRect)).width);
			num += 10f;
			Widgets.Label(new Rect(0f, num, ((Rect)(ref inRect)).width, 24f), Translator.Translate("RimTalk_Memory_Content"));
			num += 26f;
			Rect val = default(Rect);
			((Rect)(ref val))..ctor(0f, num, ((Rect)(ref inRect)).width, 100f);
			contentText = GUI.TextArea(val, contentText);
			num += 105f;
			Widgets.Label(new Rect(0f, num, ((Rect)(ref inRect)).width, 24f), Translator.Translate("RimTalk_Memory_TagsOptional"));
			num += 26f;
			Rect val2 = default(Rect);
			((Rect)(ref val2))..ctor(0f, num, ((Rect)(ref inRect)).width, 30f);
			tagsText = Widgets.TextField(val2, tagsText);
			num += 35f;
			Widgets.Label(new Rect(0f, num, ((Rect)(ref inRect)).width, 24f), Translator.Translate("RimTalk_Memory_NotesOptional"));
			num += 26f;
			Rect val3 = default(Rect);
			((Rect)(ref val3))..ctor(0f, num, ((Rect)(ref inRect)).width, 30f);
			notesText = Widgets.TextField(val3, notesText);
			num += 35f;
			Widgets.Label(new Rect(0f, num, ((Rect)(ref inRect)).width, 24f), TranslatorFormattedStringExtensions.Translate("RimTalk_Memory_ImportanceLabel", NamedArgument.op_Implicit(importance.ToString("F2"))));
			num += 26f;
			Rect val4 = default(Rect);
			((Rect)(ref val4))..ctor(0f, num, ((Rect)(ref inRect)).width, 25f);
			importance = Widgets.HorizontalSlider(val4, importance, 0.1f, 1f, true, (string)null, (string)null, (string)null, -1f);
			num += 30f;
			Widgets.CheckboxLabeled(new Rect(0f, num, ((Rect)(ref inRect)).width, 30f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Memory_PinMemory")), ref isPinned, false, (Texture2D)null, (Texture2D)null, false, false);
			num += 35f;
			Widgets.DrawLineHorizontal(0f, num, ((Rect)(ref inRect)).width);
			num += 10f;
			float num2 = (((Rect)(ref inRect)).width - 10f) / 2f;
			float num3 = ((Rect)(ref inRect)).height - 40f;
			if (Widgets.ButtonText(new Rect(0f, num3, num2, 35f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_Save")), true, true, true, (TextAnchor?)null))
			{
				if (string.IsNullOrWhiteSpace(contentText))
				{
					Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_Memory_ContentRequired")), MessageTypeDefOf.RejectInput, true);
				}
				else
				{
					SaveMemory();
					((Window)this).Close(true);
				}
			}
			if (Widgets.ButtonText(new Rect(num2 + 10f, num3, num2, 35f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_Cancel")), true, true, true, (TextAnchor?)null))
			{
				((Window)this).Close(true);
			}
		}

		private void SaveMemory()
		{
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			if (memoryComp == null)
			{
				return;
			}
			MemoryEntry memoryEntry = new MemoryEntry(contentText.Trim(), memoryType, targetLayer, importance);
			if (!string.IsNullOrWhiteSpace(tagsText))
			{
				string[] array = tagsText.Split(',', StringSplitOptions.None);
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i].Trim();
					if (!string.IsNullOrEmpty(text))
					{
						memoryEntry.AddTag(text);
					}
				}
			}
			if (!string.IsNullOrWhiteSpace(notesText))
			{
				memoryEntry.Notes = notesText.Trim();
			}
			memoryEntry.IsPinned = isPinned;
			memoryEntry.AddTag("手动添加");
			switch (targetLayer)
			{
			case MemoryLayer.Active:
				memoryComp.ActiveMemories.Insert(0, memoryEntry);
				Messages.Message("已添加到 " + ((Entity)pawn).LabelShort + " 的ABM", MessageTypeDefOf.TaskCompletion, true);
				break;
			case MemoryLayer.Situational:
				memoryComp.SituationalMemories.Insert(0, memoryEntry);
				Messages.Message("已添加到 " + ((Entity)pawn).LabelShort + " 的SCM", MessageTypeDefOf.TaskCompletion, true);
				break;
			case MemoryLayer.EventLog:
				memoryComp.EventLogMemories.Insert(0, memoryEntry);
				Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Memory_AddedToELS", NamedArgument.op_Implicit(((Entity)pawn).LabelShort))), MessageTypeDefOf.TaskCompletion, true);
				break;
			case MemoryLayer.Archive:
				memoryComp.ArchiveMemories.Insert(0, memoryEntry);
				Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Memory_AddedToCLPA", NamedArgument.op_Implicit(((Entity)pawn).LabelShort))), MessageTypeDefOf.TaskCompletion, true);
				break;
			default:
				Log.Warning($"[RimTalk Memory] 不支持手动添加到 {targetLayer} 层级");
				break;
			}
		}

		private string GetLayerDisplayName(MemoryLayer layer)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			return layer switch
			{
				MemoryLayer.Active => "ABM", 
				MemoryLayer.Situational => "SCM", 
				MemoryLayer.EventLog => TaggedString.op_Implicit(Translator.Translate("RimTalk_Layer_ELS")), 
				MemoryLayer.Archive => TaggedString.op_Implicit(Translator.Translate("RimTalk_Layer_CLPA")), 
				_ => layer.ToString(), 
			};
		}

		private string GetTypeDisplayName(MemoryType type)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return type switch
			{
				MemoryType.Action => TaggedString.op_Implicit(Translator.Translate("RimTalk_Type_Action")), 
				MemoryType.Conversation => TaggedString.op_Implicit(Translator.Translate("RimTalk_Type_Conversation")), 
				_ => type.ToString(), 
			};
		}
	}
	public class Dialog_EditMemory : Window
	{
		private MemoryEntry memory;

		private FourLayerMemoryComp memoryComp;

		private string editedContent;

		private string editedNotes;

		private Vector2 scrollPosition;

		private Vector2 contentScrollPosition;

		private List<string> availableTags;

		private string newTagInput = "";

		public override Vector2 InitialSize
		{
			get
			{
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				return new Vector2(600f, 650f);
			}
		}

		public Dialog_EditMemory(MemoryEntry memory, FourLayerMemoryComp comp)
			: base((IWindowDrawing)null)
		{
			this.memory = memory;
			memoryComp = comp;
			editedContent = memory.Content;
			editedNotes = memory.Notes ?? "";
			availableTags = new List<string>
			{
				"开心", "悲伤", "愤怒", "焦虑", "平静", "战斗", "袭击", "受伤", "死亡", "完成任务",
				"闲聊", "深谈", "争吵", "友好", "敌对", "烹饪", "建造", "种植", "采矿", "研究",
				"医疗", "重要", "紧急"
			};
			base.doCloseButton = false;
			base.doCloseX = true;
			base.forcePause = true;
			base.absorbInputAroundWindow = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0230: Unknown result type (might be due to invalid IL or missing references)
			//IL_025d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0287: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_0340: Unknown result type (might be due to invalid IL or missing references)
			//IL_037e: Unknown result type (might be due to invalid IL or missing references)
			Text.Font = (GameFont)2;
			Widgets.Label(new Rect(0f, 0f, ((Rect)(ref inRect)).width, 35f), "编辑记忆");
			Text.Font = (GameFont)1;
			float num = 45f;
			Rect val = default(Rect);
			((Rect)(ref val))..ctor(0f, num, ((Rect)(ref inRect)).width, 60f);
			GUI.color = Color.gray;
			Widgets.Label(new Rect(((Rect)(ref val)).x, ((Rect)(ref val)).y, ((Rect)(ref val)).width, 25f), "类型: " + memory.TypeName + "  |  层级: " + memory.LayerName + "  |  时间: " + memory.AgeString);
			Widgets.Label(new Rect(((Rect)(ref val)).x, ((Rect)(ref val)).y + 25f, ((Rect)(ref val)).width, 25f), $"重要性: {memory.Importance:F2}  |  活跃度: {memory.Activity:F2}");
			GUI.color = Color.white;
			num += 65f;
			Widgets.Label(new Rect(0f, num, ((Rect)(ref inRect)).width, 25f), "内容:");
			num += 25f;
			Rect val2 = default(Rect);
			((Rect)(ref val2))..ctor(0f, num, ((Rect)(ref inRect)).width, 150f);
			float num2 = Mathf.Max(((Rect)(ref val2)).height, Text.CalcHeight(editedContent, ((Rect)(ref val2)).width - 20f) + 10f);
			Rect val3 = default(Rect);
			((Rect)(ref val3))..ctor(0f, 0f, ((Rect)(ref val2)).width - 20f, num2);
			Widgets.BeginScrollView(val2, ref contentScrollPosition, val3, true);
			editedContent = Widgets.TextArea(val3, editedContent, false);
			Widgets.EndScrollView();
			num += 155f;
			Widgets.Label(new Rect(0f, num, ((Rect)(ref inRect)).width, 25f), "备注:");
			num += 25f;
			Rect val4 = default(Rect);
			((Rect)(ref val4))..ctor(0f, num, ((Rect)(ref inRect)).width, 60f);
			editedNotes = Widgets.TextArea(val4, editedNotes, false);
			num += 65f;
			Widgets.Label(new Rect(0f, num, ((Rect)(ref inRect)).width, 25f), "标签:");
			num += 25f;
			DrawTagsSection(new Rect(0f, num, ((Rect)(ref inRect)).width, 100f));
			num += 105f;
			Rect val5 = new Rect(0f, num, ((Rect)(ref inRect)).width, 30f);
			bool isPinned = memory.IsPinned;
			Widgets.CheckboxLabeled(val5, "固定此记忆（不会被删除或衰减）", ref memory.IsPinned, false, (Texture2D)null, (Texture2D)null, false, false);
			if (memory.IsPinned != isPinned && memory.IsPinned)
			{
				memory.AddTag("重要");
			}
			num += 35f;
			float num3 = 120f;
			float num4 = ((Rect)(ref inRect)).height - 40f;
			if (Widgets.ButtonText(new Rect(((Rect)(ref inRect)).width - num3 * 2f - 10f, num4, num3, 35f), "保存", true, true, true, (TextAnchor?)null))
			{
				SaveChanges();
				((Window)this).Close(true);
			}
			if (Widgets.ButtonText(new Rect(((Rect)(ref inRect)).width - num3, num4, num3, 35f), "取消", true, true, true, (TextAnchor?)null))
			{
				((Window)this).Close(true);
			}
		}

		private void DrawTagsSection(Rect rect)
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0236: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0319: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			Rect val = default(Rect);
			((Rect)(ref val))..ctor(0f, 0f, ((Rect)(ref rect)).width - 20f, (float)availableTags.Count * 25f + 40f);
			Widgets.BeginScrollView(rect, ref scrollPosition, val, true);
			float num = 0f;
			if (memory.tags != null && GenCollection.Any<string>(memory.tags))
			{
				Rect val2 = default(Rect);
				foreach (string item in memory.tags.ToList())
				{
					((Rect)(ref val2))..ctor(0f, num, ((Rect)(ref val)).width, 22f);
					Widgets.Label(new Rect(((Rect)(ref val2)).x + 5f, ((Rect)(ref val2)).y, ((Rect)(ref val2)).width - 70f, ((Rect)(ref val2)).height), "✓ " + item);
					if (Widgets.ButtonText(new Rect(((Rect)(ref val2)).xMax - 60f, ((Rect)(ref val2)).y, 55f, 22f), "移除", true, true, true, (TextAnchor?)null))
					{
						memory.RemoveTag(item);
					}
					num += 25f;
				}
				num += 5f;
				Widgets.DrawLineHorizontal(0f, num, ((Rect)(ref val)).width);
				num += 10f;
			}
			Rect val3 = default(Rect);
			foreach (string availableTag in availableTags)
			{
				if (memory.tags == null || !memory.tags.Contains(availableTag))
				{
					((Rect)(ref val3))..ctor(0f, num, ((Rect)(ref val)).width, 22f);
					Rect val4 = new Rect(((Rect)(ref val3)).x + 5f, ((Rect)(ref val3)).y, ((Rect)(ref val3)).width - 70f, ((Rect)(ref val3)).height);
					GUI.color = Color.gray;
					Widgets.Label(val4, availableTag);
					GUI.color = Color.white;
					if (Widgets.ButtonText(new Rect(((Rect)(ref val3)).xMax - 60f, ((Rect)(ref val3)).y, 55f, 22f), "添加", true, true, true, (TextAnchor?)null))
					{
						memory.AddTag(availableTag);
					}
					num += 25f;
				}
			}
			num += 5f;
			Widgets.DrawLineHorizontal(0f, num, ((Rect)(ref val)).width);
			num += 10f;
			Widgets.Label(new Rect(0f, num, ((Rect)(ref val)).width, 22f), "自定义标签:");
			num += 25f;
			Rect val5 = default(Rect);
			((Rect)(ref val5))..ctor(0f, num, ((Rect)(ref val)).width - 70f, 22f);
			newTagInput = Widgets.TextField(val5, newTagInput);
			if (Widgets.ButtonText(new Rect(((Rect)(ref val)).width - 60f, num, 55f, 22f), "添加", true, true, true, (TextAnchor?)null) && !string.IsNullOrWhiteSpace(newTagInput))
			{
				memory.AddTag(newTagInput.Trim());
				newTagInput = "";
			}
			Widgets.EndScrollView();
		}

		private void SaveChanges()
		{
			EditMemory(memory, editedContent, editedNotes);
			if (!memory.tags.Contains("用户编辑"))
			{
				memory.AddTag("用户编辑");
			}
			Messages.Message("记忆已更新", MessageTypeDefOf.TaskCompletion, true);
		}

		private void EditMemory(MemoryEntry memory, string newContent, string notes = null)
		{
			if (memory != null)
			{
				memory.Content = newContent;
				memory.IsUserEdited = true;
				if (notes != null)
				{
					memory.Notes = notes;
				}
			}
		}
	}
	public class Dialog_PromptEditor : Window
	{
		private RimTalkMemoryPatchSettings settings;

		private string editDailySummary;

		private string editDeepArchive;

		private int editMaxTokens;

		private const string DEFAULT_DAILY_SUMMARY = "殖民者{0}的记忆总结\n\n记忆列表\n{1}\n\n要求提炼地点人物事件\n相似事件合并标注频率\n极简表达不超过80字\n只输出总结文字不要其他格式";

		private const string DEFAULT_DEEP_ARCHIVE = "殖民者{0}的记忆归档\n\n记忆列表\n{1}\n\n要求提炼核心特征和里程碑事件\n合并相似经历突出长期趋势\n极简表达不超过60字\n只输出总结文字不要其他格式";

		private Vector2 scrollPosition;

		public override Vector2 InitialSize
		{
			get
			{
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				return new Vector2(800f, 650f);
			}
		}

		public Dialog_PromptEditor()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			scrollPosition = Vector2.zero;
			((Window)this)..ctor((IWindowDrawing)null);
			settings = RimTalkMemoryPatchMod.Settings;
			base.doCloseX = true;
			base.doCloseButton = false;
			base.absorbInputAroundWindow = true;
			base.closeOnClickedOutside = false;
			editDailySummary = (string.IsNullOrEmpty(settings.SummarizePrompt) ? "殖民者{0}的记忆总结\n\n记忆列表\n{1}\n\n要求提炼地点人物事件\n相似事件合并标注频率\n极简表达不超过80字\n只输出总结文字不要其他格式" : settings.SummarizePrompt);
			editDeepArchive = (string.IsNullOrEmpty(settings.ArchivePrompt) ? "殖民者{0}的记忆归档\n\n记忆列表\n{1}\n\n要求提炼核心特征和里程碑事件\n合并相似经历突出长期趋势\n极简表达不超过60字\n只输出总结文字不要其他格式" : settings.ArchivePrompt);
			editMaxTokens = settings.SummaryMaxTokens;
		}

		public override void DoWindowContents(Rect inRect)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			Text.Font = (GameFont)2;
			Widgets.Label(new Rect(0f, 0f, ((Rect)(ref inRect)).width, 35f), Translator.Translate("RimTalk_PromptEditor_Title"));
			Text.Font = (GameFont)1;
			GUI.color = Color.gray;
			Widgets.Label(new Rect(0f, 35f, ((Rect)(ref inRect)).width, 20f), Translator.Translate("RimTalk_PromptEditor_Desc"));
			GUI.color = Color.white;
			float num = 60f;
			float num2 = ((Rect)(ref inRect)).height - num - 50f;
			Rect rect = default(Rect);
			((Rect)(ref rect))..ctor(0f, num, ((Rect)(ref inRect)).width, num2);
			DrawContent(rect);
			float num3 = ((Rect)(ref inRect)).height - 40f;
			float num4 = 120f;
			float num5 = 10f;
			if (Widgets.ButtonText(new Rect(0f, num3, num4, 35f), TaggedString.op_Implicit(Translator.Translate("RimTalk_PromptEditor_ResetDefault")), true, true, true, (TextAnchor?)null))
			{
				Find.WindowStack.Add((Window)(object)Dialog_MessageBox.CreateConfirmation(Translator.Translate("RimTalk_PromptEditor_ResetConfirm"), (Action)delegate
				{
					editDailySummary = "殖民者{0}的记忆总结\n\n记忆列表\n{1}\n\n要求提炼地点人物事件\n相似事件合并标注频率\n极简表达不超过80字\n只输出总结文字不要其他格式";
					editDeepArchive = "殖民者{0}的记忆归档\n\n记忆列表\n{1}\n\n要求提炼核心特征和里程碑事件\n合并相似经历突出长期趋势\n极简表达不超过60字\n只输出总结文字不要其他格式";
					editMaxTokens = 200;
				}, false, (string)null, (WindowLayer)1));
			}
			float num6 = ((Rect)(ref inRect)).width - num4;
			if (Widgets.ButtonText(new Rect(num6, num3, num4, 35f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Save")), true, true, true, (TextAnchor?)null))
			{
				SaveAndClose();
			}
			num6 -= num4 + num5;
			if (Widgets.ButtonText(new Rect(num6, num3, num4, 35f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Cancel")), true, true, true, (TextAnchor?)null))
			{
				((Window)this).Close(true);
			}
		}

		private void DrawContent(Rect rect)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected O, but got Unknown
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Unknown result type (might be due to invalid IL or missing references)
			//IL_020c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0216: Unknown result type (might be due to invalid IL or missing references)
			//IL_0226: Unknown result type (might be due to invalid IL or missing references)
			//IL_0231: Unknown result type (might be due to invalid IL or missing references)
			//IL_0237: Unknown result type (might be due to invalid IL or missing references)
			//IL_0258: Unknown result type (might be due to invalid IL or missing references)
			//IL_025d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0268: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0305: Unknown result type (might be due to invalid IL or missing references)
			//IL_0310: Unknown result type (might be due to invalid IL or missing references)
			//IL_0316: Unknown result type (might be due to invalid IL or missing references)
			Listing_Standard val = new Listing_Standard();
			Rect val2 = default(Rect);
			((Rect)(ref val2))..ctor(0f, 0f, ((Rect)(ref rect)).width - 20f, 900f);
			Widgets.BeginScrollView(rect, ref scrollPosition, val2, true);
			((Listing)val).Begin(val2);
			Text.Font = (GameFont)1;
			GUI.color = new Color(0.8f, 0.9f, 1f);
			val.Label(Translator.Translate("RimTalk_PromptEditor_DailySummary"), -1f, (string)null);
			GUI.color = Color.white;
			GUI.color = Color.gray;
			val.Label(Translator.Translate("RimTalk_PromptEditor_DailySummaryDesc"), -1f, (string)null);
			val.Label(Translator.Translate("RimTalk_PromptEditor_Placeholders"), -1f, (string)null);
			GUI.color = Color.white;
			((Listing)val).Gap(4f);
			Rect rect2 = ((Listing)val).GetRect(180f, 1f);
			editDailySummary = Widgets.TextArea(rect2, editDailySummary, false);
			((Listing)val).Gap(15f);
			((Listing)val).GapLine(12f);
			((Listing)val).Gap(10f);
			GUI.color = new Color(0.8f, 0.9f, 1f);
			val.Label(Translator.Translate("RimTalk_PromptEditor_DeepArchive"), -1f, (string)null);
			GUI.color = Color.white;
			GUI.color = Color.gray;
			val.Label(Translator.Translate("RimTalk_PromptEditor_DeepArchiveDesc"), -1f, (string)null);
			val.Label(Translator.Translate("RimTalk_PromptEditor_Placeholders"), -1f, (string)null);
			GUI.color = Color.white;
			((Listing)val).Gap(4f);
			Rect rect3 = ((Listing)val).GetRect(180f, 1f);
			editDeepArchive = Widgets.TextArea(rect3, editDeepArchive, false);
			((Listing)val).Gap(15f);
			((Listing)val).GapLine(12f);
			((Listing)val).Gap(10f);
			GUI.color = new Color(0.8f, 0.9f, 1f);
			val.Label(Translator.Translate("RimTalk_PromptEditor_MaxTokens"), -1f, (string)null);
			GUI.color = Color.white;
			GUI.color = Color.gray;
			val.Label(Translator.Translate("RimTalk_PromptEditor_MaxTokensDesc"), -1f, (string)null);
			GUI.color = Color.white;
			((Listing)val).Gap(4f);
			val.Label(TranslatorFormattedStringExtensions.Translate("RimTalk_PromptEditor_MaxTokensLabel", NamedArgument.op_Implicit(editMaxTokens)), -1f, (string)null);
			editMaxTokens = (int)val.Slider((float)editMaxTokens, 100f, 8000f);
			((Listing)val).Gap(10f);
			GUI.color = new Color(1f, 0.9f, 0.6f);
			val.Label(Translator.Translate("RimTalk_PromptEditor_Tips"), -1f, (string)null);
			GUI.color = Color.gray;
			val.Label(Translator.Translate("RimTalk_PromptEditor_Tip1"), -1f, (string)null);
			val.Label(Translator.Translate("RimTalk_PromptEditor_Tip2"), -1f, (string)null);
			val.Label(Translator.Translate("RimTalk_PromptEditor_Tip3"), -1f, (string)null);
			GUI.color = Color.white;
			((Listing)val).End();
			Widgets.EndScrollView();
		}

		private void SaveAndClose()
		{
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			settings.SummarizePrompt = ((editDailySummary == "殖民者{0}的记忆总结\n\n记忆列表\n{1}\n\n要求提炼地点人物事件\n相似事件合并标注频率\n极简表达不超过80字\n只输出总结文字不要其他格式") ? "" : editDailySummary);
			settings.ArchivePrompt = ((editDeepArchive == "殖民者{0}的记忆归档\n\n记忆列表\n{1}\n\n要求提炼核心特征和里程碑事件\n合并相似经历突出长期趋势\n极简表达不超过60字\n只输出总结文字不要其他格式") ? "" : editDeepArchive);
			settings.SummaryMaxTokens = editMaxTokens;
			((ModSettings)settings).Write();
			Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_PromptEditor_Saved")), MessageTypeDefOf.PositiveEvent, false);
			((Window)this).Close(true);
		}
	}
	public class Dialog_TagTest : Window
	{
		private string testTag;

		private string testContext;

		private Pawn testPawn;

		private bool testResult;

		private string testMatchText;

		private bool testExecuted;

		private Vector2 scrollPosition;

		public override Vector2 InitialSize
		{
			get
			{
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				return new Vector2(650f, 600f);
			}
		}

		public Dialog_TagTest()
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			testTag = "";
			testContext = "";
			testMatchText = "";
			scrollPosition = Vector2.zero;
			((Window)this)..ctor((IWindowDrawing)null);
			base.doCloseX = true;
			base.doCloseButton = false;
			base.closeOnClickedOutside = false;
			base.absorbInputAroundWindow = true;
			base.forcePause = false;
		}

		public override void DoWindowContents(Rect inRect)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0285: Unknown result type (might be due to invalid IL or missing references)
			//IL_027c: Unknown result type (might be due to invalid IL or missing references)
			float num = 0f;
			Text.Font = (GameFont)2;
			Widgets.Label(new Rect(0f, num, ((Rect)(ref inRect)).width, 35f), Translator.Translate("RimTalk_Knowledge_TagTestTitle"));
			Text.Font = (GameFont)1;
			num += 40f;
			Widgets.Label(new Rect(0f, num, ((Rect)(ref inRect)).width, 22f), Translator.Translate("RimTalk_Knowledge_TagTestInputTag"));
			num += 22f;
			testTag = Widgets.TextField(new Rect(0f, num, ((Rect)(ref inRect)).width, 28f), testTag);
			num += 32f;
			Widgets.Label(new Rect(0f, num, ((Rect)(ref inRect)).width, 22f), Translator.Translate("RimTalk_Knowledge_TagTestInputContext"));
			num += 22f;
			testContext = Widgets.TextArea(new Rect(0f, num, ((Rect)(ref inRect)).width, 50f), testContext, false);
			num += 54f;
			Widgets.Label(new Rect(0f, num, ((Rect)(ref inRect)).width, 22f), Translator.Translate("RimTalk_Knowledge_TagTestSelectPawn"));
			num += 22f;
			string text = ((testPawn == null) ? ((object)Translator.Translate("RimTalk_Knowledge_TagTestNoPawn")/*cast due to constrained. prefix*/).ToString() : ((Entity)testPawn).LabelShort);
			if (Widgets.ButtonText(new Rect(0f, num, ((Rect)(ref inRect)).width, 28f), text, true, true, true, (TextAnchor?)null))
			{
				ShowPawnSelectionMenu();
			}
			num += 32f;
			float num2 = (((Rect)(ref inRect)).width - 10f) / 2f;
			if (Widgets.ButtonText(new Rect(0f, num, num2, 32f), "?? 测试", true, true, true, (TextAnchor?)null))
			{
				ExecuteTagTest();
			}
			if (Widgets.ButtonText(new Rect(num2 + 10f, num, num2, 32f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_TagTestClear")), true, true, true, (TextAnchor?)null))
			{
				testTag = "";
				testContext = "";
				testPawn = null;
				testExecuted = false;
			}
			num += 40f;
			Widgets.DrawLineHorizontal(0f, num, ((Rect)(ref inRect)).width);
			num += 15f;
			float num3 = ((Rect)(ref inRect)).height - num;
			Rect rect = default(Rect);
			((Rect)(ref rect))..ctor(0f, num, ((Rect)(ref inRect)).width, num3);
			if (testExecuted)
			{
				DrawResultArea(rect);
			}
			else
			{
				DrawEmptyResultArea(rect);
			}
		}

		private void DrawEmptyResultArea(Rect rect)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			Text.Anchor = (TextAnchor)4;
			GUI.color = new Color(0.6f, 0.6f, 0.6f);
			Widgets.Label(rect, "点击 ?? 测试 按钮开始测试");
			GUI.color = Color.white;
			Text.Anchor = (TextAnchor)0;
		}

		private void DrawResultArea(Rect rect)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01db: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_020a: Unknown result type (might be due to invalid IL or missing references)
			float y = ((Rect)(ref rect)).y;
			Text.Font = (GameFont)1;
			Widgets.Label(new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width, 25f), Translator.Translate("RimTalk_Knowledge_TagTestResult"));
			y += 25f;
			Text.Font = (GameFont)2;
			if (testResult)
			{
				GUI.color = new Color(0.3f, 0.9f, 0.3f);
				Widgets.Label(new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width, 30f), Translator.Translate("RimTalk_Knowledge_TagTestMatched"));
			}
			else
			{
				GUI.color = new Color(0.9f, 0.3f, 0.3f);
				Widgets.Label(new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width, 30f), Translator.Translate("RimTalk_Knowledge_TagTestNotMatched"));
			}
			GUI.color = Color.white;
			Text.Font = (GameFont)1;
			y += 35f;
			Widgets.Label(new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width, 25f), Translator.Translate("RimTalk_Knowledge_TagTestMatchText"));
			y += 25f;
			float num = ((Rect)(ref rect)).yMax - y;
			Rect val = new Rect(((Rect)(ref rect)).x, y, ((Rect)(ref rect)).width, num);
			Text.Font = (GameFont)0;
			float num2 = Text.CalcHeight(testMatchText, ((Rect)(ref rect)).width - 20f);
			Text.Font = (GameFont)1;
			Rect val2 = default(Rect);
			((Rect)(ref val2))..ctor(0f, 0f, ((Rect)(ref rect)).width - 16f, Mathf.Max(num2 + 20f, num));
			Widgets.BeginScrollView(val, ref scrollPosition, val2, true);
			Rect val3 = new Rect(5f, 5f, ((Rect)(ref val2)).width - 10f, num2 + 10f);
			Widgets.DrawBoxSolid(val3, new Color(0.1f, 0.1f, 0.1f, 0.8f));
			Rect val4 = GenUI.ContractedBy(val3, 5f);
			Text.Font = (GameFont)0;
			GUI.color = new Color(0.9f, 0.9f, 0.9f);
			Widgets.Label(val4, testMatchText);
			GUI.color = Color.white;
			Text.Font = (GameFont)1;
			Widgets.EndScrollView();
		}

		private void ShowPawnSelectionMenu()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Expected O, but got Unknown
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Expected O, but got Unknown
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Expected O, but got Unknown
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_TagTestNoPawn")), (Action)delegate
			{
				testPawn = null;
			}, (MenuOptionPriority)4, (Action<Rect>)null, (Thing)null, 0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0));
			if (Find.CurrentMap != null)
			{
				foreach (Pawn item in from p in Find.CurrentMap.mapPawns.AllPawns
					where p.RaceProps.Humanlike
					orderby ((Thing)p).Faction != Faction.OfPlayer, ((Entity)p).LabelShort
					select p)
				{
					string text = ((Entity)item).LabelShort ?? "";
					if (((Thing)item).Faction != null)
					{
						text = text + " (" + ((Thing)item).Faction.Name + ")";
					}
					Pawn localPawn = item;
					list.Add(new FloatMenuOption(text, (Action)delegate
					{
						testPawn = localPawn;
					}, (MenuOptionPriority)4, (Action<Rect>)null, (Thing)null, 0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0));
				}
			}
			Find.WindowStack.Add((Window)new FloatMenu(list));
		}

		private void ExecuteTagTest()
		{
			if (string.IsNullOrWhiteSpace(testTag))
			{
				Messages.Message("请输入标签", MessageTypeDefOf.RejectInput, false);
				return;
			}
			CommonKnowledgeEntry entry = new CommonKnowledgeEntry(testTag, "测试内容")
			{
				matchMode = KeywordMatchMode.Any
			};
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(testContext);
			if (testPawn != null)
			{
				stringBuilder.Append(" ");
				stringBuilder.Append(BuildPawnInfoText(testPawn));
			}
			testMatchText = stringBuilder.ToString();
			testResult = TestTagMatch(testMatchText, entry);
			testExecuted = true;
			Log.Message("[标签测试] 标签: " + testTag);
			Log.Message("[标签测试] 对话: " + testContext);
			Log.Message("[标签测试] Pawn: " + ((testPawn != null) ? ((Entity)testPawn).LabelShort : "无"));
			Log.Message("[标签测试] 实际匹配文本: " + testMatchText);
			Log.Message("[标签测试] 结果: " + (testResult ? "匹配成功" : "不匹配"));
		}

		private string BuildPawnInfoText(Pawn pawn)
		{
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_043a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0484: Unknown result type (might be due to invalid IL or missing references)
			if (pawn == null)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				Name name = pawn.Name;
				if (!string.IsNullOrEmpty((name != null) ? name.ToStringShort : null))
				{
					stringBuilder.Append(pawn.Name.ToStringShort);
					stringBuilder.Append(" ");
				}
				if (pawn.RaceProps != null && pawn.RaceProps.Humanlike)
				{
					float ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
					if (ageBiologicalYearsFloat < 3f)
					{
						stringBuilder.Append("婴儿 宝宝 ");
					}
					else if (ageBiologicalYearsFloat < 13f)
					{
						stringBuilder.Append("儿童 小孩 ");
					}
					else if (ageBiologicalYearsFloat < 18f)
					{
						stringBuilder.Append("青少年 ");
					}
					else
					{
						stringBuilder.Append("成人 ");
					}
				}
				stringBuilder.Append(GenderUtility.GetLabel(pawn.gender, false));
				stringBuilder.Append(" ");
				if (((Thing)pawn).def != null)
				{
					stringBuilder.Append(((Def)((Thing)pawn).def).label);
					stringBuilder.Append(" ");
					try
					{
						if (pawn.genes != null && pawn.genes.Xenotype != null)
						{
							string value = ((Def)pawn.genes.Xenotype).label ?? ((Def)pawn.genes.Xenotype).defName;
							if (!string.IsNullOrEmpty(value))
							{
								stringBuilder.Append(value);
								stringBuilder.Append(" ");
							}
						}
					}
					catch
					{
					}
				}
				if (pawn.IsColonist)
				{
					stringBuilder.Append("殖民者 ");
				}
				else if (pawn.IsPrisoner)
				{
					stringBuilder.Append("囚犯 ");
				}
				else if (pawn.IsSlaveOfColony)
				{
					stringBuilder.Append("奴隶 ");
				}
				else if (pawn.HostFaction == Faction.OfPlayer)
				{
					stringBuilder.Append("访客 ");
				}
				else if (((Thing)pawn).Faction != null && ((Thing)pawn).Faction != Faction.OfPlayer)
				{
					stringBuilder.Append(((Thing)pawn).Faction.Name);
					stringBuilder.Append(" ");
				}
				if (pawn.story?.traits != null)
				{
					foreach (Trait allTrait in pawn.story.traits.allTraits)
					{
						if (allTrait != null && ((Def)(allTrait.def?)).label != null)
						{
							stringBuilder.Append(((Def)allTrait.def).label);
							stringBuilder.Append(" ");
						}
					}
				}
				if (pawn.skills != null)
				{
					foreach (SkillRecord skill in pawn.skills.skills)
					{
						if (skill.TotallyDisabled || ((Def)(skill.def?)).label == null)
						{
							continue;
						}
						int level = skill.Level;
						if (level >= 5)
						{
							stringBuilder.Append(((Def)skill.def).label);
							stringBuilder.Append(level);
							stringBuilder.Append(" ");
							if (level >= 15)
							{
								stringBuilder.Append(((Def)skill.def).label);
								stringBuilder.Append("精通 ");
							}
							else if (level >= 10)
							{
								stringBuilder.Append(((Def)skill.def).label);
								stringBuilder.Append("熟练 ");
							}
						}
					}
				}
				if (pawn.health != null)
				{
					if (GenCollection.Any<BodyPartRecord>(pawn.health.hediffSet.GetInjuredParts()))
					{
						stringBuilder.Append("受伤 ");
					}
					else if (!pawn.health.HasHediffsNeedingTend(false))
					{
						stringBuilder.Append("健康 ");
					}
				}
				if (pawn.relations != null)
				{
					foreach (Pawn item in pawn.relations.RelatedPawns.Take(5))
					{
						Name name2 = item.Name;
						if (!string.IsNullOrEmpty((name2 != null) ? name2.ToStringShort : null))
						{
							stringBuilder.Append(item.Name.ToStringShort);
							stringBuilder.Append(" ");
						}
					}
				}
				Pawn_StoryTracker story = pawn.story;
				if (((story != null) ? story.Adulthood : null) != null)
				{
					string value2 = pawn.story.Adulthood.TitleFor(pawn.gender);
					if (!string.IsNullOrEmpty(value2))
					{
						stringBuilder.Append(value2);
						stringBuilder.Append(" ");
					}
				}
				Pawn_StoryTracker story2 = pawn.story;
				if (((story2 != null) ? story2.Childhood : null) != null)
				{
					string value3 = pawn.story.Childhood.TitleFor(pawn.gender);
					if (!string.IsNullOrEmpty(value3))
					{
						stringBuilder.Append(value3);
						stringBuilder.Append(" ");
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warning("[标签测试] 构建Pawn信息时出错: " + ex.Message);
			}
			return stringBuilder.ToString().Trim();
		}

		private bool TestTagMatch(string text, CommonKnowledgeEntry entry)
		{
			List<string> tags = entry.GetTags();
			if (tags == null || tags.Count == 0)
			{
				return false;
			}
			switch (entry.matchMode)
			{
			case KeywordMatchMode.Any:
				foreach (string item in tags)
				{
					if (!string.IsNullOrWhiteSpace(item) && text.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return true;
					}
				}
				return false;
			case KeywordMatchMode.All:
				foreach (string item2 in tags)
				{
					if (!string.IsNullOrWhiteSpace(item2) && text.IndexOf(item2, StringComparison.OrdinalIgnoreCase) < 0)
					{
						return false;
					}
				}
				return true;
			default:
				return false;
			}
		}
	}
	public class Dialog_TextInput : Window
	{
		private string title;

		private string description;

		private string text;

		private Action<string> onAccept;

		private Action onCancel;

		private bool multiline;

		private Vector2 scrollPos;

		public override Vector2 InitialSize
		{
			get
			{
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				return new Vector2(600f, multiline ? 500f : 250f);
			}
		}

		public Dialog_TextInput(string title, string description, string initialText, Action<string> onAccept, Action onCancel = null, bool multiline = false)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			scrollPos = Vector2.zero;
			((Window)this)..ctor((IWindowDrawing)null);
			this.title = title;
			this.description = description;
			text = initialText ?? "";
			this.onAccept = onAccept;
			this.onCancel = onCancel;
			this.multiline = multiline;
			base.doCloseX = true;
			base.closeOnClickedOutside = false;
			base.absorbInputAroundWindow = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			Text.Font = (GameFont)2;
			Widgets.Label(new Rect(0f, 0f, ((Rect)(ref inRect)).width, 35f), title);
			Text.Font = (GameFont)1;
			float num = 40f;
			if (!string.IsNullOrEmpty(description))
			{
				float num2 = Text.CalcHeight(description, ((Rect)(ref inRect)).width);
				Widgets.Label(new Rect(0f, num, ((Rect)(ref inRect)).width, num2), description);
				num += num2 + 10f;
			}
			Rect val = default(Rect);
			((Rect)(ref val))..ctor(0f, num, ((Rect)(ref inRect)).width, ((Rect)(ref inRect)).height - num - 50f);
			if (multiline)
			{
				Rect val2 = default(Rect);
				((Rect)(ref val2))..ctor(0f, 0f, ((Rect)(ref val)).width - 16f, Mathf.Max(Text.CalcHeight(text, ((Rect)(ref val)).width - 16f), ((Rect)(ref val)).height));
				Widgets.BeginScrollView(val, ref scrollPos, val2, true);
				text = Widgets.TextArea(val2, text, false);
				Widgets.EndScrollView();
			}
			else
			{
				text = Widgets.TextField(val, text);
			}
			Rect val3 = default(Rect);
			((Rect)(ref val3))..ctor(((Rect)(ref inRect)).width - 220f, ((Rect)(ref inRect)).height - 40f, 100f, 35f);
			if (Widgets.ButtonText(val3, TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_Save")), true, true, true, (TextAnchor?)null))
			{
				onAccept?.Invoke(text);
				((Window)this).Close(true);
			}
			((Rect)(ref val3)).x = ((Rect)(ref val3)).x + 110f;
			if (Widgets.ButtonText(val3, TaggedString.op_Implicit(Translator.Translate("RimTalk_Knowledge_Cancel")), true, true, true, (TextAnchor?)null))
			{
				onCancel?.Invoke();
				((Window)this).Close(true);
			}
		}
	}
	public class ITab_Memory : ITab
	{
		private class MemoryListEntry
		{
			public MemoryEntry memory;

			public bool isShortTerm;
		}

		private VirtualListView<MemoryListEntry> virtualList;

		private readonly List<MemoryListEntry> cachedEntries;

		private MemoryType? filterType;

		private bool showShortTerm;

		private bool showLongTerm;

		public ITab_Memory()
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			cachedEntries = new List<MemoryListEntry>();
			showShortTerm = true;
			showLongTerm = true;
			((ITab)this)..ctor();
			((InspectTabBase)this).size = new Vector2(500f, 600f);
			((InspectTabBase)this).labelKey = "RimTalk_TabMemory";
			((InspectTabBase)this).tutorTag = "RimTalkMemory";
			virtualList = new VirtualListView<MemoryListEntry>((MemoryListEntry _) => 80f, delegate(Rect r, MemoryListEntry entry, int _)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				DrawMemoryRow(r, entry.memory, entry.isShortTerm);
			})
			{
				ItemSpacing = 5f,
				EmptyLabel = TaggedString.op_Implicit(Translator.Translate("RimTalk_Memory_NoMemories"))
			};
		}

		protected override void FillTab()
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			Thing singleSelectedThing = Find.Selector.SingleSelectedThing;
			Pawn val = (Pawn)(object)((singleSelectedThing is Pawn) ? singleSelectedThing : null);
			if (val != null)
			{
				PawnMemoryComp pawnMemoryComp = ThingCompUtility.TryGetComp<PawnMemoryComp>((Thing)(object)val);
				if (pawnMemoryComp != null)
				{
					Rect val2 = GenUI.ContractedBy(new Rect(0f, 0f, ((InspectTabBase)this).size.x, ((InspectTabBase)this).size.y), 10f);
					GUI.BeginGroup(val2);
					Rect val3 = new Rect(0f, 0f, ((Rect)(ref val2)).width, 40f);
					Text.Font = (GameFont)2;
					Widgets.Label(val3, TranslatorFormattedStringExtensions.Translate("RimTalk_MemoryTitle", NamedArgument.op_Implicit(((Entity)val).LabelShort)));
					Text.Font = (GameFont)1;
					Rect rect = default(Rect);
					((Rect)(ref rect))..ctor(0f, 45f, ((Rect)(ref val2)).width, 30f);
					DrawFilterButtons(rect);
					Rect rect2 = default(Rect);
					((Rect)(ref rect2))..ctor(0f, 80f, ((Rect)(ref val2)).width, 30f);
					DrawMemoryTypeToggles(rect2);
					Rect rect3 = default(Rect);
					((Rect)(ref rect3))..ctor(0f, 115f, ((Rect)(ref val2)).width, 40f);
					DrawMemoryStats(rect3, pawnMemoryComp);
					Rect rect4 = default(Rect);
					((Rect)(ref rect4))..ctor(0f, 160f, ((Rect)(ref val2)).width, ((Rect)(ref val2)).height - 160f);
					DrawMemoryList(rect4, pawnMemoryComp);
					GUI.EndGroup();
				}
			}
		}

		private void DrawFilterButtons(Rect rect)
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			List<MemoryType> list = Enum.GetValues(typeof(MemoryType)).Cast<MemoryType>().ToList();
			int num = list.Count + 1;
			float num2 = ((Rect)(ref rect)).width / (float)num;
			if (Widgets.ButtonText(new Rect(((Rect)(ref rect)).x, ((Rect)(ref rect)).y, num2, ((Rect)(ref rect)).height), TaggedString.op_Implicit(Translator.Translate("RimTalk_Filter_All")), true, true, true, (TextAnchor?)null))
			{
				filterType = null;
			}
			for (int i = 0; i < list.Count; i++)
			{
				MemoryType value = list[i];
				Rect val = new Rect(((Rect)(ref rect)).x + num2 * (float)(i + 1), ((Rect)(ref rect)).y, num2, ((Rect)(ref rect)).height);
				string text = TaggedString.op_Implicit(Translator.Translate("RimTalk_Filter_" + value));
				if (Widgets.ButtonText(val, text, true, true, true, (TextAnchor?)null))
				{
					filterType = value;
				}
			}
		}

		private void DrawMemoryTypeToggles(Rect rect)
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			float num = ((Rect)(ref rect)).width / 2f;
			Rect val = new Rect(((Rect)(ref rect)).x, ((Rect)(ref rect)).y, num - 2f, ((Rect)(ref rect)).height);
			string text = TaggedString.op_Implicit(Translator.Translate("RimTalk_ShortTermMemories") + (showShortTerm ? " ✓" : ""));
			if (Widgets.ButtonText(val, text, true, true, true, (TextAnchor?)null))
			{
				bool flag = !showShortTerm;
				if (!flag && !showLongTerm)
				{
					showLongTerm = true;
				}
				showShortTerm = flag;
			}
			Rect val2 = new Rect(((Rect)(ref rect)).x + num + 2f, ((Rect)(ref rect)).y, num - 2f, ((Rect)(ref rect)).height);
			string text2 = TaggedString.op_Implicit(Translator.Translate("RimTalk_LongTermMemories") + (showLongTerm ? " ✓" : ""));
			if (Widgets.ButtonText(val2, text2, true, true, true, (TextAnchor?)null))
			{
				bool flag2 = !showLongTerm;
				if (!flag2 && !showShortTerm)
				{
					showShortTerm = true;
				}
				showLongTerm = flag2;
			}
		}

		private void DrawMemoryStats(Rect rect, PawnMemoryComp memoryComp)
		{
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			Text.Anchor = (TextAnchor)3;
			string text = $"SCM: {memoryComp.SituationalMemories.Count}/{RimTalkMemoryPatchMod.Settings.maxSituationalMemories} | " + $"ELS: {memoryComp.EventLogMemories.Count}/{RimTalkMemoryPatchMod.Settings.maxEventLogMemories}";
			Widgets.Label(rect, text);
			Text.Anchor = (TextAnchor)0;
		}

		private void DrawMemoryList(Rect rect, PawnMemoryComp memoryComp)
		{
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			cachedEntries.Clear();
			if (showShortTerm)
			{
				foreach (MemoryEntry situationalMemory in memoryComp.SituationalMemories)
				{
					if (!filterType.HasValue || situationalMemory.Type == filterType.Value)
					{
						cachedEntries.Add(new MemoryListEntry
						{
							memory = situationalMemory,
							isShortTerm = true
						});
					}
				}
			}
			if (showLongTerm)
			{
				foreach (MemoryEntry eventLogMemory in memoryComp.EventLogMemories)
				{
					if (!filterType.HasValue || eventLogMemory.Type == filterType.Value)
					{
						cachedEntries.Add(new MemoryListEntry
						{
							memory = eventLogMemory,
							isShortTerm = false
						});
					}
				}
			}
			virtualList.EmptyLabel = TaggedString.op_Implicit(Translator.Translate("RimTalk_Memory_NoMemories"));
			virtualList.SetItems(cachedEntries);
			virtualList.Draw(rect);
		}

		private void DrawMemoryRow(Rect rect, MemoryEntry memory, bool isShortTerm)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			Widgets.DrawBoxSolid(rect, new Color(0.2f, 0.2f, 0.2f, 0.5f));
			Widgets.DrawBox(rect, 1, (Texture2D)null);
			Rect val = GenUI.ContractedBy(rect, 5f);
			Rect val2 = new Rect(((Rect)(ref val)).x, ((Rect)(ref val)).y, ((Rect)(ref val)).width, 20f);
			Text.Font = (GameFont)0;
			string text = TaggedString.op_Implicit(Translator.Translate("RimTalk_MemoryType_" + memory.Type));
			string text2 = "[" + text + "] " + memory.AgeString;
			if (!string.IsNullOrEmpty(memory.relatedPawnName))
			{
				text2 = TaggedString.op_Implicit(text2 + (" - " + Translator.Translate("RimTalk_With") + " " + memory.relatedPawnName));
			}
			Widgets.Label(val2, text2);
			Text.Font = (GameFont)1;
			string text3 = memory.Content;
			if (!string.IsNullOrEmpty(text3) && text3.Length > 80)
			{
				text3 = text3.Substring(0, 77) + "...";
			}
			Rect val3 = default(Rect);
			((Rect)(ref val3))..ctor(((Rect)(ref val)).x, ((Rect)(ref val)).y + 22f, ((Rect)(ref val)).width, 40f);
			Widgets.Label(val3, text3);
			if (Mouse.IsOver(val3))
			{
				TooltipHandler.TipRegion(val3, TipSignal.op_Implicit(memory.Content));
			}
			Widgets.FillableBar(new Rect(((Rect)(ref val)).x, ((Rect)(ref val)).y + 64f, ((Rect)(ref val)).width, 8f), Mathf.Clamp01(memory.Importance), BaseContent.WhiteTex, (Texture2D)null, false);
			Text.Font = (GameFont)1;
		}
	}
	public class MainTabWindow_Memory : MainTabWindow
	{
		private Pawn selectedPawn;

		private FourLayerMemoryComp currentMemoryComp;

		private bool showAllHumanlikes;

		private HashSet<MemoryEntry> selectedMemories;

		private MemoryEntry lastSelectedMemory;

		private bool isDragging;

		private bool isMouseDown;

		private Vector2 dragStartPos;

		private Vector2 dragCurrentPos;

		private Vector2 mouseDownScreenPos;

		private const float DRAG_THRESHOLD = 5f;

		private Vector2 timelineScrollPosition;

		private MemoryType? filterType;

		private bool showABM;

		private bool showSCM;

		private bool showELS;

		private bool showCLPA;

		private List<MemoryEntry> cachedFilteredMemories;

		private bool filtersDirty;

		private const float TOP_BAR_HEIGHT = 50f;

		private const float CONTROL_PANEL_WIDTH = 220f;

		private const float SPACING = 10f;

		private const float CARD_WIDTH_FULL = 600f;

		private const float CARD_SPACING = 8f;

		private List<MemoryEntry> cachedMemories;

		private List<float> cachedCardHeights;

		private List<float> cachedCardYPositions;

		private float cachedTotalHeight;

		private int lastMemoryCount;

		private bool lastShowABM;

		private bool lastShowSCM;

		private bool lastShowELS;

		private bool lastShowCLPA;

		private MemoryType? lastFilterType;

		private Pawn lastSelectedPawn;

		private int lastRefreshTick;

		public override Vector2 RequestedTabSize
		{
			get
			{
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				return new Vector2(1200f, 700f);
			}
		}

		public void InvalidateCache()
		{
			lastMemoryCount = -1;
			filtersDirty = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Invalid comparison between Unknown and I4
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			Rect rect = default(Rect);
			((Rect)(ref rect))..ctor(0f, 0f, ((Rect)(ref inRect)).width, 50f);
			DrawTopBar(rect);
			float num = 60f;
			float num2 = ((Rect)(ref inRect)).height - num;
			if (selectedPawn == null)
			{
				DrawNoPawnSelected(new Rect(0f, num, ((Rect)(ref inRect)).width, num2));
				return;
			}
			FourLayerMemoryComp fourLayerMemoryComp = ThingCompUtility.TryGetComp<FourLayerMemoryComp>((Thing)(object)selectedPawn);
			if (fourLayerMemoryComp == null)
			{
				DrawNoMemoryComponent(new Rect(0f, num, ((Rect)(ref inRect)).width, num2));
				return;
			}
			currentMemoryComp = fourLayerMemoryComp;
			CheckAndRefreshCache();
			Rect rect2 = default(Rect);
			((Rect)(ref rect2))..ctor(0f, num, 220f, num2);
			DrawControlPanel(rect2);
			float num3 = 230f;
			float num4 = ((Rect)(ref inRect)).width - num3;
			Rect rect3 = default(Rect);
			((Rect)(ref rect3))..ctor(num3, num, num4, num2);
			DrawTimeline(rect3);
			if ((int)Event.current.type != 1 || Event.current.button != 0)
			{
				return;
			}
			if (isMouseDown && !isDragging)
			{
				float num5 = 230f;
				float num6 = ((Rect)(ref inRect)).width - num5;
				Rect val = GenUI.ContractedBy(new Rect(num5, num, num6, num2), 5f);
				if (((Rect)(ref val)).Contains(Event.current.mousePosition))
				{
					float num7 = Event.current.mousePosition.y - ((Rect)(ref val)).y + timelineScrollPosition.y;
					MemoryEntry memoryEntry = null;
					for (int i = 0; i < cachedMemories.Count; i++)
					{
						float num8 = cachedCardYPositions[i];
						float num9 = cachedCardHeights[i];
						if (num7 >= num8 && num7 < num8 + num9)
						{
							memoryEntry = cachedMemories[i];
							break;
						}
					}
					if (memoryEntry != null)
					{
						HandleMemoryClick(memoryEntry);
					}
					else
					{
						selectedMemories.Clear();
						lastSelectedMemory = null;
					}
				}
			}
			isMouseDown = false;
			isDragging = false;
			Event.current.Use();
		}

		private void SummarizeMemories(List<MemoryEntry> targetMemories)
		{
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			if (currentMemoryComp != null && targetMemories != null && targetMemories.Count != 0)
			{
				Find.WindowStack.Add((Window)(object)Dialog_MessageBox.CreateConfirmation(TaggedString.op_Implicit($"确定要总结选中的 {targetMemories.Count} 条记忆吗？"), (Action)delegate
				{
					currentMemoryComp.Summarizer.ManualSummarize(targetMemories);
					selectedMemories.Clear();
					filtersDirty = true;
					Messages.Message("总结命令已下发", MessageTypeDefOf.SilentInput, false);
				}, false, (string)null, (WindowLayer)1));
			}
		}

		private void ArchiveMemories(List<MemoryEntry> targetMemories)
		{
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			if (currentMemoryComp != null && targetMemories != null && targetMemories.Count != 0)
			{
				Find.WindowStack.Add((Window)(object)Dialog_MessageBox.CreateConfirmation(TaggedString.op_Implicit($"确定要归档选中的 {targetMemories.Count} 条记忆吗？"), (Action)delegate
				{
					currentMemoryComp.Summarizer.Archive(targetMemories);
					selectedMemories.Clear();
					filtersDirty = true;
					Messages.Message("归档命令已下发", MessageTypeDefOf.SilentInput, false);
				}, false, (string)null, (WindowLayer)1));
			}
		}

		private void DeleteMemories(List<MemoryEntry> targetMemories)
		{
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			if (currentMemoryComp == null || targetMemories == null || targetMemories.Count == 0)
			{
				return;
			}
			int count = targetMemories.Count;
			Find.WindowStack.Add((Window)(object)Dialog_MessageBox.CreateConfirmation(TranslatorFormattedStringExtensions.Translate("RimTalk_MindStream_DeleteConfirm", NamedArgument.op_Implicit(count)), (Action)delegate
			{
				//IL_0072: Unknown result type (might be due to invalid IL or missing references)
				//IL_0077: Unknown result type (might be due to invalid IL or missing references)
				foreach (MemoryEntry item in targetMemories.ToList())
				{
					currentMemoryComp.Maintainer.Remove(item);
				}
				selectedMemories.Clear();
				filtersDirty = true;
				Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_MindStream_DeletedN", NamedArgument.op_Implicit(count))), MessageTypeDefOf.PositiveEvent, false);
			}, false, (string)null, (WindowLayer)1));
		}

		private void SummarizeAll()
		{
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			List<Pawn> list = new List<Pawn>();
			foreach (Map map in Find.Maps)
			{
				foreach (Pawn freeColonist in map.mapPawns.FreeColonists)
				{
					PawnMemoryComp pawnMemoryComp = ThingCompUtility.TryGetComp<PawnMemoryComp>((Thing)(object)freeColonist);
					if (pawnMemoryComp != null && pawnMemoryComp.SituationalMemories.Count > 0)
					{
						list.Add(freeColonist);
					}
				}
			}
			if (list.Count > 0)
			{
				foreach (Pawn item in list)
				{
					if (item != null)
					{
						ThingCompUtility.TryGetComp<FourLayerMemoryComp>((Thing)(object)item)?.Summarizer.AutoSummarize();
					}
				}
				Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_MindStream_QueuedSummarization", NamedArgument.op_Implicit(list.Count))), MessageTypeDefOf.TaskCompletion, false);
			}
			else
			{
				Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_MindStream_NoNeedSummarization")), MessageTypeDefOf.RejectInput, false);
			}
		}

		private void DrawControlPanel(Rect rect)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			Widgets.DrawMenuSection(rect);
			Rect parentRect = GenUI.ContractedBy(rect, 10f);
			float y = ((Rect)(ref parentRect)).y;
			Text.Font = (GameFont)2;
			Widgets.Label(new Rect(((Rect)(ref parentRect)).x, y, ((Rect)(ref parentRect)).width, 30f), Translator.Translate("RimTalk_MindStream_MemoryFilters"));
			Text.Font = (GameFont)1;
			y += 35f;
			y = DrawLayerFilters(parentRect, y);
			y += 10f;
			y = DrawTypeFilters(parentRect, y);
			y += 10f;
			Widgets.DrawLineHorizontal(((Rect)(ref parentRect)).x, y, ((Rect)(ref parentRect)).width);
			y += 15f;
			y = DrawBatchActions(parentRect, y);
			y += 10f;
			DrawGlobalActions(parentRect, y);
		}

		private float DrawLayerFilters(Rect parentRect, float startY)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_01de: Unknown result type (might be due to invalid IL or missing references)
			float num = startY;
			Text.Font = (GameFont)0;
			GUI.color = new Color(0.8f, 0.8f, 0.8f);
			Widgets.Label(new Rect(((Rect)(ref parentRect)).x, num, ((Rect)(ref parentRect)).width, 20f), Translator.Translate("RimTalk_MindStream_Layers"));
			GUI.color = Color.white;
			Text.Font = (GameFont)1;
			num += 22f;
			float num2 = 24f;
			bool flag = showABM;
			bool flag2 = showSCM;
			bool flag3 = showELS;
			bool flag4 = showCLPA;
			Rect rect = default(Rect);
			((Rect)(ref rect))..ctor(((Rect)(ref parentRect)).x, num, ((Rect)(ref parentRect)).width, num2);
			Color color = default(Color);
			((Color)(ref color))..ctor(0.3f, 0.8f, 1f);
			DrawColoredCheckbox(rect, TaggedString.op_Implicit(Translator.Translate("RimTalk_MindStream_ABM")), ref showABM, color, MemoryLayer.Active);
			num += num2 + 2f;
			Rect rect2 = default(Rect);
			((Rect)(ref rect2))..ctor(((Rect)(ref parentRect)).x, num, ((Rect)(ref parentRect)).width, num2);
			Color color2 = default(Color);
			((Color)(ref color2))..ctor(0.3f, 1f, 0.5f);
			DrawColoredCheckbox(rect2, TaggedString.op_Implicit(Translator.Translate("RimTalk_MindStream_SCM")), ref showSCM, color2, MemoryLayer.Situational);
			num += num2 + 2f;
			Rect rect3 = default(Rect);
			((Rect)(ref rect3))..ctor(((Rect)(ref parentRect)).x, num, ((Rect)(ref parentRect)).width, num2);
			Color color3 = default(Color);
			((Color)(ref color3))..ctor(1f, 0.8f, 0.3f);
			DrawColoredCheckbox(rect3, TaggedString.op_Implicit(Translator.Translate("RimTalk_MindStream_ELS")), ref showELS, color3, MemoryLayer.EventLog);
			num += num2 + 2f;
			Rect rect4 = default(Rect);
			((Rect)(ref rect4))..ctor(((Rect)(ref parentRect)).x, num, ((Rect)(ref parentRect)).width, num2);
			Color color4 = default(Color);
			((Color)(ref color4))..ctor(0.8f, 0.4f, 1f);
			DrawColoredCheckbox(rect4, TaggedString.op_Implicit(Translator.Translate("RimTalk_MindStream_CLPA")), ref showCLPA, color4, MemoryLayer.Archive);
			num += num2;
			if (showABM != flag || showSCM != flag2 || showELS != flag3 || showCLPA != flag4)
			{
				filtersDirty = true;
			}
			return num;
		}

		private void DrawColoredCheckbox(Rect rect, string label, ref bool value, Color color, MemoryLayer? rightClickLayer)
		{
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			if (rightClickLayer.HasValue && (int)Event.current.type == 0 && Event.current.button == 1 && ((Rect)(ref rect)).Contains(Event.current.mousePosition))
			{
				ShowCreateMemoryMenu(rightClickLayer.Value);
				Event.current.Use();
				return;
			}
			Widgets.DrawBoxSolid(new Rect(((Rect)(ref rect)).x, ((Rect)(ref rect)).y + 2f, 3f, ((Rect)(ref rect)).height - 4f), color);
			Widgets.CheckboxLabeled(new Rect(((Rect)(ref rect)).x + 8f, ((Rect)(ref rect)).y, ((Rect)(ref rect)).width - 8f, ((Rect)(ref rect)).height), label, ref value, false, (Texture2D)null, (Texture2D)null, false, false);
			if (rightClickLayer.HasValue && Mouse.IsOver(rect))
			{
				TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_MindStream_RightClickToCreate", NamedArgument.op_Implicit(GetLayerLabel(rightClickLayer.Value)))));
			}
		}

		private float DrawTypeFilters(Rect parentRect, float startY)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0209: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0252: Unknown result type (might be due to invalid IL or missing references)
			float num = startY;
			Text.Font = (GameFont)0;
			GUI.color = new Color(0.8f, 0.8f, 0.8f);
			Widgets.Label(new Rect(((Rect)(ref parentRect)).x, num, ((Rect)(ref parentRect)).width, 20f), Translator.Translate("RimTalk_MindStream_Type"));
			GUI.color = Color.white;
			Text.Font = (GameFont)1;
			num += 22f;
			float num2 = 28f;
			float num3 = 2f;
			if (!filterType.HasValue)
			{
				GUI.color = new Color(0.5f, 0.7f, 1f);
			}
			if (Widgets.ButtonText(new Rect(((Rect)(ref parentRect)).x, num, ((Rect)(ref parentRect)).width, num2), TaggedString.op_Implicit(Translator.Translate("RimTalk_MindStream_All")), true, true, true, (TextAnchor?)null) && filterType.HasValue)
			{
				filterType = null;
				selectedMemories.Clear();
				filtersDirty = true;
			}
			GUI.color = Color.white;
			num += num2 + num3;
			if (filterType == MemoryType.Conversation)
			{
				GUI.color = new Color(0.5f, 0.7f, 1f);
			}
			if (Widgets.ButtonText(new Rect(((Rect)(ref parentRect)).x, num, ((Rect)(ref parentRect)).width, num2), TaggedString.op_Implicit(Translator.Translate("RimTalk_MindStream_Conversation")), true, true, true, (TextAnchor?)null) && filterType != MemoryType.Conversation)
			{
				filterType = MemoryType.Conversation;
				selectedMemories.Clear();
				filtersDirty = true;
			}
			GUI.color = Color.white;
			num += num2 + num3;
			if (filterType == MemoryType.Action)
			{
				GUI.color = new Color(0.5f, 0.7f, 1f);
			}
			if (Widgets.ButtonText(new Rect(((Rect)(ref parentRect)).x, num, ((Rect)(ref parentRect)).width, num2), TaggedString.op_Implicit(Translator.Translate("RimTalk_MindStream_Action")), true, true, true, (TextAnchor?)null) && filterType != MemoryType.Action)
			{
				filterType = MemoryType.Action;
				selectedMemories.Clear();
				filtersDirty = true;
			}
			GUI.color = Color.white;
			return num + num2;
		}

		private float DrawBatchActions(Rect parentRect, float startY)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_025e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0263: Unknown result type (might be due to invalid IL or missing references)
			//IL_023a: Unknown result type (might be due to invalid IL or missing references)
			//IL_023f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0244: Unknown result type (might be due to invalid IL or missing references)
			//IL_027f: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0309: Unknown result type (might be due to invalid IL or missing references)
			//IL_030e: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_032a: Unknown result type (might be due to invalid IL or missing references)
			//IL_034c: Unknown result type (might be due to invalid IL or missing references)
			float num = startY;
			Text.Font = (GameFont)0;
			GUI.color = new Color(0.8f, 0.8f, 0.8f);
			Widgets.Label(new Rect(((Rect)(ref parentRect)).x, num, ((Rect)(ref parentRect)).width, 20f), Translator.Translate("RimTalk_MindStream_BatchActions"));
			GUI.color = Color.white;
			Text.Font = (GameFont)1;
			num += 22f;
			float num2 = 32f;
			float num3 = 5f;
			bool num4 = selectedMemories.Count > 0;
			List<MemoryEntry> list = (num4 ? selectedMemories.ToList() : cachedMemories);
			int count = list.Count;
			int num5 = GenCollection.Count<MemoryEntry>(list, (Predicate<MemoryEntry>)((MemoryEntry m) => m.Layer == MemoryLayer.Active));
			int num6 = GenCollection.Count<MemoryEntry>(list, (Predicate<MemoryEntry>)((MemoryEntry m) => m.Layer == MemoryLayer.Situational));
			GUI.enabled = num5 + num6 > 0;
			string text = (num4 ? ((num5 > 0 && num6 > 0) ? TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_MindStream_SummarizeSelectedBoth", NamedArgument.op_Implicit(num5), NamedArgument.op_Implicit(num6))) : ((num5 <= 0) ? TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_MindStream_SummarizeSelectedSCM", NamedArgument.op_Implicit(num6))) : TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_MindStream_SummarizeSelectedABM", NamedArgument.op_Implicit(num5))))) : ((num5 > 0 && num6 > 0) ? TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_MindStream_SummarizeAllBoth", NamedArgument.op_Implicit(num5), NamedArgument.op_Implicit(num6))) : ((num5 <= 0) ? TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_MindStream_SummarizeAllSCM", NamedArgument.op_Implicit(num6))) : TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_MindStream_SummarizeAllABM", NamedArgument.op_Implicit(num5))))));
			if (Widgets.ButtonText(new Rect(((Rect)(ref parentRect)).x, num, ((Rect)(ref parentRect)).width, num2), text, true, true, true, (TextAnchor?)null))
			{
				SummarizeMemories(list);
			}
			GUI.enabled = true;
			num += num2 + num3;
			int num7 = GenCollection.Count<MemoryEntry>(list, (Predicate<MemoryEntry>)((MemoryEntry m) => m.Layer == MemoryLayer.EventLog));
			GUI.enabled = num7 > 0;
			string text2 = ((!num4) ? TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_MindStream_ArchiveAllCount", NamedArgument.op_Implicit(num7))) : ((object)TranslatorFormattedStringExtensions.Translate("RimTalk_MindStream_ArchiveN", NamedArgument.op_Implicit(count))/*cast due to constrained. prefix*/).ToString());
			if (Widgets.ButtonText(new Rect(((Rect)(ref parentRect)).x, num, ((Rect)(ref parentRect)).width, num2), text2, true, true, true, (TextAnchor?)null))
			{
				ArchiveMemories(list);
			}
			GUI.enabled = true;
			num += num2 + num3;
			GUI.enabled = count > 0;
			GUI.color = (Color)((count > 0) ? new Color(1f, 0.4f, 0.4f) : Color.white);
			string text3 = ((!num4) ? TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_MindStream_DeleteAllCount", NamedArgument.op_Implicit(count))) : ((object)TranslatorFormattedStringExtensions.Translate("RimTalk_MindStream_DeleteN", NamedArgument.op_Implicit(count))/*cast due to constrained. prefix*/).ToString());
			if (Widgets.ButtonText(new Rect(((Rect)(ref parentRect)).x, num, ((Rect)(ref parentRect)).width, num2), text3, true, true, true, (TextAnchor?)null))
			{
				DeleteMemories(list);
			}
			GUI.color = Color.white;
			GUI.enabled = true;
			return num + num2;
		}

		private void DrawGlobalActions(Rect parentRect, float startY)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			float num = startY;
			Text.Font = (GameFont)0;
			GUI.color = new Color(0.8f, 0.8f, 0.8f);
			Widgets.Label(new Rect(((Rect)(ref parentRect)).x, num, ((Rect)(ref parentRect)).width, 20f), Translator.Translate("RimTalk_MindStream_GlobalActions"));
			GUI.color = Color.white;
			Text.Font = (GameFont)1;
			num += 22f;
			float num2 = 32f;
			float num3 = 5f;
			if (Widgets.ButtonText(new Rect(((Rect)(ref parentRect)).x, num, ((Rect)(ref parentRect)).width, num2), TaggedString.op_Implicit(Translator.Translate("RimTalk_MindStream_SummarizeAll")), true, true, true, (TextAnchor?)null))
			{
				SummarizeAll();
			}
			num += num2 + num3;
			float num4 = (((Rect)(ref parentRect)).width - num3) / 2f;
			GUI.color = new Color(0.5f, 0.8f, 1f);
			if (Widgets.ButtonText(new Rect(((Rect)(ref parentRect)).x, num, num4, num2), TaggedString.op_Implicit(Translator.Translate("RimTalk_Memory_Export")), true, true, true, (TextAnchor?)null))
			{
				ExportMemories();
			}
			GUI.color = new Color(0.8f, 1f, 0.5f);
			if (Widgets.ButtonText(new Rect(((Rect)(ref parentRect)).x + num4 + num3, num, num4, num2), TaggedString.op_Implicit(Translator.Translate("RimTalk_Memory_Import")), true, true, true, (TextAnchor?)null))
			{
				ImportMemories();
			}
			GUI.color = Color.white;
		}

		private void ShowCreateMemoryMenu(MemoryLayer layer)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Expected O, but got Unknown
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Expected O, but got Unknown
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Expected O, but got Unknown
			if (selectedPawn == null || currentMemoryComp == null)
			{
				Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_MindStream_PleaseSelectColonist")), MessageTypeDefOf.RejectInput, false);
				return;
			}
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			string layerLabel = GetLayerLabel(layer);
			list.Add(new FloatMenuOption(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_MindStream_AddConversationTo", NamedArgument.op_Implicit(layerLabel))), (Action)delegate
			{
				Find.WindowStack.Add((Window)(object)new Dialog_CreateMemory(selectedPawn, currentMemoryComp, layer, MemoryType.Conversation));
			}, (MenuOptionPriority)4, (Action<Rect>)null, (Thing)null, 0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0));
			list.Add(new FloatMenuOption(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_MindStream_AddActionTo", NamedArgument.op_Implicit(layerLabel))), (Action)delegate
			{
				Find.WindowStack.Add((Window)(object)new Dialog_CreateMemory(selectedPawn, currentMemoryComp, layer, MemoryType.Action));
			}, (MenuOptionPriority)4, (Action<Rect>)null, (Thing)null, 0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0));
			Find.WindowStack.Add((Window)new FloatMenu(list));
		}

		private void ExportMemories()
		{
			//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0198: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			if (selectedPawn == null || currentMemoryComp == null)
			{
				Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_Memory_ExportNoPawn")), MessageTypeDefOf.RejectInput, false);
				return;
			}
			try
			{
				string text = $"{selectedPawn.Name.ToStringShort}_Memories_{Find.TickManager.TicksGame}.xml";
				string savePath = Path.Combine(GenFilePaths.SaveDataFolderPath, "MemoryExports");
				if (!Directory.Exists(savePath))
				{
					Directory.CreateDirectory(savePath);
				}
				string text2 = Path.Combine(savePath, text);
				List<MemoryEntry> list = new List<MemoryEntry>();
				list.AddRange(currentMemoryComp.ActiveMemories);
				list.AddRange(currentMemoryComp.SituationalMemories);
				list.AddRange(currentMemoryComp.EventLogMemories);
				list.AddRange(currentMemoryComp.ArchiveMemories);
				string thingID = ((Thing)selectedPawn).ThingID;
				string toStringShort = selectedPawn.Name.ToStringShort;
				Scribe.saver.InitSaving(text2, "MemoryExport");
				Scribe_Values.Look<string>(ref thingID, "pawnId", (string)null, false);
				Scribe_Values.Look<string>(ref toStringShort, "pawnName", (string)null, false);
				Scribe_Collections.Look<MemoryEntry>(ref list, "memories", (LookMode)2, Array.Empty<object>());
				Scribe.saver.FinalizeSaving();
				Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Memory_ExportSuccess", NamedArgument.op_Implicit(list.Count), NamedArgument.op_Implicit(text))), MessageTypeDefOf.PositiveEvent, false);
				Log.Message($"[RimTalk] Exported {list.Count} memories to: {text2}");
				Find.WindowStack.Add((Window)(object)Dialog_MessageBox.CreateConfirmation(Translator.Translate("RimTalk_Memory_OpenExportFolder"), (Action)delegate
				{
					Process.Start(savePath);
				}, true, TaggedString.op_Implicit(Translator.Translate("RimTalk_Memory_ExportSuccessTitle")), (WindowLayer)1));
			}
			catch (Exception ex)
			{
				Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Memory_ExportFailed", NamedArgument.op_Implicit(ex.Message))), MessageTypeDefOf.RejectInput, false);
				Log.Error($"[RimTalk] Memory export failed: {ex}");
			}
		}

		private void ImportMemories()
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Expected O, but got Unknown
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Expected O, but got Unknown
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Expected O, but got Unknown
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dc: Expected O, but got Unknown
			if (selectedPawn == null || currentMemoryComp == null)
			{
				Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_Memory_ImportNoPawn")), MessageTypeDefOf.RejectInput, false);
				return;
			}
			string savePath = Path.Combine(GenFilePaths.SaveDataFolderPath, "MemoryExports");
			if (!Directory.Exists(savePath))
			{
				Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_Memory_ImportNoFolder")), MessageTypeDefOf.RejectInput, false);
				return;
			}
			string[] files = Directory.GetFiles(savePath, "*.xml");
			if (files.Length == 0)
			{
				Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_Memory_ImportNoFiles")), MessageTypeDefOf.RejectInput, false);
				return;
			}
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("RimTalk_Memory_OpenFolder")), (Action)delegate
			{
				Process.Start(savePath);
			}, (MenuOptionPriority)4, (Action<Rect>)null, (Thing)null, 0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0));
			list.Add(new FloatMenuOption("─────────────────────", (Action)null, (MenuOptionPriority)4, (Action<Rect>)null, (Thing)null, 0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0));
			foreach (string file in files.OrderByDescending((string f) => File.GetLastWriteTime(f)))
			{
				string fileName = Path.GetFileName(file);
				FileInfo fileInfo = new FileInfo(file);
				string text = $"{fileName} ({fileInfo.Length / 1024}KB - {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm})";
				list.Add(new FloatMenuOption(text, (Action)delegate
				{
					ImportFromFile(file);
				}, (MenuOptionPriority)4, (Action<Rect>)null, (Thing)null, 0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0));
			}
			Find.WindowStack.Add((Window)new FloatMenu(list));
		}

		private void ImportFromFile(string filePath)
		{
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				List<MemoryEntry> importedMemories = new List<MemoryEntry>();
				string text = "";
				string text2 = "";
				Scribe.loader.InitLoading(filePath);
				Scribe_Values.Look<string>(ref text, "pawnId", (string)null, false);
				Scribe_Values.Look<string>(ref text2, "pawnName", (string)null, false);
				Scribe_Collections.Look<MemoryEntry>(ref importedMemories, "memories", (LookMode)2, Array.Empty<object>());
				Scribe.loader.FinalizeLoading();
				if (importedMemories == null || importedMemories.Count == 0)
				{
					Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_Memory_ImportEmpty")), MessageTypeDefOf.RejectInput, false);
					return;
				}
				Find.WindowStack.Add((Window)(object)Dialog_MessageBox.CreateConfirmation(TranslatorFormattedStringExtensions.Translate("RimTalk_Memory_ImportConfirm", NamedArgument.op_Implicit(text2), NamedArgument.op_Implicit(importedMemories.Count), NamedArgument.op_Implicit(selectedPawn.Name.ToStringShort)), (Action)delegate
				{
					//IL_0142: Unknown result type (might be due to invalid IL or missing references)
					//IL_0152: Unknown result type (might be due to invalid IL or missing references)
					//IL_0157: Unknown result type (might be due to invalid IL or missing references)
					int num = 0;
					foreach (MemoryEntry item in importedMemories)
					{
						switch (item.Layer)
						{
						case MemoryLayer.Active:
							currentMemoryComp.ActiveMemories.Add(item);
							num++;
							break;
						case MemoryLayer.Situational:
							if (currentMemoryComp.SituationalMemories.Count < RimTalkMemoryPatchMod.Settings.maxSituationalMemories)
							{
								currentMemoryComp.SituationalMemories.Add(item);
								num++;
							}
							break;
						case MemoryLayer.EventLog:
							if (currentMemoryComp.EventLogMemories.Count < RimTalkMemoryPatchMod.Settings.maxEventLogMemories)
							{
								currentMemoryComp.EventLogMemories.Add(item);
								num++;
							}
							break;
						case MemoryLayer.Archive:
							if (currentMemoryComp.ArchiveMemories.Count < RimTalkMemoryPatchMod.Settings.maxArchiveMemories)
							{
								currentMemoryComp.ArchiveMemories.Add(item);
								num++;
							}
							break;
						}
					}
					filtersDirty = true;
					Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Memory_ImportSuccess", NamedArgument.op_Implicit(num), NamedArgument.op_Implicit(importedMemories.Count))), MessageTypeDefOf.PositiveEvent, false);
					Log.Message($"[RimTalk] Imported {num}/{importedMemories.Count} memories from: {filePath}");
				}, false, (string)null, (WindowLayer)1));
			}
			catch (Exception ex)
			{
				Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_Memory_ImportFailed", NamedArgument.op_Implicit(ex.Message))), MessageTypeDefOf.RejectInput, false);
				Log.Error($"[RimTalk] Memory import failed: {ex}");
			}
		}

		private void DrawTimeline(Rect rect)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			if (currentMemoryComp == null)
			{
				return;
			}
			Widgets.DrawMenuSection(rect);
			Rect val = GenUI.ContractedBy(rect, 5f);
			List<MemoryEntry> list = cachedMemories;
			float num = cachedTotalHeight;
			Rect val2 = default(Rect);
			((Rect)(ref val2))..ctor(0f, 0f, ((Rect)(ref val)).width - 16f, num);
			HandleDragSelection(val, val2);
			Widgets.BeginScrollView(val, ref timelineScrollPosition, val2, true);
			float item = timelineScrollPosition.y - 200f;
			float num2 = timelineScrollPosition.y + ((Rect)(ref val)).height + 200f;
			int num3 = 0;
			if (cachedCardYPositions.Count > 0)
			{
				int num4 = cachedCardYPositions.BinarySearch(item);
				num3 = ((num4 < 0) ? Mathf.Max(0, ~num4 - 1) : num4);
			}
			Rect rect2 = default(Rect);
			for (int i = num3; i < list.Count; i++)
			{
				float num5 = cachedCardYPositions[i];
				if (num5 > num2)
				{
					break;
				}
				MemoryEntry memory = list[i];
				float num6 = cachedCardHeights[i];
				((Rect)(ref rect2))..ctor(0f, num5, ((Rect)(ref val2)).width, num6);
				DrawMemoryCard(rect2, memory);
			}
			if (isDragging)
			{
				DrawSelectionBox();
			}
			Widgets.EndScrollView();
			if (filterType.HasValue || !showABM || !showSCM || !showELS || !showCLPA)
			{
				Rect val3 = new Rect(((Rect)(ref val)).x, ((Rect)(ref val)).yMax - 25f, ((Rect)(ref val)).width, 20f);
				Text.Font = (GameFont)0;
				GUI.color = new Color(0.7f, 0.7f, 0.7f);
				Widgets.Label(val3, TranslatorFormattedStringExtensions.Translate("RimTalk_MindStream_ShowingN", NamedArgument.op_Implicit(list.Count)));
				GUI.color = Color.white;
				Text.Font = (GameFont)1;
			}
		}

		private void CheckAndRefreshCache()
		{
			if (currentMemoryComp != null)
			{
				int num = currentMemoryComp.ActiveMemories.Count + currentMemoryComp.SituationalMemories.Count + currentMemoryComp.EventLogMemories.Count + currentMemoryComp.ArchiveMemories.Count;
				int ticksGame = Find.TickManager.TicksGame;
				bool flag = false;
				if (selectedPawn != lastSelectedPawn)
				{
					flag = true;
				}
				else if (num != lastMemoryCount)
				{
					flag = true;
				}
				else if (showABM != lastShowABM)
				{
					flag = true;
				}
				else if (showSCM != lastShowSCM)
				{
					flag = true;
				}
				else if (showELS != lastShowELS)
				{
					flag = true;
				}
				else if (showCLPA != lastShowCLPA)
				{
					flag = true;
				}
				else if (filterType != lastFilterType)
				{
					flag = true;
				}
				else if (ticksGame - lastRefreshTick > 60)
				{
					flag = true;
				}
				if (flag)
				{
					RefreshCache(num, ticksGame);
				}
			}
		}

		private void RefreshCache(int currentCount, int currentTick)
		{
			lastSelectedPawn = selectedPawn;
			lastMemoryCount = currentCount;
			lastShowABM = showABM;
			lastShowSCM = showSCM;
			lastShowELS = showELS;
			lastShowCLPA = showCLPA;
			lastFilterType = filterType;
			lastRefreshTick = currentTick;
			cachedMemories = GetFilteredMemories();
			cachedCardHeights.Clear();
			cachedCardYPositions.Clear();
			cachedTotalHeight = 0f;
			foreach (MemoryEntry cachedMemory in cachedMemories)
			{
				cachedCardYPositions.Add(cachedTotalHeight);
				float cardHeight = GetCardHeight(cachedMemory.Layer);
				cachedCardHeights.Add(cardHeight);
				cachedTotalHeight += cardHeight + 8f;
			}
		}

		private void DrawMemoryCard(Rect rect, MemoryEntry memory)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_0193: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_0251: Unknown result type (might be due to invalid IL or missing references)
			//IL_026c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0260: Unknown result type (might be due to invalid IL or missing references)
			//IL_0271: Unknown result type (might be due to invalid IL or missing references)
			//IL_0290: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0299: Unknown result type (might be due to invalid IL or missing references)
			//IL_02df: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_047d: Unknown result type (might be due to invalid IL or missing references)
			//IL_04e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_04e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0500: Unknown result type (might be due to invalid IL or missing references)
			//IL_051f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0524: Unknown result type (might be due to invalid IL or missing references)
			//IL_0529: Unknown result type (might be due to invalid IL or missing references)
			//IL_054c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0551: Unknown result type (might be due to invalid IL or missing references)
			//IL_0568: Unknown result type (might be due to invalid IL or missing references)
			//IL_0583: Unknown result type (might be due to invalid IL or missing references)
			//IL_0588: Unknown result type (might be due to invalid IL or missing references)
			//IL_058d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0495: Unknown result type (might be due to invalid IL or missing references)
			//IL_049e: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a6: Unknown result type (might be due to invalid IL or missing references)
			bool num = selectedMemories.Contains(memory);
			Color layerColor = GetLayerColor(memory.Layer);
			if (memory.IsPinned)
			{
				Widgets.DrawBoxSolid(rect, new Color(0.25f, 0.2f, 0.1f, 0.5f));
			}
			else
			{
				Widgets.DrawBoxSolid(rect, new Color(0.15f, 0.15f, 0.15f, 0.9f));
			}
			if (num)
			{
				Widgets.DrawBox(rect, 2, (Texture2D)null);
				Rect val = GenUI.ContractedBy(rect, 1f);
				GUI.color = new Color(1f, 0.8f, 0.3f);
				Widgets.DrawBox(val, 2, (Texture2D)null);
				GUI.color = Color.white;
			}
			else
			{
				GUI.color = layerColor;
				Widgets.DrawBox(rect, 1, (Texture2D)null);
				GUI.color = Color.white;
			}
			FourLayerMemoryComp fourLayerMemoryComp = currentMemoryComp;
			if (fourLayerMemoryComp != null && fourLayerMemoryComp.Summarizer.CheckSummarizing(memory))
			{
				float num2 = (Mathf.Sin(Time.realtimeSinceStartup * 4f) + 1f) * 0.5f;
				GUI.color = Color.Lerp(new Color(0.48f, 0.2f, 0.68f, 0.65f), new Color(0.86f, 0.5f, 1f), num2);
				Widgets.DrawBox(GenUI.ContractedBy(rect, 4f), 2, (Texture2D)null);
				GUI.color = Color.white;
			}
			else
			{
				FourLayerMemoryComp fourLayerMemoryComp2 = currentMemoryComp;
				if (fourLayerMemoryComp2 != null && fourLayerMemoryComp2.Summarizer.CheckSummarized(memory))
				{
					GUI.color = new Color(0.68f, 0.4f, 0.88f, 0.55f);
					Widgets.DrawBox(GenUI.ContractedBy(rect, 4f), 1, (Texture2D)null);
					GUI.color = Color.white;
				}
			}
			if (Mouse.IsOver(rect) && !isDragging)
			{
				Widgets.DrawLightHighlight(rect);
			}
			Rect val2 = GenUI.ContractedBy(rect, 8f);
			float num3 = 24f;
			float num4 = 4f;
			float num5 = ((Rect)(ref val2)).xMax - num3;
			float y = ((Rect)(ref val2)).y;
			Rect val3 = default(Rect);
			((Rect)(ref val3))..ctor(num5, y, num3, num3);
			if (Mouse.IsOver(val3))
			{
				Widgets.DrawHighlight(val3);
			}
			if (Widgets.ButtonImage(val3, memory.IsPinned ? TexButton.ReorderUp : TexButton.ReorderDown, true, (string)null))
			{
				bool isPinned = !memory.IsPinned;
				if (currentMemoryComp != null)
				{
					currentMemoryComp.Maintainer.PinMemory(memory, isPinned);
					InvalidateCache();
				}
				Event.current.Use();
			}
			TooltipHandler.TipRegion(val3, TipSignal.op_Implicit(memory.IsPinned ? Translator.Translate("RimTalk_MindStream_Unpin") : Translator.Translate("RimTalk_MindStream_Pin")));
			num5 -= num3 + num4;
			Rect val4 = default(Rect);
			((Rect)(ref val4))..ctor(num5, y, num3, num3);
			if (Mouse.IsOver(val4))
			{
				Widgets.DrawHighlight(val4);
			}
			if (Widgets.ButtonImage(val4, TexButton.Rename, true, (string)null))
			{
				if (currentMemoryComp != null)
				{
					Find.WindowStack.Add((Window)(object)new Dialog_EditMemory(memory, currentMemoryComp));
					filtersDirty = true;
				}
				Event.current.Use();
			}
			TooltipHandler.TipRegion(val4, TipSignal.op_Implicit(Translator.Translate("RimTalk_MindStream_Edit")));
			Rect val5 = default(Rect);
			((Rect)(ref val5))..ctor(((Rect)(ref val2)).x, ((Rect)(ref val2)).y, ((Rect)(ref val2)).width - (num3 * 2f + num4 + 8f), ((Rect)(ref val2)).height);
			Text.Font = (GameFont)0;
			string layerLabel = GetLayerLabel(memory.Layer);
			string text = memory.Type.ToString();
			string ageString = memory.AgeString;
			string text2 = "[" + layerLabel + "] " + text + " ? " + ageString;
			if (!string.IsNullOrEmpty(memory.relatedPawnName))
			{
				text2 += string.Format(" ? {0} {1}", Translator.Translate("RimTalk_MindStream_With"), memory.relatedPawnName);
			}
			GUI.color = new Color(0.8f, 0.8f, 0.8f);
			Widgets.Label(new Rect(((Rect)(ref val5)).x, ((Rect)(ref val5)).y, ((Rect)(ref val5)).width, 18f), text2);
			GUI.color = Color.white;
			Text.Font = (GameFont)1;
			float num6 = ((Rect)(ref val5)).y + 20f;
			float num7 = ((Rect)(ref val5)).height - 40f;
			Rect val6 = default(Rect);
			((Rect)(ref val6))..ctor(((Rect)(ref val5)).x, num6, ((Rect)(ref val5)).width, num7);
			string text3 = memory.Content;
			int contentMaxLength = GetContentMaxLength(memory.Layer);
			if (text3.Length > contentMaxLength)
			{
				text3 = text3.Substring(0, contentMaxLength) + "...";
			}
			Widgets.Label(val6, text3);
			if (memory.Content.Length > contentMaxLength && Mouse.IsOver(val6))
			{
				TooltipHandler.TipRegion(val6, TipSignal.op_Implicit(memory.Content));
			}
			float num8 = ((Rect)(ref val5)).yMax - 12f;
			float num9 = (((Rect)(ref val5)).width - 4f) / 2f;
			Rect val7 = new Rect(((Rect)(ref val5)).x, num8, num9, 8f);
			Widgets.FillableBar(val7, Mathf.Clamp01(memory.Importance), Texture2D.whiteTexture, BaseContent.ClearTex, false);
			TooltipHandler.TipRegion(val7, TipSignal.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_MindStream_ImportanceLabel", NamedArgument.op_Implicit(memory.Importance.ToString("F2")))));
			Rect val8 = new Rect(((Rect)(ref val5)).x + num9 + 4f, num8, num9, 8f);
			Widgets.FillableBar(val8, Mathf.Clamp01(memory.Activity), Texture2D.whiteTexture, BaseContent.ClearTex, false);
			TooltipHandler.TipRegion(val8, TipSignal.op_Implicit(TranslatorFormattedStringExtensions.Translate("RimTalk_MindStream_ActivityLabel", NamedArgument.op_Implicit(memory.Activity.ToString("F2")))));
			Text.Font = (GameFont)1;
		}

		private void HandleMemoryClick(MemoryEntry memory)
		{
			bool control = Event.current.control;
			bool shift = Event.current.shift;
			if (control)
			{
				if (selectedMemories.Contains(memory))
				{
					selectedMemories.Remove(memory);
				}
				else
				{
					selectedMemories.Add(memory);
				}
				lastSelectedMemory = memory;
			}
			else if (shift && lastSelectedMemory != null)
			{
				List<MemoryEntry> filteredMemories = GetFilteredMemories();
				int num = filteredMemories.IndexOf(lastSelectedMemory);
				int num2 = filteredMemories.IndexOf(memory);
				if (num >= 0 && num2 >= 0)
				{
					int num3 = Math.Min(num, num2);
					int num4 = Math.Max(num, num2);
					for (int i = num3; i <= num4; i++)
					{
						selectedMemories.Add(filteredMemories[i]);
					}
				}
				lastSelectedMemory = memory;
			}
			else
			{
				selectedMemories.Clear();
				selectedMemories.Add(memory);
				lastSelectedMemory = memory;
			}
		}

		private void HandleDragSelection(Rect listRect, Rect viewRect)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Invalid comparison between Unknown and I4
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			Event current = Event.current;
			if ((int)current.type == 0 && current.button == 0 && ((Rect)(ref listRect)).Contains(current.mousePosition))
			{
				isMouseDown = true;
				mouseDownScreenPos = current.mousePosition;
				dragStartPos = current.mousePosition - ((Rect)(ref listRect)).position;
				dragCurrentPos = dragStartPos;
			}
			if (!isMouseDown || (int)current.type != 3 || current.button != 0)
			{
				return;
			}
			float num = Vector2.Distance(mouseDownScreenPos, current.mousePosition);
			if (!isDragging && num >= 5f)
			{
				isDragging = true;
			}
			if (!isDragging)
			{
				return;
			}
			dragCurrentPos = current.mousePosition - ((Rect)(ref listRect)).position;
			Rect selectionBox = GetSelectionBox();
			Rect val = default(Rect);
			((Rect)(ref val))..ctor(((Rect)(ref selectionBox)).x, ((Rect)(ref selectionBox)).y + timelineScrollPosition.y, ((Rect)(ref selectionBox)).width, ((Rect)(ref selectionBox)).height);
			List<MemoryEntry> filteredMemories = GetFilteredMemories();
			if (!Event.current.control)
			{
				selectedMemories.Clear();
			}
			float num2 = 0f;
			Rect val2 = default(Rect);
			foreach (MemoryEntry item in filteredMemories)
			{
				float cardHeight = GetCardHeight(item.Layer);
				((Rect)(ref val2))..ctor(0f, num2, ((Rect)(ref viewRect)).width, cardHeight);
				if (((Rect)(ref val)).Overlaps(val2))
				{
					selectedMemories.Add(item);
				}
				num2 += cardHeight + 8f;
			}
			current.Use();
		}

		private void DrawSelectionBox()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			Rect selectionBox = GetSelectionBox();
			Widgets.DrawBox(selectionBox, 1, (Texture2D)null);
			Widgets.DrawBoxSolid(selectionBox, new Color(1f, 0.8f, 0.3f, 0.2f));
		}

		private Rect GetSelectionBox()
		{
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			float num = Mathf.Min(dragStartPos.x, dragCurrentPos.x);
			float num2 = Mathf.Min(dragStartPos.y, dragCurrentPos.y);
			float num3 = Mathf.Max(dragStartPos.x, dragCurrentPos.x);
			float num4 = Mathf.Max(dragStartPos.y, dragCurrentPos.y);
			return new Rect(num, num2, num3 - num, num4 - num2);
		}

		private void DrawTopBar(Rect rect)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			Widgets.DrawMenuSection(rect);
			Rect val = GenUI.ContractedBy(rect, 5f);
			Rect rect2 = default(Rect);
			((Rect)(ref rect2))..ctor(((Rect)(ref val)).x, ((Rect)(ref val)).y + 5f, 250f, 35f);
			DrawPawnSelector(rect2);
			Widgets.CheckboxLabeled(new Rect(((Rect)(ref val)).x + 260f, ((Rect)(ref val)).y + 10f, 180f, 25f), TaggedString.op_Implicit(Translator.Translate("RimTalk_ShowAllHumanlikes")), ref showAllHumanlikes, false, (Texture2D)null, (Texture2D)null, false, false);
			if (currentMemoryComp != null)
			{
				Rect rect3 = default(Rect);
				((Rect)(ref rect3))..ctor(((Rect)(ref val)).x + 450f, ((Rect)(ref val)).y + 8f, 350f, 30f);
				DrawTopBarStats(rect3);
			}
			float num = 120f;
			float num2 = 5f;
			float num3 = ((Rect)(ref val)).xMax - num;
			if (Widgets.ButtonText(new Rect(num3, ((Rect)(ref val)).y + 5f, num, 35f), TaggedString.op_Implicit(Translator.Translate("RimTalk_MindStream_Preview")), true, true, true, (TextAnchor?)null))
			{
				Find.WindowStack.Add((Window)(object)new Dialog_InjectionPreview());
			}
			float num4 = num3 - (num + num2);
			if (Widgets.ButtonText(new Rect(num4, ((Rect)(ref val)).y + 5f, num, 35f), TaggedString.op_Implicit(Translator.Translate("RimTalk_MindStream_SummaryPrompt")), true, true, true, (TextAnchor?)null))
			{
				Find.WindowStack.Add((Window)(object)new Dialog_PromptEditor());
			}
			float num5 = num4 - (num + num2);
			if (Widgets.ButtonText(new Rect(num5, ((Rect)(ref val)).y + 5f, num, 35f), TaggedString.op_Implicit(Translator.Translate("RimTalk_MindStream_Knowledge")), true, true, true, (TextAnchor?)null))
			{
				OpenCommonKnowledgeDialog();
			}
			if (Widgets.ButtonText(new Rect(num5 - (num + num2), ((Rect)(ref val)).y + 5f, num, 35f), TaggedString.op_Implicit(Translator.Translate("RimTalk_MindStream_OperationGuide")), true, true, true, (TextAnchor?)null))
			{
				ShowOperationGuide();
			}
		}

		private void DrawTopBarStats(Rect rect)
		{
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			if (currentMemoryComp != null)
			{
				int count = currentMemoryComp.ActiveMemories.Count;
				int count2 = currentMemoryComp.SituationalMemories.Count;
				int count3 = currentMemoryComp.EventLogMemories.Count;
				int count4 = currentMemoryComp.ArchiveMemories.Count;
				Text.Font = (GameFont)1;
				string text = $"ABM: {count}  SCM: {count2}  ELS: {count3}  CLPA: {count4}";
				GUI.color = new Color(0.7f, 0.9f, 1f);
				Widgets.Label(new Rect(((Rect)(ref rect)).x, ((Rect)(ref rect)).y, ((Rect)(ref rect)).width, ((Rect)(ref rect)).height), text);
				GUI.color = Color.white;
				Text.Font = (GameFont)1;
			}
		}

		private void DrawPawnSelector(Rect rect)
		{
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e7: Expected O, but got Unknown
			//IL_020a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0214: Expected O, but got Unknown
			List<Pawn> list;
			if (showAllHumanlikes)
			{
				Map currentMap = Find.CurrentMap;
				object obj;
				if (currentMap == null)
				{
					obj = null;
				}
				else
				{
					MapPawns mapPawns = currentMap.mapPawns;
					obj = ((mapPawns == null) ? null : mapPawns.AllPawnsSpawned?.Where((Pawn val) => val.RaceProps.Humanlike)?.ToList());
				}
				list = (List<Pawn>)obj;
			}
			else
			{
				Map currentMap2 = Find.CurrentMap;
				object obj2;
				if (currentMap2 == null)
				{
					obj2 = null;
				}
				else
				{
					MapPawns mapPawns2 = currentMap2.mapPawns;
					obj2 = ((mapPawns2 == null) ? null : mapPawns2.FreeColonists?.ToList());
				}
				list = (List<Pawn>)obj2;
			}
			if (list == null || list.Count == 0)
			{
				Widgets.Label(rect, Translator.Translate("RimTalk_MindStream_NoColonists"));
				return;
			}
			string text = ((selectedPawn != null) ? ((Entity)selectedPawn).LabelShort : ((object)Translator.Translate("RimTalk_SelectColonist")/*cast due to constrained. prefix*/).ToString());
			if (Widgets.ButtonText(rect, text, true, true, true, (TextAnchor?)null))
			{
				List<FloatMenuOption> list2 = new List<FloatMenuOption>();
				foreach (Pawn item in list)
				{
					Pawn p = item;
					string text2 = ((Entity)p).LabelShort;
					if (!p.IsColonist)
					{
						if (((Thing)p).Faction != null && ((Thing)p).Faction != Faction.OfPlayer)
						{
							text2 = text2 + " (" + ((Thing)p).Faction.Name + ")";
						}
						else if (p.IsPrisoner)
						{
							text2 += " (Prisoner)";
						}
						else if (p.IsSlaveOfColony)
						{
							text2 += " (Slave)";
						}
					}
					list2.Add(new FloatMenuOption(text2, (Action)delegate
					{
						selectedPawn = p;
						selectedMemories.Clear();
						filtersDirty = true;
					}, (MenuOptionPriority)4, (Action<Rect>)null, (Thing)null, 0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0));
				}
				Find.WindowStack.Add((Window)new FloatMenu(list2));
			}
			if (selectedPawn == null && list.Count > 0)
			{
				selectedPawn = list[0];
				filtersDirty = true;
			}
		}

		private List<MemoryEntry> GetFilteredMemories()
		{
			if (filtersDirty || cachedFilteredMemories == null)
			{
				RebuildFilteredMemories();
				filtersDirty = false;
			}
			return cachedFilteredMemories;
		}

		private void RebuildFilteredMemories()
		{
			if (currentMemoryComp == null)
			{
				cachedFilteredMemories = new List<MemoryEntry>();
				return;
			}
			List<MemoryEntry> list = new List<MemoryEntry>();
			if (showABM)
			{
				list.AddRange(currentMemoryComp.ActiveMemories.Where((MemoryEntry m) => !filterType.HasValue || m.Type == filterType.Value));
			}
			if (showSCM)
			{
				list.AddRange(currentMemoryComp.SituationalMemories.Where((MemoryEntry m) => !filterType.HasValue || m.Type == filterType.Value));
			}
			if (showELS)
			{
				list.AddRange(currentMemoryComp.EventLogMemories.Where((MemoryEntry m) => !filterType.HasValue || m.Type == filterType.Value));
			}
			if (showCLPA)
			{
				list.AddRange(currentMemoryComp.ArchiveMemories.Where((MemoryEntry m) => !filterType.HasValue || m.Type == filterType.Value));
			}
			cachedFilteredMemories = list.OrderByDescending((MemoryEntry m) => m.GameTick).ToList();
		}

		private float GetCardHeight(MemoryLayer layer)
		{
			return layer switch
			{
				MemoryLayer.Active => 80f, 
				MemoryLayer.Situational => 100f, 
				MemoryLayer.EventLog => 130f, 
				MemoryLayer.Archive => 160f, 
				_ => 100f, 
			};
		}

		private int GetContentMaxLength(MemoryLayer layer)
		{
			return layer switch
			{
				MemoryLayer.Active => 80, 
				MemoryLayer.Situational => 120, 
				MemoryLayer.EventLog => 200, 
				MemoryLayer.Archive => 300, 
				_ => 120, 
			};
		}

		private Color GetLayerColor(MemoryLayer layer)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			return (Color)(layer switch
			{
				MemoryLayer.Active => new Color(0.3f, 0.8f, 1f), 
				MemoryLayer.Situational => new Color(0.3f, 1f, 0.5f), 
				MemoryLayer.EventLog => new Color(1f, 0.8f, 0.3f), 
				MemoryLayer.Archive => new Color(0.8f, 0.4f, 1f), 
				_ => Color.white, 
			});
		}

		private string GetLayerLabel(MemoryLayer layer)
		{
			return layer switch
			{
				MemoryLayer.Active => "ABM", 
				MemoryLayer.Situational => "SCM", 
				MemoryLayer.EventLog => "ELS", 
				MemoryLayer.Archive => "CLPA", 
				_ => "UNK", 
			};
		}

		private void DrawNoPawnSelected(Rect rect)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			Text.Anchor = (TextAnchor)4;
			Text.Font = (GameFont)2;
			Widgets.Label(rect, Translator.Translate("RimTalk_MindStream_SelectColonist"));
			Text.Anchor = (TextAnchor)0;
			Text.Font = (GameFont)1;
		}

		private void DrawNoMemoryComponent(Rect rect)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			Text.Anchor = (TextAnchor)4;
			Widgets.Label(rect, Translator.Translate("RimTalk_NoMemoryComponent"));
			Text.Anchor = (TextAnchor)0;
		}

		private void OpenCommonKnowledgeDialog()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			if (Current.Game == null)
			{
				Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_MindStream_MustEnterGame")), MessageTypeDefOf.RejectInput, false);
				return;
			}
			MemoryManager component = Find.World.GetComponent<MemoryManager>();
			if (component == null)
			{
				Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_MindStream_CannotFindManager")), MessageTypeDefOf.RejectInput, false);
			}
			else
			{
				Find.WindowStack.Add((Window)(object)new Dialog_CommonKnowledge(component.CommonKnowledge));
			}
		}

		private void ShowOperationGuide()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected O, but got Unknown
			string text = TaggedString.op_Implicit(Translator.Translate("RimTalk_MindStream_GuideContent"));
			Find.WindowStack.Add((Window)new Dialog_MessageBox(TaggedString.op_Implicit(text), TaggedString.op_Implicit(Translator.Translate("RimTalk_Close")), (Action)null, TaggedString.op_Implicit(Translator.Translate("RimTalk_MindStream_OperationGuide")), (Action)null, (string)null, false, (Action)null, (Action)null, (WindowLayer)1));
		}

		public MainTabWindow_Memory()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			selectedMemories = new HashSet<MemoryEntry>();
			dragStartPos = Vector2.zero;
			dragCurrentPos = Vector2.zero;
			mouseDownScreenPos = Vector2.zero;
			timelineScrollPosition = Vector2.zero;
			showABM = true;
			showSCM = true;
			showELS = true;
			showCLPA = true;
			filtersDirty = true;
			cachedMemories = new List<MemoryEntry>();
			cachedCardHeights = new List<float>();
			cachedCardYPositions = new List<float>();
			lastMemoryCount = -1;
			lastRefreshTick = -1;
			((MainTabWindow)this)..ctor();
		}
	}
	public class VirtualListView<T> where T : class
	{
		private List<T> items = new List<T>();

		private Func<T, float> getItemHeight;

		private Action<Rect, T, int> drawItem;

		private Vector2 scrollPosition;

		private readonly List<float> itemOffsets = new List<float>();

		private readonly List<float> itemHeights = new List<float>();

		private float totalHeight;

		public float ItemSpacing { get; set; } = 5f;

		public Vector2 ScrollPosition
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return scrollPosition;
			}
			set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				scrollPosition = value;
			}
		}

		public string EmptyLabel { get; set; } = "No items to display";

		public VirtualListView(Func<T, float> getItemHeight, Action<Rect, T, int> drawItem)
		{
			this.getItemHeight = getItemHeight ?? throw new ArgumentNullException("getItemHeight");
			this.drawItem = drawItem ?? throw new ArgumentNullException("drawItem");
		}

		public void SetItems(List<T> newItems)
		{
			items = newItems ?? new List<T>();
			RebuildLayout();
		}

		private void RebuildLayout()
		{
			itemOffsets.Clear();
			itemHeights.Clear();
			totalHeight = 0f;
			for (int i = 0; i < items.Count; i++)
			{
				itemOffsets.Add(totalHeight);
				float num = getItemHeight(items[i]);
				itemHeights.Add(num);
				totalHeight += num + ItemSpacing;
			}
		}

		public void Draw(Rect rect)
		{
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			if (items.Count == 0)
			{
				Text.Anchor = (TextAnchor)4;
				GUI.color = new Color(0.6f, 0.6f, 0.6f);
				Widgets.Label(rect, EmptyLabel);
				GUI.color = Color.white;
				Text.Anchor = (TextAnchor)0;
				return;
			}
			Rect val = default(Rect);
			((Rect)(ref val))..ctor(0f, 0f, ((Rect)(ref rect)).width - 16f, totalHeight);
			Widgets.BeginScrollView(rect, ref scrollPosition, val, true);
			float y = scrollPosition.y;
			float visibleBottom = y + ((Rect)(ref rect)).height;
			GetVisibleIndexRange(y, visibleBottom, out var startIndex, out var endIndex);
			Rect arg = default(Rect);
			for (int i = startIndex; i <= endIndex; i++)
			{
				float num = itemOffsets[i];
				float num2 = itemHeights[i];
				((Rect)(ref arg))..ctor(0f, num, ((Rect)(ref val)).width, num2);
				drawItem(arg, items[i], i);
			}
			Widgets.EndScrollView();
		}

		public List<int> FindItemsInRange(float minY, float maxY)
		{
			List<int> list = new List<int>();
			if (items.Count == 0)
			{
				return list;
			}
			int num = FindFirstIndexAtOrBefore(minY);
			if (num < 0)
			{
				num = 0;
			}
			for (int i = num; i < items.Count; i++)
			{
				float num2 = itemOffsets[i];
				float num3 = num2 + itemHeights[i];
				if (num2 > maxY)
				{
					break;
				}
				if (num3 >= minY)
				{
					list.Add(i);
				}
			}
			return list;
		}

		public string GetRenderStats(float viewportHeight)
		{
			if (items.Count == 0)
			{
				return "0/0 rendered";
			}
			float y = scrollPosition.y;
			float visibleBottom = y + viewportHeight;
			GetVisibleIndexRange(y, visibleBottom, out var startIndex, out var endIndex);
			int num = 0;
			if (endIndex >= startIndex)
			{
				num = endIndex - startIndex + 1;
			}
			return $"{num}/{items.Count} rendered";
		}

		private void GetVisibleIndexRange(float visibleTop, float visibleBottom, out int startIndex, out int endIndex)
		{
			startIndex = FindFirstIndexAtOrBefore(visibleTop);
			if (startIndex < 0)
			{
				startIndex = 0;
			}
			endIndex = FindLastIndexAtOrAfter(visibleBottom);
			if (endIndex < 0)
			{
				endIndex = items.Count - 1;
			}
			if (startIndex < 0)
			{
				startIndex = 0;
			}
			if (endIndex >= items.Count)
			{
				endIndex = items.Count - 1;
			}
			if (endIndex < startIndex)
			{
				endIndex = startIndex;
			}
		}

		private int FindFirstIndexAtOrBefore(float y)
		{
			int num = 0;
			int num2 = items.Count - 1;
			int num3 = items.Count;
			while (num <= num2)
			{
				int num4 = (num + num2) / 2;
				if (itemOffsets[num4] + itemHeights[num4] >= y)
				{
					num3 = num4;
					num2 = num4 - 1;
				}
				else
				{
					num = num4 + 1;
				}
			}
			if (num3 == items.Count)
			{
				return items.Count - 1;
			}
			return num3;
		}

		private int FindLastIndexAtOrAfter(float y)
		{
			int num = 0;
			int num2 = items.Count - 1;
			int num3 = -1;
			while (num <= num2)
			{
				int num4 = (num + num2) / 2;
				if (itemOffsets[num4] <= y)
				{
					num3 = num4;
					num = num4 + 1;
				}
				else
				{
					num2 = num4 - 1;
				}
			}
			if (num3 < 0)
			{
				return 0;
			}
			return num3;
		}
	}
	public static class SettingsUIDrawers
	{
		private static Dictionary<string, List<(string name, string description, bool isPawnProperty)>> _cachedCategorizedSources;

		private static Vector2 _matchingSourcesScrollPosition;

		public static void DrawRimTalkStyleApiConfigs(Listing_Standard listing, RimTalkMemoryPatchSettings settings)
		{
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_020c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0216: Unknown result type (might be due to invalid IL or missing references)
			if (settings.ApiConfigs == null)
			{
				settings.ApiConfigs = new List<ApiConfig>();
			}
			if (settings.ApiConfigs.Count == 0)
			{
				settings.ApiConfigs.Add(new ApiConfig
				{
					Provider = AIProvider.Player2,
					IsEnabled = true
				});
			}
			Rect rect = ((Listing)listing).GetRect(24f, 1f);
			float num = 24f;
			Rect val = default(Rect);
			((Rect)(ref val))..ctor(((Rect)(ref rect)).xMax - num, ((Rect)(ref rect)).y, num, num);
			((Rect)(ref rect)).width = ((Rect)(ref rect)).width - (num + 5f);
			Widgets.Label(rect, Translator.Translate("RimTalk_Settings_CloudApiConfigurations"));
			Text.Font = (GameFont)0;
			GUI.color = Color.gray;
			Rect rect2 = ((Listing)listing).GetRect(Text.LineHeight * 2f, 1f);
			((Rect)(ref rect2)).width = ((Rect)(ref rect2)).width - 35f;
			Widgets.Label(rect2, Translator.Translate("RimTalk_Settings_CloudApiConfigurationsDesc"));
			GUI.color = Color.white;
			Text.Font = (GameFont)1;
			Color color = GUI.color;
			GUI.color = new Color(0.3f, 0.9f, 0.3f);
			if (Widgets.ButtonText(val, "+", true, true, true, (TextAnchor?)null))
			{
				settings.ApiConfigs.Add(new ApiConfig());
			}
			GUI.color = color;
			((Listing)listing).Gap(6f);
			Rect rect3 = ((Listing)listing).GetRect(20f, 1f);
			float x = ((Rect)(ref rect3)).x;
			float y = ((Rect)(ref rect3)).y;
			float height = ((Rect)(ref rect3)).height;
			float width = ((Rect)(ref rect3)).width;
			Widgets.Label(new Rect(x, y, 90f, height), Translator.Translate("RimTalk_Settings_ProviderHeader"));
			Widgets.Label(new Rect(x + 90f + 5f, y, 200f, height), Translator.Translate("RimTalk_Settings_ApiKeyHeader"));
			Widgets.Label(new Rect(x + width - 100f - 190f - 5f, y, 190f, height), Translator.Translate("RimTalk_Settings_ModelHeader"));
			Widgets.Label(new Rect(x + width - 100f + 5f, y, 100f, height), Translator.Translate("RimTalk_Settings_EnabledHeader"));
			((Listing)listing).Gap(3f);
			for (int i = 0; i < settings.ApiConfigs.Count; i++)
			{
				if (DrawApiConfigRow(listing, settings.ApiConfigs[i], i, settings.ApiConfigs))
				{
					settings.ApiConfigs.RemoveAt(i);
					i--;
				}
				((Listing)listing).Gap(2f);
			}
			Text.Font = (GameFont)1;
		}

		private static bool DrawApiConfigRow(Listing_Standard listing, ApiConfig config, int index, List<ApiConfig> configs)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0239: Unknown result type (might be due to invalid IL or missing references)
			//IL_029c: Unknown result type (might be due to invalid IL or missing references)
			//IL_02af: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0319: Unknown result type (might be due to invalid IL or missing references)
			//IL_032d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0341: Unknown result type (might be due to invalid IL or missing references)
			//IL_0346: Unknown result type (might be due to invalid IL or missing references)
			//IL_0350: Unknown result type (might be due to invalid IL or missing references)
			Text.Font = (GameFont)0;
			Rect rect = ((Listing)listing).GetRect(22f, 1f);
			float x = ((Rect)(ref rect)).x;
			float y = ((Rect)(ref rect)).y;
			float height = ((Rect)(ref rect)).height;
			float width = ((Rect)(ref rect)).width;
			float num = width - 90f - 190f - 100f - 15f;
			float num2 = x + 90f + 5f;
			Color color = GUI.color;
			if (!config.IsEnabled)
			{
				GUI.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
			}
			DrawProviderDropdown(x, y, height, 90f, config);
			if (config.Provider == AIProvider.Custom)
			{
				float num3 = num * 0.4f - 2.5f;
				float width2 = num * 0.6f - 2.5f;
				DrawApiKeyInput(num2, y, height, num3, config);
				DrawBaseUrlInput(num2 + num3 + 5f, y, height, width2, config);
			}
			else
			{
				DrawApiKeyInput(num2, y, height, num, config);
			}
			float x2 = num2 + num + 5f;
			if (config.Provider == AIProvider.Player2)
			{
				DrawPlayer2AuthButton(x2, y, height, 190f, config);
			}
			else
			{
				DrawModelTextInput(x2, y, height, 190f, config);
			}
			GUI.color = color;
			float num4 = 22f;
			float num5 = 2f;
			float num6 = x + width - num4;
			float num7 = num6 - num5 - num4 - num5 - num4;
			float num8 = x + width - 100f;
			float num9 = num7 - num8;
			float num10 = num8 + (num9 - 24f) / 2f;
			Rect val = default(Rect);
			((Rect)(ref val))..ctor(num10, y, 24f, height);
			Widgets.Checkbox(new Vector2(((Rect)(ref val)).x, ((Rect)(ref val)).y), ref config.IsEnabled, 20f, false, false, (Texture2D)null, (Texture2D)null);
			if (Mouse.IsOver(val))
			{
				TooltipHandler.TipRegion(val, TipSignal.op_Implicit(Translator.Translate("RimTalk_Settings_EnableDisableTooltip")));
			}
			if (Widgets.ButtonText(new Rect(num7, y, num4, height), "▲", true, true, true, (TextAnchor?)null) && index > 0)
			{
				List<ApiConfig> list = configs;
				int index2 = index;
				int index3 = index - 1;
				ApiConfig value = configs[index - 1];
				ApiConfig value2 = configs[index];
				list[index2] = value;
				configs[index3] = value2;
			}
			if (Widgets.ButtonText(new Rect(num7 + num4 + num5, y, num4, height), "▼", true, true, true, (TextAnchor?)null) && index < configs.Count - 1)
			{
				List<ApiConfig> list = configs;
				int index3 = index;
				int index2 = index + 1;
				ApiConfig value2 = configs[index + 1];
				ApiConfig value = configs[index];
				list[index3] = value2;
				configs[index2] = value;
			}
			Rect val2 = new Rect(num6, y, num4, height);
			bool result = false;
			bool flag = configs.Count > 1;
			Color color2 = GUI.color;
			GUI.color = (Color)(flag ? new Color(1f, 0.4f, 0.4f) : Color.gray);
			if (Widgets.ButtonText(val2, "×", true, true, flag, (TextAnchor?)null))
			{
				result = true;
			}
			GUI.color = color2;
			if (config.IsEnabled && !config.IsValid)
			{
				Rect rect2 = ((Listing)listing).GetRect(Text.LineHeight, 1f);
				GUI.color = new Color(1f, 0.45f, 0.35f);
				Widgets.Label(rect2, "  " + Translator.Translate("RimTalk_Settings_ApiConfigInvalid"));
				GUI.color = Color.white;
			}
			Text.Font = (GameFont)0;
			return result;
		}

		private static void DrawProviderDropdown(float x, float y, float height, float width, ApiConfig config)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Expected O, but got Unknown
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Expected O, but got Unknown
			if (!Widgets.ButtonText(new Rect(x, y, width, height), config.Provider.ToString(), true, true, true, (TextAnchor?)null))
			{
				return;
			}
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (AIProvider value in Enum.GetValues(typeof(AIProvider)))
			{
				AIProvider captured = value;
				list.Add(new FloatMenuOption(captured.ToString(), (Action)delegate
				{
					config.Provider = captured;
					config.CustomUrl = "";
					config.CustomModelName = "";
				}, (MenuOptionPriority)4, (Action<Rect>)null, (Thing)null, 0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0));
			}
			Find.WindowStack.Add((Window)new FloatMenu(list));
		}

		private static void DrawApiKeyInput(float x, float y, float height, float width, ApiConfig config)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			Rect rect = default(Rect);
			((Rect)(ref rect))..ctor(x, y, width, height);
			string placeholder = TaggedString.op_Implicit((config.Provider == AIProvider.Player2) ? Translator.Translate("RimTalk_Settings_PlaceholderPlayer2Key") : Translator.Translate("RimTalk_Settings_PlaceholderApiKey"));
			config.ApiKey = DrawTextFieldWithPlaceholder(rect, config.ApiKey, placeholder);
		}

		private static void DrawPlayer2AuthButton(float x, float y, float height, float width, ApiConfig config)
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_028f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0295: Unknown result type (might be due to invalid IL or missing references)
			//IL_029a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Expected O, but got Unknown
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ee: Expected O, but got Unknown
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Expected O, but got Unknown
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_0229: Unknown result type (might be due to invalid IL or missing references)
			//IL_0233: Expected O, but got Unknown
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Expected O, but got Unknown
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Expected O, but got Unknown
			//IL_0285: Unknown result type (might be due to invalid IL or missing references)
			//IL_028f: Expected O, but got Unknown
			//IL_023f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0274: Unknown result type (might be due to invalid IL or missing references)
			//IL_027e: Expected O, but got Unknown
			Rect val = default(Rect);
			((Rect)(ref val))..ctor(x, y, width, height);
			bool isAuthenticating = Player2AuthHelper.IsAuthenticating;
			string text = TaggedString.op_Implicit(isAuthenticating ? Translator.Translate("RimTalk_Settings_Player2AuthWaiting") : Translator.Translate("RimTalk_Settings_Player2GetWebKey"));
			Color color = GUI.color;
			if (isAuthenticating)
			{
				GUI.color = new Color(0.95f, 0.85f, 0.45f);
			}
			if (Widgets.ButtonText(val, text, true, true, true, (TextAnchor?)null))
			{
				if (isAuthenticating)
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>(3)
					{
						new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_Player2AuthReopen")), (Action)delegate
						{
							Player2AuthHelper.ReopenApprovalPage();
						}, (MenuOptionPriority)4, (Action<Rect>)null, (Thing)null, 0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0),
						new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_Player2AuthCancel")), (Action)delegate
						{
							Player2AuthHelper.Cancel();
						}, (MenuOptionPriority)4, (Action<Rect>)null, (Thing)null, 0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0),
						new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_Player2OpenWebsite")), (Action)delegate
						{
							Player2AuthHelper.OpenWebsite();
						}, (MenuOptionPriority)4, (Action<Rect>)null, (Thing)null, 0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0)
					};
					Find.WindowStack.Add((Window)new FloatMenu(list));
				}
				else
				{
					Player2AuthHelper.StartAuth(delegate(string key)
					{
						config.ApiKey = key;
						RimTalkSettings val2 = Settings.Get();
						if (val2 != null)
						{
							val2.SimplePlayer2ApiKey = key;
							((ModSettings)val2).Write();
						}
						((ModSettings)RimTalkMemoryPatchMod.Settings).Write();
					});
				}
			}
			GUI.color = color;
			if ((int)Event.current.type == 0 && Event.current.button == 1 && Mouse.IsOver(val))
			{
				Event.current.Use();
				List<FloatMenuOption> list2 = new List<FloatMenuOption>(2)
				{
					new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_Player2GetWebKey")), (Action)delegate
					{
						Player2AuthHelper.StartAuth(delegate(string key)
						{
							config.ApiKey = key;
							RimTalkSettings val2 = Settings.Get();
							if (val2 != null)
							{
								val2.SimplePlayer2ApiKey = key;
								((ModSettings)val2).Write();
							}
							((ModSettings)RimTalkMemoryPatchMod.Settings).Write();
						});
					}, (MenuOptionPriority)4, (Action<Rect>)null, (Thing)null, 0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0),
					new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_Player2OpenWebsite")), (Action)delegate
					{
						Player2AuthHelper.OpenWebsite();
					}, (MenuOptionPriority)4, (Action<Rect>)null, (Thing)null, 0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0)
				};
				if (isAuthenticating)
				{
					list2.Add(new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_Player2AuthCancel")), (Action)delegate
					{
						Player2AuthHelper.Cancel();
					}, (MenuOptionPriority)4, (Action<Rect>)null, (Thing)null, 0f, (Func<Rect, bool>)null, (WorldObject)null, true, 0));
				}
				Find.WindowStack.Add((Window)new FloatMenu(list2));
			}
			TooltipHandler.TipRegion(val, TipSignal.op_Implicit(Translator.Translate("RimTalk_Settings_Player2WebDesc")));
		}

		private static void DrawBaseUrlInput(float x, float y, float height, float width, ApiConfig config)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			Rect val = default(Rect);
			((Rect)(ref val))..ctor(x, y, width, height);
			config.CustomUrl = DrawTextFieldWithPlaceholder(val, config.CustomUrl, "https://...");
			if (Mouse.IsOver(val))
			{
				TooltipHandler.TipRegion(val, TipSignal.op_Implicit(Translator.Translate("RimTalk_Settings_BaseUrlTooltip")));
			}
		}

		private static void DrawModelTextInput(float x, float y, float height, float width, ApiConfig config)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			Rect rect = default(Rect);
			((Rect)(ref rect))..ctor(x, y, width, height);
			config.CustomModelName = DrawTextFieldWithPlaceholder(rect, config.CustomModelName, TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_PlaceholderModelId")));
		}

		private static string DrawTextFieldWithPlaceholder(Rect rect, string text, string placeholder)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			string text2 = Widgets.TextField(rect, text ?? string.Empty);
			if (string.IsNullOrEmpty(text2))
			{
				TextAnchor anchor = Text.Anchor;
				Color color = GUI.color;
				Text.Anchor = (TextAnchor)3;
				GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.7f);
				Widgets.Label(new Rect(((Rect)(ref rect)).x + 5f, ((Rect)(ref rect)).y, ((Rect)(ref rect)).width - 5f, ((Rect)(ref rect)).height), placeholder);
				GUI.color = color;
				Text.Anchor = anchor;
			}
			return text2;
		}

		public static void DrawKnowledgeMatchingSourcesSettings(Listing_Standard listing, RimTalkMemoryPatchSettings settings)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_019f: Unknown result type (might be due to invalid IL or missing references)
			//IL_026e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0273: Unknown result type (might be due to invalid IL or missing references)
			//IL_0288: Unknown result type (might be due to invalid IL or missing references)
			//IL_0297: Unknown result type (might be due to invalid IL or missing references)
			//IL_029c: Unknown result type (might be due to invalid IL or missing references)
			//IL_02be: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0309: Unknown result type (might be due to invalid IL or missing references)
			//IL_0322: Unknown result type (might be due to invalid IL or missing references)
			//IL_033b: Unknown result type (might be due to invalid IL or missing references)
			//IL_035f: Unknown result type (might be due to invalid IL or missing references)
			//IL_038b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0746: Unknown result type (might be due to invalid IL or missing references)
			//IL_0761: Unknown result type (might be due to invalid IL or missing references)
			//IL_0768: Unknown result type (might be due to invalid IL or missing references)
			//IL_076f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0774: Unknown result type (might be due to invalid IL or missing references)
			//IL_077f: Unknown result type (might be due to invalid IL or missing references)
			//IL_07d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_07ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0805: Unknown result type (might be due to invalid IL or missing references)
			//IL_040c: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_0402: Unknown result type (might be due to invalid IL or missing references)
			//IL_0429: Unknown result type (might be due to invalid IL or missing references)
			//IL_0415: Unknown result type (might be due to invalid IL or missing references)
			//IL_046c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0482: Unknown result type (might be due to invalid IL or missing references)
			//IL_049b: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_04df: Unknown result type (might be due to invalid IL or missing references)
			//IL_051c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0506: Unknown result type (might be due to invalid IL or missing references)
			//IL_053a: Unknown result type (might be due to invalid IL or missing references)
			//IL_054b: Unknown result type (might be due to invalid IL or missing references)
			//IL_055b: Unknown result type (might be due to invalid IL or missing references)
			//IL_05b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_05cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_060e: Unknown result type (might be due to invalid IL or missing references)
			//IL_061a: Unknown result type (might be due to invalid IL or missing references)
			//IL_061f: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_05fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_062b: Unknown result type (might be due to invalid IL or missing references)
			//IL_062f: Unknown result type (might be due to invalid IL or missing references)
			//IL_06a0: Unknown result type (might be due to invalid IL or missing references)
			GUI.color = new Color(0.8f, 1f, 0.8f);
			listing.Label(Translator.Translate("RimTalk_Settings_KnowledgeMatchingSources"), -1f, (string)null);
			GUI.color = Color.white;
			GUI.color = Color.gray;
			listing.Label("  " + Translator.Translate("RimTalk_Settings_KnowledgeMatchingSourcesDesc"), -1f, (string)null);
			GUI.color = Color.white;
			((Listing)listing).Gap(12f);
			Rect rect = ((Listing)listing).GetRect(25f, 1f);
			if (Widgets.ButtonText(new Rect(((Rect)(ref rect)).x, ((Rect)(ref rect)).y, 120f, 25f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_RefreshList")), true, true, true, (TextAnchor?)null))
			{
				MustacheVariableHelper.ClearCache();
				_cachedCategorizedSources = null;
				Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_ListRefreshed")), MessageTypeDefOf.NeutralEvent, true);
			}
			Dictionary<string, List<(string name, string description, bool isPawnProperty)>> categorizedSources = GetCachedCategorizedSources();
			int num = categorizedSources.Sum((KeyValuePair<string, List<(string name, string description, bool isPawnProperty)>> c) => c.Value.Count);
			int num2 = categorizedSources.Sum((KeyValuePair<string, List<(string name, string description, bool isPawnProperty)>> c) => GenCollection.Count<(string, string, bool)>(c.Value, (Predicate<(string, string, bool)>)(((string name, string description, bool isPawnProperty) v) => v.isPawnProperty)));
			Rect val = new Rect(((Rect)(ref rect)).x + 130f, ((Rect)(ref rect)).y, 250f, 25f);
			GUI.color = Color.gray;
			Widgets.Label(val, TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_VariableCount", NamedArgument.op_Implicit(num), NamedArgument.op_Implicit(num2)));
			GUI.color = Color.white;
			((Listing)listing).Gap(12f);
			if (settings.knowledgeMatchingSources == null || settings.knowledgeMatchingSources.Count == 0)
			{
				settings.knowledgeMatchingSources = new List<string> { "dialogue", "context" };
			}
			float num3 = 22f;
			float num4 = 26f;
			float num5 = 0f;
			foreach (KeyValuePair<string, List<(string, string, bool)>> item in categorizedSources)
			{
				num5 += num4;
				num5 += (float)item.Value.Count * num3 + 2f;
			}
			float num6 = Mathf.Min(num5 + 10f, 320f);
			Rect rect2 = ((Listing)listing).GetRect(num6, 1f);
			Widgets.DrawBoxSolid(rect2, new Color(0.12f, 0.12f, 0.12f, 0.5f));
			Rect val2 = GenUI.ContractedBy(rect2, 5f);
			Rect val3 = default(Rect);
			((Rect)(ref val3))..ctor(0f, 0f, ((Rect)(ref val2)).width - 16f, num5);
			Widgets.BeginScrollView(val2, ref _matchingSourcesScrollPosition, val3, true);
			float num7 = 0f;
			float width = ((Rect)(ref val3)).width;
			Rect val4 = default(Rect);
			Rect val5 = default(Rect);
			foreach (KeyValuePair<string, List<(string, string, bool)>> item2 in categorizedSources)
			{
				Widgets.DrawBoxSolid(new Rect(0f, num7, width, num4 - 2f), new Color(0.2f, 0.25f, 0.3f, 0.8f));
				GUI.color = new Color(0.9f, 0.95f, 1f);
				Widgets.Label(new Rect(8f, num7 + 3f, width - 16f, 20f), $"{item2.Key} ({item2.Value.Count})");
				GUI.color = Color.white;
				num7 += num4;
				foreach (var item3 in item2.Value)
				{
					bool flag = settings.knowledgeMatchingSources.Contains(item3.Item1);
					bool flag2 = flag;
					((Rect)(ref val4))..ctor(0f, num7, width, num3 - 2f);
					if (flag)
					{
						Widgets.DrawBoxSolid(val4, new Color(0.2f, 0.4f, 0.2f, 0.3f));
					}
					if (Mouse.IsOver(val4))
					{
						Widgets.DrawHighlight(val4);
					}
					Widgets.Checkbox(new Vector2(8f, num7 + 1f), ref flag2, 24f, false, false, (Texture2D)null, (Texture2D)null);
					float num8 = 32f;
					if (item3.Item3)
					{
						((Rect)(ref val5))..ctor(num8, num7 + 2f, 45f, 18f);
						Widgets.DrawBoxSolid(val5, new Color(0.3f, 0.5f, 0.7f, 0.5f));
						GUI.color = new Color(0.7f, 0.9f, 1f);
						Text.Font = (GameFont)0;
						Widgets.Label(new Rect(((Rect)(ref val5)).x + 3f, num7 + 3f, 40f, 16f), "Pawn");
						Text.Font = (GameFont)1;
						GUI.color = Color.white;
						num8 += 50f;
					}
					GUI.color = (flag ? new Color(0.5f, 1f, 0.5f) : new Color(0.9f, 0.9f, 0.7f));
					Widgets.Label(new Rect(num8, num7 + 1f, 140f, 20f), item3.Item1);
					GUI.color = Color.white;
					Text.Font = (GameFont)0;
					GUI.color = Color.gray;
					string text = ((item3.Item2.Length > 30) ? (item3.Item2.Substring(0, 27) + "...") : item3.Item2);
					Widgets.Label(new Rect(num8 + 145f, num7 + 2f, width - num8 - 150f, 18f), text);
					Text.Font = (GameFont)1;
					GUI.color = Color.white;
					string text2 = TaggedString.op_Implicit(item3.Item3 ? TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_PawnPropertyTooltip", NamedArgument.op_Implicit(item3.Item1), NamedArgument.op_Implicit(item3.Item2)) : TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_VariableTooltip", NamedArgument.op_Implicit(item3.Item1), NamedArgument.op_Implicit(item3.Item2)));
					TooltipHandler.TipRegion(val4, TipSignal.op_Implicit(text2));
					if (flag2 != flag)
					{
						if (flag2)
						{
							if (!settings.knowledgeMatchingSources.Contains(item3.Item1))
							{
								settings.knowledgeMatchingSources.Add(item3.Item1);
							}
						}
						else
						{
							settings.knowledgeMatchingSources.Remove(item3.Item1);
							if (settings.knowledgeMatchingSources.Count == 0)
							{
								settings.knowledgeMatchingSources.Add("dialogue");
								Messages.Message(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_AtLeastOneSource")), MessageTypeDefOf.RejectInput, true);
							}
						}
					}
					num7 += num3;
				}
				num7 += 2f;
			}
			Widgets.EndScrollView();
			((Listing)listing).Gap(12f);
			int num9 = GenCollection.Count<string>(settings.knowledgeMatchingSources, (Predicate<string>)((string s) => categorizedSources.Any((KeyValuePair<string, List<(string name, string description, bool isPawnProperty)>> c) => GenCollection.Any<(string, string, bool)>(c.Value, (Predicate<(string, string, bool)>)(((string name, string description, bool isPawnProperty) v) => v.name == s && v.isPawnProperty)))));
			int num10 = settings.knowledgeMatchingSources.Count - num9;
			GUI.color = new Color(0.7f, 0.9f, 1f);
			listing.Label(TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_SelectedCount", NamedArgument.op_Implicit(settings.knowledgeMatchingSources.Count), NamedArgument.op_Implicit(num9), NamedArgument.op_Implicit(num10)), -1f, (string)null);
			string text3 = ((settings.knowledgeMatchingSources.Count <= 4) ? string.Join(", ", settings.knowledgeMatchingSources) : string.Format("{0}... (+{1})", string.Join(", ", settings.knowledgeMatchingSources.Take(3)), settings.knowledgeMatchingSources.Count - 3));
			GUI.color = Color.gray;
			listing.Label("  " + text3, -1f, (TipSignal?)null);
			GUI.color = Color.white;
			((Listing)listing).Gap(12f);
			((Listing)listing).GapLine(12f);
		}

		private static Dictionary<string, List<(string name, string description, bool isPawnProperty)>> GetCachedCategorizedSources()
		{
			if (_cachedCategorizedSources == null)
			{
				_cachedCategorizedSources = MustacheVariableHelper.GetCategorizedMatchingSources();
			}
			return _cachedCategorizedSources;
		}

		public static void DrawKnowledgeChainingSettings(Listing_Standard listing, RimTalkMemoryPatchSettings settings)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			listing.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_EnableKnowledgeChaining")), ref settings.enableKnowledgeChaining, (string)null, 0f, 1f);
			if (settings.enableKnowledgeChaining)
			{
				GUI.color = new Color(1f, 0.8f, 0.5f);
				listing.Label("  " + Translator.Translate("RimTalk_Settings_KnowledgeChainingDesc"), -1f, (string)null);
				GUI.color = Color.white;
				((Listing)listing).Gap(12f);
				listing.Label(TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_MaxChainingRoundsLabel", NamedArgument.op_Implicit(settings.maxChainingRounds)), -1f, (string)null);
				settings.maxChainingRounds = (int)listing.Slider((float)settings.maxChainingRounds, 1f, 5f);
			}
			((Listing)listing).Gap(12f);
		}

		public static void DrawPromptNormalizationSettings(Listing_Standard listing, RimTalkMemoryPatchSettings settings)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Expected O, but got Unknown
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0254: Unknown result type (might be due to invalid IL or missing references)
			//IL_025e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02de: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0304: Unknown result type (might be due to invalid IL or missing references)
			//IL_0328: Unknown result type (might be due to invalid IL or missing references)
			//IL_0338: Unknown result type (might be due to invalid IL or missing references)
			//IL_0343: Unknown result type (might be due to invalid IL or missing references)
			//IL_0354: Unknown result type (might be due to invalid IL or missing references)
			//IL_0359: Unknown result type (might be due to invalid IL or missing references)
			//IL_0364: Unknown result type (might be due to invalid IL or missing references)
			//IL_036a: Unknown result type (might be due to invalid IL or missing references)
			//IL_021f: Unknown result type (might be due to invalid IL or missing references)
			Rect rect = ((Listing)listing).GetRect(300f, 1f);
			Widgets.DrawBoxSolid(rect, new Color(0.15f, 0.15f, 0.15f, 0.5f));
			Listing_Standard val = new Listing_Standard();
			((Listing)val).Begin(GenUI.ContractedBy(rect, 10f));
			Text.Font = (GameFont)1;
			GUI.color = new Color(1f, 0.9f, 0.7f);
			val.Label(Translator.Translate("RimTalk_Settings_ReplacementRulesList"), -1f, (string)null);
			GUI.color = Color.white;
			((Listing)val).Gap(5f);
			if (settings.normalizationRules == null)
			{
				settings.normalizationRules = new List<RimTalkMemoryPatchSettings.ReplacementRule>();
			}
			Rect val2 = default(Rect);
			Rect val3 = default(Rect);
			Rect val4 = default(Rect);
			for (int i = 0; i < settings.normalizationRules.Count; i++)
			{
				RimTalkMemoryPatchSettings.ReplacementRule replacementRule = settings.normalizationRules[i];
				Rect rect2 = ((Listing)val).GetRect(30f, 1f);
				((Rect)(ref val2))..ctor(((Rect)(ref rect2)).x, ((Rect)(ref rect2)).y, 24f, 24f);
				Widgets.Checkbox(((Rect)(ref val2)).position, ref replacementRule.isEnabled, 24f, false, false, (Texture2D)null, (Texture2D)null);
				((Rect)(ref val3))..ctor(((Rect)(ref rect2)).x + 30f, ((Rect)(ref rect2)).y, 200f, 25f);
				replacementRule.pattern = Widgets.TextField(val3, replacementRule.pattern ?? "");
				Widgets.Label(new Rect(((Rect)(ref rect2)).x + 235f, ((Rect)(ref rect2)).y, 30f, 25f), " → ");
				((Rect)(ref val4))..ctor(((Rect)(ref rect2)).x + 270f, ((Rect)(ref rect2)).y, 150f, 25f);
				replacementRule.replacement = Widgets.TextField(val4, replacementRule.replacement ?? "");
				Rect val5 = new Rect(((Rect)(ref rect2)).x + 430f, ((Rect)(ref rect2)).y, 30f, 25f);
				GUI.color = new Color(1f, 0.3f, 0.3f);
				if (Widgets.ButtonText(val5, "×", true, true, true, (TextAnchor?)null))
				{
					settings.normalizationRules.RemoveAt(i);
					i--;
				}
				GUI.color = Color.white;
				((Listing)val).Gap(3f);
			}
			if (Widgets.ButtonText(((Listing)val).GetRect(30f, 1f), TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_AddNewRule")), true, true, true, (TextAnchor?)null))
			{
				settings.normalizationRules.Add(new RimTalkMemoryPatchSettings.ReplacementRule("", ""));
			}
			((Listing)val).Gap(5f);
			int num = GenCollection.Count<RimTalkMemoryPatchSettings.ReplacementRule>(settings.normalizationRules, (Predicate<RimTalkMemoryPatchSettings.ReplacementRule>)((RimTalkMemoryPatchSettings.ReplacementRule r) => r.isEnabled));
			GUI.color = Color.gray;
			val.Label(TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_EnabledRulesCount", NamedArgument.op_Implicit(num), NamedArgument.op_Implicit(settings.normalizationRules.Count)), -1f, (string)null);
			GUI.color = Color.white;
			((Listing)val).Gap(3f);
			GUI.color = new Color(0.7f, 0.9f, 1f);
			val.Label(Translator.Translate("RimTalk_Settings_RuleExample"), -1f, (string)null);
			val.Label("   " + Translator.Translate("RimTalk_Settings_RegexSupport"), -1f, (string)null);
			GUI.color = Color.white;
			((Listing)val).End();
		}

		public static void DrawSiliconFlowSettings(Listing_Standard listing, RimTalkMemoryPatchSettings settings)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Unknown result type (might be due to invalid IL or missing references)
			//IL_020a: Unknown result type (might be due to invalid IL or missing references)
			listing.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("RimTalk_Settings_EnableVectorEnhancement")), ref settings.enableVectorEnhancement, (string)null, 0f, 1f);
			if (settings.enableVectorEnhancement)
			{
				GUI.color = new Color(0.8f, 1f, 0.8f);
				listing.Label("  " + Translator.Translate("RimTalk_Settings_VectorEnhancementDesc"), -1f, (string)null);
				GUI.color = Color.white;
				((Listing)listing).Gap(12f);
				listing.Label(TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_SimilarityThresholdLabel", NamedArgument.op_Implicit(settings.vectorSimilarityThreshold.ToString("F2"))), -1f, (string)null);
				settings.vectorSimilarityThreshold = listing.Slider(settings.vectorSimilarityThreshold, 0.5f, 1f);
				listing.Label(TranslatorFormattedStringExtensions.Translate("RimTalk_Settings_MaxVectorResultsLabel", NamedArgument.op_Implicit(settings.maxVectorResults)), -1f, (string)null);
				settings.maxVectorResults = (int)listing.Slider((float)settings.maxVectorResults, 1f, 15f);
				((Listing)listing).Gap(12f);
				GUI.color = new Color(1f, 0.9f, 0.7f);
				listing.Label(Translator.Translate("RimTalk_Settings_CloudEmbeddingConfig"), -1f, (string)null);
				GUI.color = Color.white;
				listing.Label(Translator.Translate("RimTalk_Settings_EmbeddingAPIKey") + ":", -1f, (string)null);
				settings.embeddingApiKey = listing.TextEntry(settings.embeddingApiKey, 1);
				listing.Label(Translator.Translate("RimTalk_Settings_EmbeddingAPIURL") + ":", -1f, (string)null);
				settings.embeddingApiUrl = listing.TextEntry(settings.embeddingApiUrl, 1);
				listing.Label(Translator.Translate("RimTalk_Settings_EmbeddingModel") + ":", -1f, (string)null);
				settings.embeddingModel = listing.TextEntry(settings.embeddingModel, 1);
				GUI.color = Color.gray;
				listing.Label(Translator.Translate("RimTalk_Settings_EmbeddingAPIKeyTip"), -1f, (string)null);
				GUI.color = Color.white;
			}
			((Listing)listing).Gap(12f);
		}

		static SettingsUIDrawers()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			_cachedCategorizedSources = null;
			_matchingSourcesScrollPosition = Vector2.zero;
		}
	}
}
namespace RimTalk.Memory.Injection
{
	public static class ABMCollector
	{
		private static HashSet<RoundMemory> _roundMemoryCache;

		private const int MaxInjectedLength = 5000;

		public static HashSet<RoundMemory> RoundMemoryCache
		{
			get
			{
				if (_roundMemoryCache == null)
				{
					_roundMemoryCache = new HashSet<RoundMemory>();
				}
				return _roundMemoryCache;
			}
			set
			{
				_roundMemoryCache = value;
			}
		}

		public static bool IsRoundMemoryEnabled => RimTalkMemoryPatchMod.Settings?.IsRoundMemoryActive ?? false;

		public static List<MemoryEntry> Collect(Pawn pawn, int maxRounds)
		{
			List<MemoryEntry> list = new List<MemoryEntry>();
			if (pawn == null || maxRounds <= 0)
			{
				return list;
			}
			FourLayerMemoryComp fourLayerMemoryComp = ThingCompUtility.TryGetComp<FourLayerMemoryComp>((Thing)(object)pawn);
			if (fourLayerMemoryComp == null || fourLayerMemoryComp.ActiveMemories == null || fourLayerMemoryComp.ActiveMemories.Count == 0)
			{
				return list;
			}
			List<MemoryEntry> list2 = (from m in fourLayerMemoryComp.ActiveMemories
				orderby (m.Type != MemoryType.Conversation) ? 1 : 0, m.GameTick descending
				select m).ToList();
			int num = 0;
			int num2 = 0;
			foreach (MemoryEntry item2 in list2)
			{
				if (num2 >= maxRounds || num > 5000)
				{
					break;
				}
				if (item2 is RoundMemory item)
				{
					if (!IsRoundMemoryEnabled || (RoundMemoryCache != null && RoundMemoryCache.Contains(item)))
					{
						continue;
					}
					RoundMemoryCache?.Add(item);
				}
				int num3 = item2.Content?.Length ?? 0;
				num += num3;
				num2++;
				list.Add(item2);
			}
			return list;
		}

		public static void ResetDuplicateCache()
		{
			RoundMemoryCache.Clear();
		}

		public static int GetCollectedCount(List<MemoryEntry> collected)
		{
			return collected?.Count ?? 0;
		}
	}
	public static class ELSCollector
	{
		public static List<MemoryEntry> Collect(Pawn pawn, string context, int maxCount)
		{
			List<MemoryEntry> result = new List<MemoryEntry>();
			if (pawn == null || maxCount <= 0)
			{
				return result;
			}
			FourLayerMemoryComp fourLayerMemoryComp = ThingCompUtility.TryGetComp<FourLayerMemoryComp>((Thing)(object)pawn);
			if (fourLayerMemoryComp == null)
			{
				return result;
			}
			DynamicMemoryInjection.InjectMemoriesWithDetails(fourLayerMemoryComp, context, maxCount, out var scores);
			if (scores == null || scores.Count == 0)
			{
				return result;
			}
			return scores.Select((DynamicMemoryInjection.MemoryScore s) => s.Memory).ToList();
		}

		public static List<MemoryEntry> CollectELSOnly(Pawn pawn, string context, int maxCount)
		{
			return (from m in Collect(pawn, context, maxCount * 2)
				where m.Layer == MemoryLayer.EventLog
				select m).Take(maxCount).ToList();
		}

		public static List<MemoryEntry> CollectCLPAOnly(Pawn pawn, string context, int maxCount)
		{
			return (from m in Collect(pawn, context, maxCount * 2)
				where m.Layer == MemoryLayer.Archive
				select m).Take(maxCount).ToList();
		}
	}
	public static class MemoryFormatter
	{
		public static string Format(List<MemoryEntry> memories, int startIndex = 1)
		{
			if (memories == null || memories.Count == 0)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num = startIndex;
			foreach (MemoryEntry memory in memories)
			{
				string memoryTypeTag = GetMemoryTypeTag(memory.Type);
				string timeString = GetTimeString(memory);
				stringBuilder.AppendLine($"{num}. [{memoryTypeTag}] {memory.Content} ({timeString})");
				num++;
			}
			return stringBuilder.ToString().TrimEnd();
		}

		public static string FormatSingle(MemoryEntry memory, int index)
		{
			if (memory == null)
			{
				return string.Empty;
			}
			string memoryTypeTag = GetMemoryTypeTag(memory.Type);
			string timeString = GetTimeString(memory);
			return $"{index}. [{memoryTypeTag}] {memory.Content} ({timeString})";
		}

		private static string GetTimeString(MemoryEntry memory)
		{
			return memory.AgeString;
		}

		public static string GetMemoryTypeTag(MemoryType type)
		{
			return type switch
			{
				MemoryType.Conversation => "Conversation", 
				MemoryType.Action => "Action", 
				MemoryType.Summarization => "Summarization", 
				MemoryType.Event => "Event", 
				MemoryType.Emotion => "Emotion", 
				MemoryType.Relationship => "Relationship", 
				_ => "Memory", 
			};
		}
	}
	public static class UnifiedMemoryInjector
	{
		public static string Inject(Pawn pawn, string dialogueContext)
		{
			if (pawn == null)
			{
				return string.Empty;
			}
			RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
			int maxRounds = settings?.maxABMInjectionRounds ?? 3;
			int num = settings?.maxInjectedMemories ?? 10;
			List<MemoryEntry> list = ABMCollector.Collect(pawn, maxRounds);
			int num2 = num - list.Count;
			List<MemoryEntry> collection = new List<MemoryEntry>();
			if (num2 > 0)
			{
				collection = ELSCollector.Collect(pawn, dialogueContext, num2);
			}
			List<MemoryEntry> list2 = new List<MemoryEntry>();
			list2.AddRange(list);
			list2.AddRange(collection);
			if (list2.Count == 0)
			{
				return string.Empty;
			}
			return MemoryFormatter.Format(list2);
		}

		public static string InjectWithDetails(Pawn pawn, string dialogueContext, out int abmCount, out int elsCount, out int totalCount)
		{
			abmCount = 0;
			elsCount = 0;
			totalCount = 0;
			if (pawn == null)
			{
				return string.Empty;
			}
			RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
			int maxRounds = settings?.maxABMInjectionRounds ?? 3;
			int num = settings?.maxInjectedMemories ?? 10;
			List<MemoryEntry> list = ABMCollector.Collect(pawn, maxRounds);
			abmCount = list.Count;
			int num2 = num - list.Count;
			List<MemoryEntry> list2 = new List<MemoryEntry>();
			if (num2 > 0)
			{
				list2 = ELSCollector.Collect(pawn, dialogueContext, num2);
			}
			elsCount = list2.Count;
			List<MemoryEntry> list3 = new List<MemoryEntry>();
			list3.AddRange(list);
			list3.AddRange(list2);
			totalCount = list3.Count;
			if (list3.Count == 0)
			{
				return string.Empty;
			}
			return MemoryFormatter.Format(list3);
		}

		public static string InjectABMOnly(Pawn pawn)
		{
			if (pawn == null)
			{
				return string.Empty;
			}
			int maxRounds = RimTalkMemoryPatchMod.Settings?.maxABMInjectionRounds ?? 3;
			List<MemoryEntry> list = ABMCollector.Collect(pawn, maxRounds);
			if (list.Count == 0)
			{
				return "(No ABM memories)";
			}
			return MemoryFormatter.Format(list);
		}
	}
}
namespace RimTalk.Memory.Maintenance
{
	public class MemoryMaintainer
	{
		private readonly FourLayerMemoryComp _memoryComp;

		private List<MemoryEntry> ABMList => _memoryComp.ActiveMemories;

		private List<MemoryEntry> SCMList => _memoryComp.SituationalMemories;

		private List<MemoryEntry> ELSList => _memoryComp.EventLogMemories;

		private List<MemoryEntry> CLPAList => _memoryComp.ArchiveMemories;

		public MemoryMaintainer(FourLayerMemoryComp memoryComp)
		{
			_memoryComp = memoryComp;
		}

		public void RunDecay()
		{
			RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
			foreach (MemoryEntry sCM in SCMList)
			{
				sCM?.Decay(settings.ScmDecayRate);
			}
			foreach (MemoryEntry eLS in ELSList)
			{
				eLS?.Decay(settings.ElsDecayRate);
			}
			foreach (MemoryEntry cLPA in CLPAList)
			{
				cLPA?.Decay(settings.ClpaDecayRate);
			}
		}

		public void ConvertActiveMemories()
		{
			TickManager tickManager = Find.TickManager;
			if (tickManager == null)
			{
				return;
			}
			int abmLifespanHours = RimTalkMemoryPatchMod.Settings.AbmLifespanHours;
			if (abmLifespanHours <= 0)
			{
				return;
			}
			List<MemoryEntry> aBMList = ABMList;
			if (aBMList == null || aBMList.Count == 0)
			{
				return;
			}
			int i = 0;
			int ticksGame = tickManager.TicksGame;
			int num = abmLifespanHours * 2500;
			for (; i < aBMList.Count; i++)
			{
				MemoryEntry memoryEntry = aBMList[i];
				if (memoryEntry != null)
				{
					if (ticksGame - memoryEntry.GameTick <= num)
					{
						break;
					}
					if (memoryEntry.Type != MemoryType.Action)
					{
						memoryEntry = memoryEntry.Privatize();
						memoryEntry.Layer = MemoryLayer.Situational;
						SCMList.Add(memoryEntry);
					}
				}
			}
			ABMList.RemoveRange(0, i);
		}

		public void CleanupLowActivityMemories()
		{
			SCMList.RemoveAll((MemoryEntry m) => m?.ShouldBeCleaned ?? true);
			ELSList.RemoveAll((MemoryEntry m) => m?.ShouldBeCleaned ?? true);
			CLPAList.RemoveAll((MemoryEntry m) => m?.ShouldBeCleaned ?? true);
		}

		public void EnforceMemoryLimits()
		{
			RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
			TrimToCapacity(SCMList, settings.maxSituationalMemories);
			TrimToCapacity(ELSList, settings.maxEventLogMemories);
			TrimToCapacity(CLPAList, settings.maxArchiveMemories);
		}

		private static void TrimToCapacity(List<MemoryEntry> memoryList, int max)
		{
			memoryList.RemoveAll((MemoryEntry m) => m == null);
			List<MemoryEntry> list = memoryList.Where((MemoryEntry m) => !m.IsPinned).ToList();
			int num = list.Count - max;
			if (num > 0)
			{
				HashSet<MemoryEntry> hashSet = (from m in list
					orderby m.Activity, m.GameTick
					select m).Take(num).ToHashSet();
				memoryList.RemoveAll(hashSet.Contains);
			}
		}

		public void PinMemory(MemoryEntry memory, bool isPinned)
		{
			if (memory != null)
			{
				if (isPinned && memory.Layer == MemoryLayer.Active)
				{
					ABMList.Remove(memory);
					memory = memory.Privatize();
					memory.Layer = MemoryLayer.Situational;
					SCMList.Add(memory);
				}
				memory.IsPinned = isPinned;
			}
		}

		public bool Remove(MemoryEntry memory)
		{
			if (memory == null)
			{
				return false;
			}
			return (ABMList.RemoveAll((MemoryEntry m) => m == memory) > 0) | (SCMList.RemoveAll((MemoryEntry m) => m == memory) > 0) | (ELSList.RemoveAll((MemoryEntry m) => m == memory) > 0) | (CLPAList.RemoveAll((MemoryEntry m) => m == memory) > 0);
		}
	}
	public class MemorySummarizer
	{
		private readonly FourLayerMemoryComp _memoryComp;

		private int _lastSummarizeTick = -1;

		private const int SummarizeIntervalTicks = 60000;

		private HashSet<MemoryEntry> _summarizingMemories;

		private ThingWithComps Parent => ((ThingComp)_memoryComp).parent;

		private int LastSummarizeTick
		{
			get
			{
				if (_lastSummarizeTick == -1)
				{
					return _lastSummarizeTick = ComputeLatestPotentialSummarizeTick();
				}
				return _lastSummarizeTick;
			}
		}

		private HashSet<MemoryEntry> SummarizingMemories => _summarizingMemories ?? (_summarizingMemories = new HashSet<MemoryEntry>());

		private List<MemoryEntry> ABMList => _memoryComp.ActiveMemories;

		private List<MemoryEntry> SCMList => _memoryComp.SituationalMemories;

		private List<MemoryEntry> ELSList => _memoryComp.EventLogMemories;

		private List<MemoryEntry> CLPAList => _memoryComp.ArchiveMemories;

		private HashSet<long> SummarizedIds => _memoryComp.SummarizedIds;

		public MemorySummarizer(FourLayerMemoryComp memoryComp)
		{
			_memoryComp = memoryComp;
		}

		public void DailySummarize()
		{
			if (!RimTalkMemoryPatchMod.Settings.EnableDailySummarization)
			{
				return;
			}
			ThingWithComps parent = Parent;
			Pawn val = (Pawn)(object)((parent is Pawn) ? parent : null);
			if (val != null && val.IsColonist && GenTicks.TicksGame - LastSummarizeTick >= 60000)
			{
				int num = ComputeLatestPotentialSummarizeTick();
				if (num > _lastSummarizeTick)
				{
					AutoSummarize(num);
					_lastSummarizeTick = num;
				}
			}
		}

		public void AutoSummarize(int latestPotentialSummarizeTick = -1)
		{
			if (latestPotentialSummarizeTick == -1)
			{
				latestPotentialSummarizeTick = ComputeLatestPotentialSummarizeTick();
			}
			foreach (IGrouping<int, MemoryEntry> item in from m in ABMList.Where(IsSummarizable).Concat(SCMList.Where(IsSummarizable))
				group m by (latestPotentialSummarizeTick - m.GameTick) / 60000 into g
				orderby g.Key descending
				select g)
			{
				SummarizeInternal(item);
			}
			bool IsSummarizable(MemoryEntry memory)
			{
				if (memory != null && !SummarizedIds.Contains(memory.OriginId) && memory.GameTick < latestPotentialSummarizeTick)
				{
					return !SummarizingMemories.Contains(memory);
				}
				return false;
			}
		}

		public void ManualSummarize(IEnumerable<MemoryEntry> source)
		{
			HashSet<MemoryEntry> sourceHashSet;
			if (source != null && source.Any())
			{
				sourceHashSet = source.ToHashSet();
				List<MemoryEntry> list = new List<MemoryEntry>();
				list.AddRange(ABMList.Where(IsSummarizable));
				list.AddRange(SCMList.Where(IsSummarizable));
				List<MemoryEntry> list2 = list;
				if (list2.Count == 0)
				{
					Messages.Message("无可总结条目", MessageTypeDefOf.RejectInput, false);
				}
				else
				{
					SummarizeInternal(list2);
				}
			}
			bool IsSummarizable(MemoryEntry memory)
			{
				if (memory != null && sourceHashSet.Contains(memory))
				{
					return !SummarizingMemories.Contains(memory);
				}
				return false;
			}
		}

		private void SummarizeInternal(IEnumerable<MemoryEntry> memories)
		{
			List<MemoryEntry> memoryList = memories.OrderBy((MemoryEntry m) => m.GameTick).ToList();
			string text = BuildPrompt(memoryList, RimTalkMemoryPatchMod.Settings.SummarizePrompt, "殖民者{0}的记忆总结\n\n记忆列表\n{1}\n\n要求提炼地点人物事件\n相似事件合并标注频率\n极简表达不超过80字\n只输出总结文字不要其他格式", showHour: true);
			if (string.IsNullOrWhiteSpace(text))
			{
				Log.Error("[RimTalk.Memory.Maintenance] 总结提示词构建失败");
				return;
			}
			MemoryEntry summaryMemory = BuildEmptySummary(memoryList, MemoryLayer.EventLog);
			foreach (MemoryEntry item in memoryList)
			{
				SummarizingMemories.Add(item);
			}
			try
			{
				AIService.EnqueueAIRequest(text, delegate(string result)
				{
					if (string.IsNullOrWhiteSpace(result))
					{
						Log.Error("[RimTalk.Memory.Maintenance] AI 总结请求返回空结果，总结失败");
						return;
					}
					summaryMemory.Content = result;
					ELSList?.Add(summaryMemory);
					foreach (MemoryEntry item2 in memoryList)
					{
						if (item2 != null)
						{
							SummarizedIds.Add(item2.OriginId);
						}
					}
				}, delegate
				{
					foreach (MemoryEntry item3 in memoryList)
					{
						SummarizingMemories.Remove(item3);
					}
				});
			}
			catch (Exception ex)
			{
				foreach (MemoryEntry item4 in memoryList)
				{
					SummarizingMemories.Remove(item4);
				}
				Log.Error("[RimTalk.Memory.Maintenance] 总结过程中发生异常: " + ex.Message);
			}
		}

		public void PeriodicArchive()
		{
			if (RimTalkMemoryPatchMod.Settings.EnableAutoArchive)
			{
				ThingWithComps parent = Parent;
				Pawn val = (Pawn)(object)((parent is Pawn) ? parent : null);
				if (val != null && val.IsColonist && Gen.IsHashIntervalTick((Thing)(object)Parent, 60000 * RimTalkMemoryPatchMod.Settings.ArchiveIntervalDays))
				{
					Archive();
				}
			}
		}

		public void Archive(IEnumerable<MemoryEntry> source = null)
		{
			List<MemoryEntry> list = source?.Where((MemoryEntry m) => m != null && m.Type == MemoryType.Summarization && !SummarizingMemories.Contains(m)).ToList() ?? ELSList.Where((MemoryEntry m) => m != null && !SummarizedIds.Contains(m.OriginId) && !SummarizingMemories.Contains(m)).ToList();
			if (list.Count == 0)
			{
				if (source != null)
				{
					Messages.Message("无可归档条目", MessageTypeDefOf.RejectInput, false);
				}
			}
			else
			{
				ArchiveInternal(list);
			}
		}

		private void ArchiveInternal(IEnumerable<MemoryEntry> memories)
		{
			List<MemoryEntry> memoryList = memories.OrderBy((MemoryEntry m) => m.GameTick).ToList();
			string text = BuildPrompt(memoryList, RimTalkMemoryPatchMod.Settings.ArchivePrompt, "殖民者{0}的记忆归档\n\n记忆列表\n{1}\n\n要求提炼核心特征和里程碑事件\n合并相似经历突出长期趋势\n极简表达不超过60字\n只输出总结文字不要其他格式");
			if (string.IsNullOrWhiteSpace(text))
			{
				Log.Error("[RimTalk.Memory.Maintenance] 归档提示词构建失败");
				return;
			}
			MemoryEntry archiveMemory = BuildEmptySummary(memoryList, MemoryLayer.Archive);
			foreach (MemoryEntry item in memoryList)
			{
				SummarizingMemories.Add(item);
			}
			try
			{
				AIService.EnqueueAIRequest(text, delegate(string result)
				{
					if (string.IsNullOrWhiteSpace(result))
					{
						Log.Error("[RimTalk.Memory.Maintenance] AI 归档请求返回空结果，归档失败");
						return;
					}
					archiveMemory.Content = result;
					CLPAList?.Add(archiveMemory);
					foreach (MemoryEntry item2 in memoryList)
					{
						if (item2 != null)
						{
							SummarizedIds.Add(item2.OriginId);
						}
					}
				}, delegate
				{
					foreach (MemoryEntry item3 in memoryList)
					{
						SummarizingMemories.Remove(item3);
					}
				});
			}
			catch (Exception ex)
			{
				foreach (MemoryEntry item4 in memoryList)
				{
					SummarizingMemories.Remove(item4);
				}
				Log.Error("[RimTalk.Memory.Maintenance] 归档过程中发生异常: " + ex.Message);
			}
		}

		public bool CheckSummarizing(MemoryEntry memory)
		{
			return SummarizingMemories.Contains(memory);
		}

		public bool CheckSummarized(MemoryEntry memory)
		{
			return SummarizedIds.Contains(memory?.OriginId ?? 0);
		}

		private int ComputeLatestPotentialSummarizeTick()
		{
			TickManager tickManager = Find.TickManager;
			if (tickManager == null)
			{
				return -1;
			}
			int num = GenTicks.TicksGame - GenLocalDate.DayTick((Thing)(object)Parent) + RimTalkMemoryPatchMod.Settings.summarizationHour * 2500;
			if (num > tickManager.TicksGame)
			{
				return num - 60000;
			}
			return num;
		}

		private string BuildPrompt(List<MemoryEntry> memoryList, string template, string backUp = null, bool showHour = false)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrWhiteSpace(template))
			{
				if (backUp == null)
				{
					return string.Empty;
				}
				template = backUp;
			}
			? val;
			if (!showHour)
			{
				val = Vector2.zero;
			}
			else
			{
				WorldGrid worldGrid = Find.WorldGrid;
				val = ((worldGrid != null) ? worldGrid.LongLatOf(((Thing)Parent).Tile) : Vector2.zero);
			}
			Vector2 location = (Vector2)val;
			string arg = string.Join("\n\n", from m in memoryList
				where !string.IsNullOrWhiteSpace(m.Content)
				select m.AgeString + (showHour ? (", " + ComputeHour(m)) : string.Empty) + "\n" + m.Content);
			try
			{
				string format = template;
				string labelShort = ((Entity)Parent).LabelShort;
				ThingWithComps parent = Parent;
				Pawn val2 = (Pawn)(object)((parent is Pawn) ? parent : null);
				string text;
				if (val2 == null)
				{
					text = string.Empty;
				}
				else
				{
					object arg2 = val2.gender;
					Pawn_AgeTracker ageTracker = val2.ageTracker;
					text = $"({arg2}, {((ageTracker != null) ? new int?(ageTracker.AgeBiologicalYears) : ((int?)null))})";
				}
				return string.Format(format, labelShort + text, arg);
			}
			catch (Exception ex)
			{
				if (backUp == null)
				{
					return string.Empty;
				}
				Log.Error("[RimTalk.Memory.Maintenance] 提示词非法，尝试使用默认模板: " + ex.Message);
				string labelShort2 = ((Entity)Parent).LabelShort;
				ThingWithComps parent2 = Parent;
				Pawn val3 = (Pawn)(object)((parent2 is Pawn) ? parent2 : null);
				string text2;
				if (val3 == null)
				{
					text2 = string.Empty;
				}
				else
				{
					object arg3 = val3.gender;
					Pawn_AgeTracker ageTracker2 = val3.ageTracker;
					text2 = $"({arg3}, {((ageTracker2 != null) ? new int?(ageTracker2.AgeBiologicalYears) : ((int?)null))})";
				}
				return string.Format(backUp, labelShort2 + text2, arg);
			}
			string ComputeHour(MemoryEntry memory)
			{
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				return GenDateExtension.GetInGameHour12HString(GenDate.TickGameToAbs(memory.GameTick), location);
			}
		}

		private MemoryEntry BuildEmptySummary(List<MemoryEntry> memoryList, MemoryLayer targetLayer)
		{
			bool flag = targetLayer == MemoryLayer.Archive;
			MemoryEntry memoryEntry = new MemoryEntry(null, MemoryType.Summarization, targetLayer, memoryList.Average((MemoryEntry m) => m.Importance));
			int gameTick;
			if (!flag)
			{
				gameTick = memoryList[memoryList.Count - 1].GameTick;
			}
			else
			{
				gameTick = memoryList[0].GameTick;
			}
			memoryEntry.GameTick = gameTick;
			memoryEntry.tags = memoryList.SelectMany((MemoryEntry m) => m.tags).Distinct().ToList();
			memoryEntry.keywords = memoryList.SelectMany((MemoryEntry m) => m.keywords).Distinct().ToList();
			MemoryEntry memoryEntry2 = memoryEntry;
			if (flag)
			{
				memoryEntry2.EndGameTick = memoryList[memoryList.Count - 1].GameTick;
			}
			return memoryEntry2;
		}
	}
}
namespace RimTalk.Memory.Capture
{
	public class CombatMemoryCapturer
	{
		private const int SessionTimeoutTicks = 1250;

		private const int SessionMaxDurationTicks = 5000;

		private const float CombatImportance = 0.9f;

		private const int CellTargetId = -1;

		private const string CellTargetName = "目标区域";

		private readonly FourLayerMemoryComp _memoryComp;

		private MemoryEntry _lastCombatMemory;

		private int _startGameTick;

		private int _lastActiveTick;

		private bool _lastAttackWasMelee;

		private Dictionary<string, HashSet<int>> _dictTargetNameToIds;

		private Dictionary<string, HashSet<int>> DictTargetNameToIds => _dictTargetNameToIds ?? (_dictTargetNameToIds = new Dictionary<string, HashSet<int>>());

		public CombatMemoryCapturer(FourLayerMemoryComp memoryComp)
		{
			_memoryComp = memoryComp;
		}

		public static void CaptureAttackEnter(Pawn pawn, Verb verb)
		{
			if (!RimTalkMemoryPatchMod.Settings.EnableCombatMemory || pawn == null || !pawn.IsColonist || verb == null || pawn.CurJobDef != JobDefOf.Wait_Combat)
			{
				return;
			}
			if (!verb.IsMeleeAttack)
			{
				VerbProperties verbProps = verb.verbProps;
				if (verbProps == null || !verbProps.Ranged || !verbProps.violent)
				{
					return;
				}
			}
			((ThingWithComps)pawn).GetComp<FourLayerMemoryComp>()?.CombatCapturer?.CaptureAttack(verb);
		}

		private void CaptureAttack(Verb verb)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			int ticksGame = GenTicks.TicksGame;
			bool isMeleeAttack = verb.IsMeleeAttack;
			int targetId = GetTargetId(verb.CurrentTarget);
			string targetName = GetTargetName(verb.CurrentTarget);
			if (ticksGame - _startGameTick <= 5000 && ticksGame - _lastActiveTick <= 1250 && isMeleeAttack == _lastAttackWasMelee && _lastCombatMemory != null)
			{
				if (!DictTargetNameToIds.TryGetValue(targetName, out var value))
				{
					HashSet<int> hashSet = (DictTargetNameToIds[targetName] = new HashSet<int>());
					value = hashSet;
				}
				value.Add(targetId);
				_lastActiveTick = ticksGame;
				_lastCombatMemory.Content = BuildContent();
			}
			else
			{
				_startGameTick = (_lastActiveTick = ticksGame);
				_lastAttackWasMelee = isMeleeAttack;
				DictTargetNameToIds.Clear();
				DictTargetNameToIds[targetName] = new HashSet<int> { targetId };
				_lastCombatMemory = new MemoryEntry(BuildContent(), MemoryType.Action, MemoryLayer.Active, 0.9f);
				_memoryComp.ActiveMemories.Add(_lastCombatMemory);
			}
		}

		private static int GetTargetId(LocalTargetInfo target)
		{
			if (!((LocalTargetInfo)(ref target)).HasThing)
			{
				return -1;
			}
			return ((LocalTargetInfo)(ref target)).Thing.thingIDNumber;
		}

		private static string GetTargetName(LocalTargetInfo target)
		{
			Thing thing = ((LocalTargetInfo)(ref target)).Thing;
			if (thing == null)
			{
				return "目标区域";
			}
			return ((Entity)thing).LabelShort ?? ((Def)(thing.def?)).label ?? "目标";
		}

		private string BuildContent()
		{
			string text = (_lastAttackWasMelee ? "近身攻击" : "射击");
			if (DictTargetNameToIds.Count == 1)
			{
				KeyValuePair<string, HashSet<int>> keyValuePair = DictTargetNameToIds.First();
				HashSet<int> value = keyValuePair.Value;
				if (value != null && value.Count == 1)
				{
					return text + keyValuePair.Key;
				}
			}
			List<KeyValuePair<string, HashSet<int>>> list = DictTargetNameToIds.OrderByDescending((KeyValuePair<string, HashSet<int>> kv) => kv.Value.Count).ToList();
			return "连续" + text + "了" + string.Join("、", from kv in list.Take(3)
				select $"{kv.Value.Count}个{kv.Key}") + ((list.Count > 3) ? "等" : "");
		}
	}
	public class JobMemoryCapturer
	{
		private static readonly HashSet<JobDef> _jobsToIgnore;

		private static readonly HashSet<JobDef> _jobsAlwaysCapture;

		private static readonly Dictionary<JobDef, string> _dictAggregatedJobToDesc;

		private static readonly Dictionary<JobDef, float> _dictJobToImportance;

		private const int SessionTimeoutTicks = 5000;

		private const int SessionMaxDurationTicks = 30000;

		private readonly FourLayerMemoryComp _memoryComp;

		private int _startGameTick;

		private int _lastActiveTick;

		private int _repeatCount;

		private HashSet<string> _targetNames;

		private MemoryEntry _lastJobMemory;

		private string _lastJobReport;

		private string _lastJobAggregateDesc;

		private WeakReference<Job> _curJobWeakRef;

		private string _curJobReport;

		private string _curJobTargetAName;

		private ThingWithComps Parent => ((ThingComp)_memoryComp).parent;

		private HashSet<string> TargetNames => _targetNames ?? (_targetNames = new HashSet<string>());

		private Job CurJob
		{
			get
			{
				if (_curJobWeakRef == null)
				{
					_curJobWeakRef = new WeakReference<Job>(null);
				}
				_curJobWeakRef.TryGetTarget(out var target);
				return target;
			}
			set
			{
				if (_curJobWeakRef == null)
				{
					_curJobWeakRef = new WeakReference<Job>(null);
				}
				_curJobWeakRef.SetTarget(value);
			}
		}

		static JobMemoryCapturer()
		{
			_jobsToIgnore = new HashSet<JobDef>();
			_jobsAlwaysCapture = new HashSet<JobDef>();
			_dictAggregatedJobToDesc = new Dictionary<JobDef, string>();
			_dictJobToImportance = new Dictionary<JobDef, float>();
			_jobsToIgnore.UnionWith(new global::<>z__ReadOnlyArray<JobDef>((JobDef[])(object)new JobDef[12]
			{
				JobDefOf.Goto,
				JobDefOf.GotoWander,
				JobDefOf.Wait,
				JobDefOf.Wait_Wander,
				JobDefOf.Wait_Combat,
				JobDefOf.Wait_MaintainPosture,
				JobDefOf.Wait_Asleep,
				JobDefOf.Wait_AsleepDormancy,
				JobDefOf.Wait_WithSleeping,
				JobDefOf.LayDown,
				JobDefOf.LayDownAwake,
				JobDefOf.LayDownResting
			}));
			_jobsAlwaysCapture.UnionWith(new global::<>z__ReadOnlyArray<JobDef>((JobDef[])(object)new JobDef[5]
			{
				JobDefOf.AttackMelee,
				JobDefOf.AttackStatic,
				JobDefOf.CastAbilityOnThing,
				JobDefOf.CastAbilityOnWorldTile,
				JobDefOf.Lovin
			}));
			_jobsAlwaysCapture.Remove(null);
			foreach (JobDef item in DefDatabase<JobDef>.AllDefsListForReading)
			{
				if (item != null)
				{
					string defName = ((Def)item).defName;
					string empty = string.Empty;
					string text = (defName.Contains("Haul") ? "搬运" : (defName.Contains("Harvest") ? "收获" : (defName.Contains("CutPlant") ? "割除" : (defName.Contains("Mine") ? "采矿" : (defName.Contains("Repair") ? "修理" : (defName.Contains("Milk") ? "挤奶" : (defName.Contains("Shear") ? "剪毛" : ((!defName.Contains("Deconstruct") && !defName.Contains("RemoveFloor") && !defName.Contains("RemoveRoof") && !defName.Contains("Uninstall")) ? ((!defName.Contains("Frame") && !defName.Contains("BuildRoof") && !defName.Contains("Smooth")) ? ((!defName.Contains("Sow") && !defName.Contains("Replant") && !defName.Contains("PlantSeed")) ? ((!defName.Contains("Clean") && !defName.Contains("Clear")) ? empty : "清洁") : "种植") : "建造") : "拆除"))))))));
					empty = text;
					if (!string.IsNullOrEmpty(empty))
					{
						_dictAggregatedJobToDesc[item] = empty;
					}
				}
			}
			AddToAggregatedDict(JobDefOf.AttackMelee, "近身攻击");
			AddToAggregatedDict(JobDefOf.AttackStatic, "射击");
			AddToImportanceDict(JobDefOf.AttackMelee, 0.9f);
			AddToImportanceDict(JobDefOf.AttackStatic, 0.9f);
			AddToImportanceDict(JobDefOf.SocialFight, 0.85f);
			AddToImportanceDict(JobDefOf.MarryAdjacentPawn, 1f);
			AddToImportanceDict(JobDefOf.SpectateCeremony, 0.7f);
			AddToImportanceDict(JobDefOf.Lovin, 0.6f);
			static void AddToAggregatedDict(JobDef jobDef, string desc)
			{
				if (jobDef != null)
				{
					_dictAggregatedJobToDesc[jobDef] = desc;
				}
			}
			static void AddToImportanceDict(JobDef jobDef, float importance)
			{
				if (jobDef != null)
				{
					_dictJobToImportance[jobDef] = importance;
				}
			}
		}

		public JobMemoryCapturer(FourLayerMemoryComp memoryComp)
		{
			_memoryComp = memoryComp;
		}

		public static void ExtractJobInfoEnter(Job job, Pawn pawn)
		{
			if (RimTalkMemoryPatchMod.Settings.EnableActionMemory && pawn != null && pawn.IsColonist)
			{
				JobDef val = job?.def;
				if (val != null && !_jobsToIgnore.Contains(val))
				{
					((ThingWithComps)pawn).GetComp<FourLayerMemoryComp>()?.JobCapturer?.ExtractJobInfo(job);
				}
			}
		}

		public static void BuildJobMemoryEnter(JobCondition condition, Job job, Pawn pawn)
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Invalid comparison between Unknown and I4
			if (RimTalkMemoryPatchMod.Settings.EnableActionMemory && pawn != null && pawn.IsColonist)
			{
				JobDef val = job?.def;
				if (val != null && !_jobsToIgnore.Contains(val) && ((int)condition == 2 || _jobsAlwaysCapture.Contains(val)))
				{
					((ThingWithComps)pawn).GetComp<FourLayerMemoryComp>()?.JobCapturer?.BuildJobMemory(job);
				}
			}
		}

		private void ExtractJobInfo(Job job)
		{
			CurJob = job;
			_curJobReport = GetJobReport(job);
			_curJobTargetAName = (_dictAggregatedJobToDesc.ContainsKey(job.def) ? GetTargetAName(job) : null);
		}

		private void BuildJobMemory(Job job)
		{
			bool? sharedConditionCache = null;
			string report = null;
			if (TargetNames.Count <= 1 && getReport() == _lastJobReport && sharedCondition())
			{
				_lastJobMemory.Importance += ImportanceIncrement();
				UpdateSession();
				_lastJobMemory.Content = BuildExactContent();
				return;
			}
			JobDef def = job.def;
			bool flag = _dictAggregatedJobToDesc.TryGetValue(def, out var value);
			if (flag && value == _lastJobAggregateDesc && sharedCondition())
			{
				_lastJobMemory.Importance += ImportanceIncrement();
				UpdateSession(GetTargetANameCached(job));
				_lastJobMemory.Content = BuildFuzzyContent();
			}
			else
			{
				string text = getReport();
				MemoryEntry memoryEntry = new MemoryEntry(text, MemoryType.Action, MemoryLayer.Active, _dictJobToImportance.TryGetValue(def, out var value2) ? value2 : 0.5f);
				_memoryComp.ActiveMemories.Add(memoryEntry);
				StartNewSession(memoryEntry, text, value, flag ? GetTargetANameCached(job) : null);
			}
			string getReport()
			{
				return report ?? (report = GetJobReportCached(job));
			}
			bool sharedCondition()
			{
				bool valueOrDefault = sharedConditionCache == true;
				if (!sharedConditionCache.HasValue)
				{
					valueOrDefault = GenTicks.TicksGame - _lastActiveTick <= 5000 && GenTicks.TicksGame - _startGameTick <= 30000 && _lastJobMemory != null;
					sharedConditionCache = valueOrDefault;
					return valueOrDefault;
				}
				return valueOrDefault;
			}
		}

		private void StartNewSession(MemoryEntry newMemory, string report, string jobAggregateDesc, string targetName = null)
		{
			_startGameTick = (_lastActiveTick = GenTicks.TicksGame);
			_repeatCount = 1;
			TargetNames.Clear();
			if (!string.IsNullOrEmpty(targetName))
			{
				TargetNames.Add(targetName);
			}
			_lastJobMemory = newMemory;
			_lastJobReport = report;
			_lastJobAggregateDesc = jobAggregateDesc;
		}

		private void UpdateSession(string targetName = null)
		{
			_lastActiveTick = GenTicks.TicksGame;
			_repeatCount++;
			if (!string.IsNullOrEmpty(targetName))
			{
				TargetNames.Add(targetName);
			}
		}

		private string GetJobReportCached(Job job)
		{
			if (CurJob == job && !string.IsNullOrEmpty(_curJobReport))
			{
				return _curJobReport;
			}
			return GetJobReport(job);
		}

		private string GetJobReport(Job job)
		{
			ThingWithComps parent = Parent;
			Pawn val = (Pawn)(object)((parent is Pawn) ? parent : null);
			if (val == null)
			{
				return string.Empty;
			}
			string text = null;
			try
			{
				text = job.GetReport(val);
			}
			catch
			{
				return string.Empty;
			}
			if (string.IsNullOrEmpty(text))
			{
				return string.Empty;
			}
			if (text.StartsWith("正在"))
			{
				text = text.Substring(2);
			}
			return text;
		}

		private string GetTargetANameCached(Job job)
		{
			if (CurJob == job && !string.IsNullOrEmpty(_curJobTargetAName))
			{
				return _curJobTargetAName;
			}
			return GetTargetAName(job);
		}

		private string GetTargetAName(Job job)
		{
			if (!((LocalTargetInfo)(ref job.targetA)).HasThing)
			{
				return string.Empty;
			}
			Thing thing = ((LocalTargetInfo)(ref job.targetA)).Thing;
			if ((object)thing == Parent)
			{
				return "自己";
			}
			try
			{
				if ((thing is Blueprint || thing is Frame) ? true : false)
				{
					return ((Def)(thing.def?.entityDefToBuild?)).label ?? string.Empty;
				}
				return ((Entity)thing).LabelShort ?? ((Def)(thing.def?)).label ?? string.Empty;
			}
			catch
			{
				return string.Empty;
			}
		}

		private string BuildExactContent()
		{
			return $"{GetDurationDesc()}{_repeatCount}次{_lastJobReport}";
		}

		private string BuildFuzzyContent()
		{
			return string.Format("{0}{1}了{2}次{3}{4}。", GetDurationDesc(), _lastJobAggregateDesc, _repeatCount, string.Join("、", TargetNames.Take(3)), (TargetNames.Count > 3) ? "等" : "");
		}

		private string GetDurationDesc()
		{
			int num = GenTicks.TicksGame - _startGameTick;
			if (num <= 20000)
			{
				if (num <= 5000)
				{
					if (num <= 2500)
					{
						return "连续";
					}
					return "两小时内";
				}
				if (num <= 10000)
				{
					return "花费几小时";
				}
				return "花费小半天";
			}
			if (num <= 30000)
			{
				return "花费大半天";
			}
			return "花费一整天";
		}

		private float ImportanceIncrement()
		{
			float num = (float)(GenTicks.TicksGame - _lastActiveTick) * 8E-06f;
			if (_repeatCount <= 20)
			{
				num += 0.01f;
			}
			return num;
		}
	}
}
namespace RimTalk.Memory.API
{
	public static class KnowledgeVariableProvider
	{
		public class KnowledgeInjectionContext
		{
			public string MatchText { get; set; }

			public string DialogueType { get; set; }

			public string KeywordKnowledge { get; set; }

			public Pawn Speaker { get; set; }

			public Pawn Listener { get; set; }

			public int Tick { get; set; }
		}

		private static KnowledgeInjectionContext _lastContext;

		private static readonly object _contextLock = new object();

		private static List<KnowledgeScore> _cachedScores;

		private static int _cachedTick = -1;

		private static readonly object _cacheLock = new object();

		public static KnowledgeInjectionContext GetLastContext()
		{
			lock (_contextLock)
			{
				return _lastContext;
			}
		}

		public static void ClearContext()
		{
			lock (_contextLock)
			{
				_lastContext = null;
			}
		}

		public static string GetMatchedKnowledge(object promptContext)
		{
			if (promptContext == null)
			{
				return "";
			}
			try
			{
				RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
				string text = BuildMatchText(promptContext, settings);
				if (string.IsNullOrEmpty(text))
				{
					return "(No context available for matching)";
				}
				Pawn propertyValue = GetPropertyValue<Pawn>(promptContext, "CurrentPawn");
				Pawn val = null;
				List<Pawn> propertyValue2 = GetPropertyValue<List<Pawn>>(promptContext, "AllPawns");
				if (propertyValue2 != null && propertyValue2.Count > 1)
				{
					val = propertyValue2[1];
				}
				World world = Find.World;
				MemoryManager memoryManager = ((world != null) ? world.GetComponent<MemoryManager>() : null);
				if (memoryManager?.CommonKnowledge == null)
				{
					return "(No world knowledge available)";
				}
				string text2 = memoryManager.CommonKnowledge.InjectKnowledgeWithDetails(text, settings.maxInjectedKnowledge, out var _, propertyValue, val);
				string variableValue = GetVariableValue(promptContext, "dialogue.type");
				lock (_contextLock)
				{
					KnowledgeInjectionContext obj = new KnowledgeInjectionContext
					{
						MatchText = text,
						DialogueType = variableValue,
						KeywordKnowledge = text2,
						Speaker = propertyValue,
						Listener = val
					};
					TickManager tickManager = Find.TickManager;
					obj.Tick = ((tickManager != null) ? tickManager.TicksGame : 0);
					_lastContext = obj;
				}
				if (string.IsNullOrEmpty(text2))
				{
					return "(No matching knowledge found)";
				}
				return PromptNormalizer.Normalize(text2);
			}
			catch (Exception ex)
			{
				Log.Warning("[MemoryPatch] Error getting knowledge: " + ex.Message);
				return "";
			}
		}

		public static string GetGroupedKnowledge(object promptContext)
		{
			if (promptContext == null)
			{
				return "";
			}
			try
			{
				List<KnowledgeScore> matchedScores = GetMatchedScores(promptContext);
				if (matchedScores == null || matchedScores.Count == 0)
				{
					return "(No matching knowledge found)";
				}
				return PromptNormalizer.Normalize(FormatGroupedKnowledge(matchedScores));
			}
			catch (Exception ex)
			{
				Log.Warning("[MemoryPatch] Error getting grouped knowledge: " + ex.Message);
				return "";
			}
		}

		public static string GetKnowledgeByCategory(object promptContext, KnowledgeCategory category)
		{
			if (promptContext == null)
			{
				return "";
			}
			try
			{
				List<KnowledgeScore> matchedScores = GetMatchedScores(promptContext);
				if (matchedScores == null || matchedScores.Count == 0)
				{
					return "";
				}
				List<KnowledgeScore> list = matchedScores.Where((KnowledgeScore s) => CommonKnowledgeUIHelpers.GetEntryCategory(s.Entry) == category).ToList();
				if (list.Count == 0)
				{
					return "";
				}
				return PromptNormalizer.Normalize(FormatCategoryEntries(list));
			}
			catch (Exception ex)
			{
				Log.Warning($"[MemoryPatch] Error getting knowledge for category {category}: {ex.Message}");
				return "";
			}
		}

		public static string GetKnowledgeRules(object ctx)
		{
			return GetKnowledgeByCategory(ctx, KnowledgeCategory.Instructions);
		}

		public static string GetKnowledgeLore(object ctx)
		{
			return GetKnowledgeByCategory(ctx, KnowledgeCategory.Lore);
		}

		public static string GetKnowledgeStatus(object ctx)
		{
			return GetKnowledgeByCategory(ctx, KnowledgeCategory.PawnStatus);
		}

		public static string GetKnowledgeHistory(object ctx)
		{
			return GetKnowledgeByCategory(ctx, KnowledgeCategory.History);
		}

		public static string GetKnowledgeOther(object ctx)
		{
			return GetKnowledgeByCategory(ctx, KnowledgeCategory.Other);
		}

		private static List<KnowledgeScore> GetMatchedScores(object promptContext)
		{
			TickManager tickManager = Find.TickManager;
			int num = ((tickManager != null) ? tickManager.TicksGame : 0);
			lock (_cacheLock)
			{
				if (_cachedScores != null && _cachedTick == num)
				{
					return _cachedScores;
				}
			}
			RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
			string text = BuildMatchText(promptContext, settings);
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			Pawn propertyValue = GetPropertyValue<Pawn>(promptContext, "CurrentPawn");
			Pawn val = null;
			List<Pawn> propertyValue2 = GetPropertyValue<List<Pawn>>(promptContext, "AllPawns");
			if (propertyValue2 != null && propertyValue2.Count > 1)
			{
				val = propertyValue2[1];
			}
			World world = Find.World;
			MemoryManager memoryManager = ((world != null) ? world.GetComponent<MemoryManager>() : null);
			if (memoryManager?.CommonKnowledge == null)
			{
				return null;
			}
			memoryManager.CommonKnowledge.InjectKnowledgeWithDetails(text, settings.maxInjectedKnowledge, out var scores, propertyValue, val);
			string variableValue = GetVariableValue(promptContext, "dialogue.type");
			lock (_contextLock)
			{
				_lastContext = new KnowledgeInjectionContext
				{
					MatchText = text,
					DialogueType = variableValue,
					KeywordKnowledge = FormatCategoryEntries(scores),
					Speaker = propertyValue,
					Listener = val,
					Tick = num
				};
			}
			lock (_cacheLock)
			{
				_cachedScores = scores;
				_cachedTick = num;
				return scores;
			}
		}

		private static string GetCategoryDisplayName(KnowledgeCategory category)
		{
			return category switch
			{
				KnowledgeCategory.Instructions => "规则", 
				KnowledgeCategory.Lore => "世界观", 
				KnowledgeCategory.PawnStatus => "殖民者状态", 
				KnowledgeCategory.History => "历史", 
				KnowledgeCategory.Other => "其他", 
				_ => "未知", 
			};
		}

		private static string GetFirstTag(CommonKnowledgeEntry entry)
		{
			if (string.IsNullOrEmpty(entry.tag))
			{
				return "";
			}
			List<string> tags = entry.GetTags();
			if (tags.Count <= 0)
			{
				return entry.tag;
			}
			return tags[0];
		}

		private static string FormatGroupedKnowledge(List<KnowledgeScore> scores)
		{
			if (scores == null || scores.Count == 0)
			{
				return "";
			}
			Dictionary<KnowledgeCategory, List<KnowledgeScore>> dictionary = new Dictionary<KnowledgeCategory, List<KnowledgeScore>>();
			foreach (KnowledgeScore score in scores)
			{
				KnowledgeCategory entryCategory = CommonKnowledgeUIHelpers.GetEntryCategory(score.Entry);
				if (!dictionary.ContainsKey(entryCategory))
				{
					dictionary[entryCategory] = new List<KnowledgeScore>();
				}
				dictionary[entryCategory].Add(score);
			}
			KnowledgeCategory[] obj = new KnowledgeCategory[5]
			{
				KnowledgeCategory.Instructions,
				KnowledgeCategory.Lore,
				KnowledgeCategory.PawnStatus,
				KnowledgeCategory.History,
				KnowledgeCategory.Other
			};
			StringBuilder stringBuilder = new StringBuilder();
			KnowledgeCategory[] array = obj;
			foreach (KnowledgeCategory knowledgeCategory in array)
			{
				if (!dictionary.ContainsKey(knowledgeCategory))
				{
					continue;
				}
				List<KnowledgeScore> list = dictionary[knowledgeCategory];
				stringBuilder.AppendLine("## " + GetCategoryDisplayName(knowledgeCategory));
				foreach (KnowledgeScore item in list)
				{
					stringBuilder.AppendLine("- " + item.Entry.content);
				}
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString().TrimEnd();
		}

		private static string FormatCategoryEntries(List<KnowledgeScore> scores)
		{
			if (scores == null || scores.Count == 0)
			{
				return "";
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KnowledgeScore score in scores)
			{
				stringBuilder.AppendLine("- " + score.Entry.content);
			}
			return stringBuilder.ToString().TrimEnd();
		}

		private static string BuildMatchText(object promptContext, RimTalkMemoryPatchSettings settings)
		{
			StringBuilder stringBuilder = new StringBuilder();
			List<string> list = settings.knowledgeMatchingSources;
			if (list == null || list.Count == 0)
			{
				list = new List<string> { "prompt" };
			}
			List<Pawn> propertyValue = GetPropertyValue<List<Pawn>>(promptContext, "AllPawns");
			_ = propertyValue?.Count;
			foreach (string item in list)
			{
				if (item.Equals("knowledge", StringComparison.OrdinalIgnoreCase) || item.StartsWith("knowledge_", StringComparison.OrdinalIgnoreCase) || item.StartsWith("knowledge.", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				if (MustacheVariableHelper.IsPawnProperty(item))
				{
					if (propertyValue == null)
					{
						continue;
					}
					foreach (Pawn item2 in propertyValue)
					{
						if (item2 != null && MustacheVariableHelper.TryGetPawnPropertyValue(item, item2, out var value) && !string.IsNullOrEmpty(value))
						{
							if (stringBuilder.Length > 0)
							{
								stringBuilder.AppendLine();
							}
							stringBuilder.Append(value);
						}
					}
					continue;
				}
				string variableValue = GetVariableValue(promptContext, item);
				if (!string.IsNullOrEmpty(variableValue))
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(variableValue);
				}
			}
			return stringBuilder.ToString();
		}

		private static string GetVariableValue(object ctx, string variableName)
		{
			try
			{
				string directContextProperty = GetDirectContextProperty(ctx, variableName);
				if (!string.IsNullOrEmpty(directContextProperty))
				{
					return directContextProperty;
				}
				string text = TryParseScribanVariable(ctx, variableName);
				if (!string.IsNullOrEmpty(text))
				{
					return text;
				}
				return null;
			}
			catch
			{
				return null;
			}
		}

		private static string GetDirectContextProperty(object ctx, string variableName)
		{
			switch (variableName)
			{
			case "dialogue.prompt":
			case "prompt":
				return GetPropertyValue<string>(ctx, "DialoguePrompt");
			case "dialogue.type":
				return GetPropertyValue<string>(ctx, "DialogueType");
			case "dialogue.status":
				return GetPropertyValue<string>(ctx, "DialogueStatus");
			case "pawn.context":
			case "context":
				return GetPropertyValue<string>(ctx, "PawnContext");
			default:
				return null;
			}
		}

		private static string TryParseScribanVariable(object ctx, string variableName)
		{
			try
			{
				Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.GetName().Name == "RimTalk");
				if (assembly == null)
				{
					return null;
				}
				Type type = assembly.GetType("RimTalk.Prompt.ScribanParser") ?? assembly.GetType("RimTalk.Prompt.MustacheParser");
				if (type == null)
				{
					return null;
				}
				string text = "{{" + variableName + "}}";
				MethodInfo methodInfo = type.GetMethod("Render", BindingFlags.Static | BindingFlags.Public) ?? type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public);
				if (methodInfo != null)
				{
					object[] parameters = ((methodInfo.GetParameters().Length < 3) ? new object[2] { text, ctx } : new object[3] { text, ctx, false });
					string text2 = methodInfo.Invoke(null, parameters) as string;
					if (!string.IsNullOrEmpty(text2) && text2 != text)
					{
						return text2;
					}
				}
				return null;
			}
			catch (Exception ex)
			{
				if (Prefs.DevMode)
				{
					Log.Warning("[MemoryPatch] TryParseScribanVariable failed: " + ex.Message);
				}
				return null;
			}
		}

		private static T GetPropertyValue<T>(object obj, string propertyName) where T : class
		{
			if (obj == null)
			{
				return null;
			}
			PropertyInfo property = obj.GetType().GetProperty(propertyName);
			if (property != null)
			{
				return property.GetValue(obj) as T;
			}
			return null;
		}

		private static Pawn GetCurrentPawn(object ctx)
		{
			return GetPropertyValue<Pawn>(ctx, "CurrentPawn");
		}

		private static Pawn GetPawn2(object ctx)
		{
			List<Pawn> propertyValue = GetPropertyValue<List<Pawn>>(ctx, "AllPawns");
			if (propertyValue == null || propertyValue.Count <= 1)
			{
				return null;
			}
			return propertyValue[1];
		}

		private static Map GetMap(object ctx)
		{
			return GetPropertyValue<Map>(ctx, "Map");
		}
	}
	public static class MemoryVariableProvider
	{
		[ThreadStatic]
		private static Dictionary<string, string> _pawnMemoryCache;

		[ThreadStatic]
		private static int _memoryCacheTick;

		private const int MEMORY_CACHE_EXPIRE_TICKS = 120;

		public static string GetPawnMemory(Pawn pawn)
		{
			if (pawn == null)
			{
				return "";
			}
			try
			{
				RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
				FourLayerMemoryComp fourLayerMemoryComp = ThingCompUtility.TryGetComp<FourLayerMemoryComp>((Thing)(object)pawn);
				if (fourLayerMemoryComp != null)
				{
					return GetFourLayerMemories(pawn, fourLayerMemoryComp, settings);
				}
				PawnMemoryComp pawnMemoryComp = ThingCompUtility.TryGetComp<PawnMemoryComp>((Thing)(object)pawn);
				if (pawnMemoryComp != null)
				{
					return PromptNormalizer.Normalize(GetLegacyMemories(pawnMemoryComp, settings));
				}
				return "(No memory component)";
			}
			catch (Exception ex)
			{
				Log.Warning("[MemoryPatch] Error getting pawn memory for " + ((pawn != null) ? ((Entity)pawn).LabelShort : null) + ": " + ex.Message);
				return "";
			}
		}

		private static string GetFourLayerMemories(Pawn pawn, FourLayerMemoryComp comp, RimTalkMemoryPatchSettings settings)
		{
			string thingID = ((Thing)pawn).ThingID;
			TickManager tickManager = Find.TickManager;
			int num = ((tickManager != null) ? tickManager.TicksGame : 0);
			if (_pawnMemoryCache == null || num < _memoryCacheTick || num - _memoryCacheTick > 120)
			{
				_pawnMemoryCache = new Dictionary<string, string>();
				_memoryCacheTick = num;
			}
			if (_pawnMemoryCache.TryGetValue(thingID, out var value))
			{
				return value;
			}
			string currentDialogueContext = GetCurrentDialogueContext();
			string text = UnifiedMemoryInjector.Inject(pawn, currentDialogueContext);
			if (string.IsNullOrEmpty(text))
			{
				text = GetRecentMemories(comp, settings.maxInjectedMemories);
			}
			text = PromptNormalizer.Normalize(text);
			_pawnMemoryCache[thingID] = text;
			return text;
		}

		private static string GetRecentMemories(FourLayerMemoryComp comp, int maxCount)
		{
			List<MemoryEntry> list = new List<MemoryEntry>();
			list.AddRange(comp.SituationalMemories.Take(maxCount / 2));
			list.AddRange(comp.EventLogMemories.Take(maxCount / 2));
			if (list.Count == 0)
			{
				return "(No memories yet)";
			}
			return MemoryFormatter.Format(list.OrderByDescending((MemoryEntry m) => m.GameTick).Take(maxCount).ToList());
		}

		private static string GetLegacyMemories(PawnMemoryComp comp, RimTalkMemoryPatchSettings settings)
		{
			List<MemoryEntry> relevantMemories = comp.GetRelevantMemories(settings.maxInjectedMemories);
			if (relevantMemories == null || relevantMemories.Count == 0)
			{
				return "(No memories yet)";
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num = 1;
			foreach (MemoryEntry item in relevantMemories)
			{
				stringBuilder.AppendLine($"{num}. {item.Content} ({item.AgeString})");
				num++;
			}
			return stringBuilder.ToString().TrimEnd();
		}

		private static string GetCurrentDialogueContext()
		{
			return "";
		}

		public static string GetPawnABM(Pawn pawn)
		{
			if (pawn == null)
			{
				return "";
			}
			try
			{
				return PromptNormalizer.Normalize(UnifiedMemoryInjector.InjectABMOnly(pawn));
			}
			catch (Exception ex)
			{
				Log.Warning("[MemoryPatch] Error getting ABM for " + ((pawn != null) ? ((Entity)pawn).LabelShort : null) + ": " + ex.Message);
				return "";
			}
		}

		public static string GetPawnELS(Pawn pawn)
		{
			if (pawn == null)
			{
				return "";
			}
			try
			{
				FourLayerMemoryComp fourLayerMemoryComp = ThingCompUtility.TryGetComp<FourLayerMemoryComp>((Thing)(object)pawn);
				if (fourLayerMemoryComp == null || fourLayerMemoryComp.EventLogMemories == null || fourLayerMemoryComp.EventLogMemories.Count == 0)
				{
					return "(No ELS memories)";
				}
				return PromptNormalizer.Normalize(FormatMemoryList(fourLayerMemoryComp.EventLogMemories, MemoryLayer.EventLog));
			}
			catch (Exception ex)
			{
				Log.Warning("[MemoryPatch] Error getting ELS for " + ((pawn != null) ? ((Entity)pawn).LabelShort : null) + ": " + ex.Message);
				return "";
			}
		}

		public static string GetPawnCLPA(Pawn pawn)
		{
			if (pawn == null)
			{
				return "";
			}
			try
			{
				FourLayerMemoryComp fourLayerMemoryComp = ThingCompUtility.TryGetComp<FourLayerMemoryComp>((Thing)(object)pawn);
				if (fourLayerMemoryComp == null || fourLayerMemoryComp.ArchiveMemories == null || fourLayerMemoryComp.ArchiveMemories.Count == 0)
				{
					return "(No CLPA memories)";
				}
				return PromptNormalizer.Normalize(FormatMemoryList(fourLayerMemoryComp.ArchiveMemories, MemoryLayer.Archive));
			}
			catch (Exception ex)
			{
				Log.Warning("[MemoryPatch] Error getting CLPA for " + ((pawn != null) ? ((Entity)pawn).LabelShort : null) + ": " + ex.Message);
				return "";
			}
		}

		private static string FormatMemoryList(List<MemoryEntry> memories, MemoryLayer layer)
		{
			if (memories == null || memories.Count == 0)
			{
				return "";
			}
			int count = RimTalkMemoryPatchMod.Settings?.maxInjectedMemories ?? 10;
			return MemoryFormatter.Format(memories.OrderByDescending((MemoryEntry m) => m.GameTick).Take(count).ToList());
		}

		public static string GetPawnMatchELS(Pawn pawn)
		{
			if (pawn == null)
			{
				return "";
			}
			try
			{
				FourLayerMemoryComp fourLayerMemoryComp = ThingCompUtility.TryGetComp<FourLayerMemoryComp>((Thing)(object)pawn);
				if (fourLayerMemoryComp == null || fourLayerMemoryComp.EventLogMemories == null || fourLayerMemoryComp.EventLogMemories.Count == 0)
				{
					return "(No ELS memories)";
				}
				RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
				string currentDialogueContext = GetCurrentDialogueContext();
				string matchedMemoriesForLayer = GetMatchedMemoriesForLayer(pawn, fourLayerMemoryComp, fourLayerMemoryComp.EventLogMemories, MemoryLayer.EventLog, currentDialogueContext, settings?.maxInjectedMemories ?? 10);
				if (string.IsNullOrEmpty(matchedMemoriesForLayer))
				{
					return "(No matched ELS memories)";
				}
				return PromptNormalizer.Normalize(matchedMemoriesForLayer);
			}
			catch (Exception ex)
			{
				Log.Warning("[MemoryPatch] Error getting matchELS for " + ((pawn != null) ? ((Entity)pawn).LabelShort : null) + ": " + ex.Message);
				return "";
			}
		}

		public static string GetPawnMatchCLPA(Pawn pawn)
		{
			if (pawn == null)
			{
				return "";
			}
			try
			{
				FourLayerMemoryComp fourLayerMemoryComp = ThingCompUtility.TryGetComp<FourLayerMemoryComp>((Thing)(object)pawn);
				if (fourLayerMemoryComp == null || fourLayerMemoryComp.ArchiveMemories == null || fourLayerMemoryComp.ArchiveMemories.Count == 0)
				{
					return "(No CLPA memories)";
				}
				RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
				string currentDialogueContext = GetCurrentDialogueContext();
				string matchedMemoriesForLayer = GetMatchedMemoriesForLayer(pawn, fourLayerMemoryComp, fourLayerMemoryComp.ArchiveMemories, MemoryLayer.Archive, currentDialogueContext, settings?.maxInjectedMemories ?? 10);
				if (string.IsNullOrEmpty(matchedMemoriesForLayer))
				{
					return "(No matched CLPA memories)";
				}
				return PromptNormalizer.Normalize(matchedMemoriesForLayer);
			}
			catch (Exception ex)
			{
				Log.Warning("[MemoryPatch] Error getting matchCLPA for " + ((pawn != null) ? ((Entity)pawn).LabelShort : null) + ": " + ex.Message);
				return "";
			}
		}

		private static string GetMatchedMemoriesForLayer(Pawn pawn, FourLayerMemoryComp comp, List<MemoryEntry> memories, MemoryLayer layer, string context, int maxCount)
		{
			if (memories == null || memories.Count == 0)
			{
				return null;
			}
			DynamicMemoryInjection.InjectMemoriesWithDetails(comp, context, maxCount, out var scores, layer);
			if (scores == null || scores.Count == 0)
			{
				return null;
			}
			List<MemoryEntry> list = (from s in scores
				orderby s.TotalScore descending
				select s.Memory).ToList();
			if (list.Count == 0)
			{
				return null;
			}
			return MemoryFormatter.Format(list);
		}
	}
	public static class MustacheVariableHelper
	{
		private static Dictionary<string, List<(string name, string description)>> _cachedVariables;

		private static HashSet<string> _cachedPawnProperties;

		private static Assembly _rimTalkAssembly;

		private static Assembly GetRimTalkAssembly()
		{
			if (_rimTalkAssembly == null)
			{
				_rimTalkAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.GetName().Name == "RimTalk");
			}
			return _rimTalkAssembly;
		}

		public static Dictionary<string, List<(string name, string description)>> GetBuiltinVariables()
		{
			if (_cachedVariables != null)
			{
				return _cachedVariables;
			}
			Assembly rimTalkAssembly = GetRimTalkAssembly();
			if (rimTalkAssembly == null)
			{
				_cachedVariables = GetFallbackVariables();
				return _cachedVariables;
			}
			try
			{
				MethodInfo methodInfo = rimTalkAssembly.GetType("RimTalk.Prompt.VariableDefinitions")?.GetMethod("GetScribanVariables", BindingFlags.Static | BindingFlags.Public);
				if (methodInfo != null)
				{
					_cachedVariables = ConvertDictionaryResult(methodInfo.Invoke(null, null));
					if (_cachedVariables.Count > 0)
					{
						return _cachedVariables;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warning("[MemoryPatch] Failed to get builtin variables: " + ex.Message);
			}
			_cachedVariables = GetFallbackVariables();
			return _cachedVariables;
		}

		public static Dictionary<string, List<(string name, string description, bool isPawnProperty)>> GetCategorizedMatchingSources()
		{
			Dictionary<string, List<(string, string, bool)>> dictionary = new Dictionary<string, List<(string, string, bool)>>();
			foreach (KeyValuePair<string, List<(string, string)>> builtinVariable in GetBuiltinVariables())
			{
				List<(string, string, bool)> list = new List<(string, string, bool)>();
				foreach (var item2 in builtinVariable.Value)
				{
					if (!(item2.Item1 == "json.format") && !(item2.Item1 == "chat.history") && !item2.Item1.StartsWith("#") && !(item2.Item1 == "knowledge") && !item2.Item1.StartsWith("knowledge."))
					{
						bool flag = item2.Item1.StartsWith("pawn.") && !item2.Item1.StartsWith("pawn.memory");
						string text;
						if (!flag)
						{
							(text, _) = item2;
						}
						else
						{
							text = item2.Item1.Substring(5);
						}
						string item = text;
						list.Add((item, item2.Item2, flag));
					}
				}
				if (list.Count > 0)
				{
					dictionary[builtinVariable.Key] = list;
				}
			}
			return dictionary;
		}

		public static bool IsPawnProperty(string propertyName)
		{
			if (string.IsNullOrEmpty(propertyName))
			{
				return false;
			}
			if (_cachedPawnProperties == null)
			{
				_cachedPawnProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (KeyValuePair<string, List<(string, string)>> builtinVariable in GetBuiltinVariables())
				{
					foreach (var item in builtinVariable.Value)
					{
						if (item.Item1.StartsWith("pawn.") && !item.Item1.StartsWith("pawn.memory"))
						{
							_cachedPawnProperties.Add(item.Item1.Substring(5));
						}
					}
				}
			}
			return _cachedPawnProperties.Contains(propertyName);
		}

		public static bool TryGetPawnPropertyValue(string propertyName, Pawn pawn, out string value)
		{
			value = null;
			if (pawn == null || string.IsNullOrEmpty(propertyName))
			{
				return false;
			}
			Assembly rimTalkAssembly = GetRimTalkAssembly();
			if (rimTalkAssembly == null)
			{
				return false;
			}
			try
			{
				Type type = rimTalkAssembly.GetType("RimTalk.API.ContextHookRegistry");
				if (type != null)
				{
					MethodInfo method = type.GetMethod("TryGetPawnVariable", BindingFlags.Static | BindingFlags.Public);
					if (method != null)
					{
						object[] array = new object[3] { propertyName, pawn, null };
						if ((bool)method.Invoke(null, array))
						{
							value = array[2] as string;
							if (!string.IsNullOrEmpty(value))
							{
								return true;
							}
						}
					}
				}
				Type type2 = rimTalkAssembly.GetType("RimTalk.Prompt.ScribanParser");
				Type type3 = rimTalkAssembly.GetType("RimTalk.Prompt.PromptContext");
				if (type2 != null && type3 != null)
				{
					object obj = Activator.CreateInstance(type3, pawn, null);
					string text = "{{pawn." + propertyName + "}}";
					MethodInfo method2 = type2.GetMethod("Render", BindingFlags.Static | BindingFlags.Public);
					if (method2 != null)
					{
						string text2 = method2.Invoke(null, new object[3] { text, obj, false }) as string;
						if (!string.IsNullOrEmpty(text2) && text2 != text)
						{
							value = text2;
							return true;
						}
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				if (Prefs.DevMode)
				{
					Log.Warning("[MemoryPatch] TryGetPawnPropertyValue failed: " + ex.Message);
				}
				return false;
			}
		}

		public static void ClearCache()
		{
			_cachedVariables = null;
			_cachedPawnProperties = null;
			_rimTalkAssembly = null;
		}

		private static Dictionary<string, List<(string, string)>> ConvertDictionaryResult(object result)
		{
			Dictionary<string, List<(string, string)>> dictionary = new Dictionary<string, List<(string, string)>>();
			if (result == null)
			{
				return dictionary;
			}
			try
			{
				if (result is Dictionary<string, List<(string, string)>> result2)
				{
					return result2;
				}
				if (result is IDictionary dictionary2)
				{
					foreach (object key in dictionary2.Keys)
					{
						string text = key?.ToString() ?? "";
						if (string.IsNullOrEmpty(text) || !(dictionary2[key] is IEnumerable enumerable))
						{
							continue;
						}
						List<(string, string)> list = new List<(string, string)>();
						foreach (object item2 in enumerable)
						{
							Type type = item2.GetType();
							string text2 = type.GetField("Item1")?.GetValue(item2)?.ToString() ?? "";
							string item = type.GetField("Item2")?.GetValue(item2)?.ToString() ?? "";
							if (!string.IsNullOrEmpty(text2))
							{
								list.Add((text2, item));
							}
						}
						if (list.Count > 0)
						{
							dictionary[text] = list;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warning("[MemoryPatch] Failed to convert result: " + ex.Message);
			}
			return dictionary;
		}

		private static Dictionary<string, List<(string, string)>> GetFallbackVariables()
		{
			return new Dictionary<string, List<(string, string)>>
			{
				["Context"] = new List<(string, string)>
				{
					("prompt", "Full dialogue prompt"),
					("context", "Pawn context"),
					("ctx.DialogueType", "Dialogue type"),
					("ctx.DialogueStatus", "Dialogue status")
				},
				["Pawn Shorthands"] = new List<(string, string)>
				{
					("pawn.name", "Name"),
					("pawn.backstory", "Backstory"),
					("pawn.traits", "Traits"),
					("pawn.mood", "Mood"),
					("pawn.job", "Current job"),
					("pawn.health", "Health"),
					("pawn.skills", "Skills")
				},
				["System"] = new List<(string, string)>
				{
					("hour", "Current hour"),
					("season", "Current season"),
					("weather", "Current weather")
				}
			};
		}

		public static List<(string name, string category, string description)> GetFlattenedVariables()
		{
			List<(string, string, string)> list = new List<(string, string, string)>();
			foreach (KeyValuePair<string, List<(string, string)>> builtinVariable in GetBuiltinVariables())
			{
				foreach (var item in builtinVariable.Value)
				{
					list.Add((item.Item1, builtinVariable.Key, item.Item2));
				}
			}
			return list;
		}

		public static List<(string propertyName, string description, bool isPawnProperty)> GetMatchingPropertyCategories()
		{
			List<(string, string, bool)> list = new List<(string, string, bool)>();
			foreach (KeyValuePair<string, List<(string, string, bool)>> categorizedMatchingSource in GetCategorizedMatchingSources())
			{
				list.AddRange(categorizedMatchingSource.Value);
			}
			return list;
		}

		public static List<(string name, string description)> GetMatchingSourceVariables()
		{
			List<(string, string)> list = new List<(string, string)>();
			foreach (KeyValuePair<string, List<(string, string)>> builtinVariable in GetBuiltinVariables())
			{
				foreach (var item in builtinVariable.Value)
				{
					if (!item.Item1.StartsWith("#") && item.Item1 != "json.format" && item.Item1 != "chat.history")
					{
						list.Add(item);
					}
				}
			}
			return list;
		}

		public static List<(string name, string description)> GetExtensionPawnVariables()
		{
			List<(string, string)> list = new List<(string, string)>();
			Assembly rimTalkAssembly = GetRimTalkAssembly();
			if (rimTalkAssembly == null)
			{
				return list;
			}
			try
			{
				MethodInfo methodInfo = rimTalkAssembly.GetType("RimTalk.API.ContextHookRegistry")?.GetMethod("GetAllCustomVariables", BindingFlags.Static | BindingFlags.Public);
				if (methodInfo != null && methodInfo.Invoke(null, null) is IEnumerable enumerable)
				{
					foreach (object item2 in enumerable)
					{
						Type type = item2.GetType();
						string text = type.GetField("Item1")?.GetValue(item2)?.ToString() ?? "";
						string text2 = type.GetField("Item4")?.GetValue(item2)?.ToString() ?? "";
						string item = type.GetField("Item3")?.GetValue(item2)?.ToString() ?? "";
						if (text2 == "Pawn" && !string.IsNullOrEmpty(text))
						{
							if (text.StartsWith("pawn.", StringComparison.OrdinalIgnoreCase))
							{
								text = text.Substring(5);
							}
							list.Add((text, item));
						}
					}
				}
			}
			catch
			{
			}
			return list;
		}

		public static List<(string name, string description)> GetExtensionContextVariables()
		{
			List<(string, string)> list = new List<(string, string)>();
			Assembly rimTalkAssembly = GetRimTalkAssembly();
			if (rimTalkAssembly == null)
			{
				return list;
			}
			try
			{
				MethodInfo methodInfo = rimTalkAssembly.GetType("RimTalk.API.ContextHookRegistry")?.GetMethod("GetAllCustomVariables", BindingFlags.Static | BindingFlags.Public);
				if (methodInfo != null && methodInfo.Invoke(null, null) is IEnumerable enumerable)
				{
					foreach (object item2 in enumerable)
					{
						Type type = item2.GetType();
						string text = type.GetField("Item1")?.GetValue(item2)?.ToString() ?? "";
						string text2 = type.GetField("Item4")?.GetValue(item2)?.ToString() ?? "";
						string item = type.GetField("Item3")?.GetValue(item2)?.ToString() ?? "";
						if (text2 == "Context" && !string.IsNullOrEmpty(text))
						{
							list.Add((text, item));
						}
					}
				}
			}
			catch
			{
			}
			return list;
		}
	}
	[StaticConstructorOnStartup]
	public static class RimTalkAPIIntegration
	{
		private const string MOD_ID = "RimTalk.MemoryPatch";

		private const string ENTRY_NAME = "Memory & Knowledge Context";

		private static bool _initialized;

		private static bool _apiAvailable;

		private static Assembly _rimTalkAssembly;

		private static Type _promptAPIType;

		private static Type _promptEntryType;

		private static Type _promptRoleType;

		private static Type _promptPositionType;

		private static Type _promptContextType;

		private static readonly string[] SniffingKeywords;

		private const string CHAT_HISTORY_ENTRY_NAME = "Chat History";

		public static bool IsUsingNewAPI => _apiAvailable;

		static RimTalkAPIIntegration()
		{
			_initialized = false;
			_apiAvailable = false;
			SniffingKeywords = new string[15]
			{
				"pawn.memory", "p.memory", "pawn.ABM", "p.ABM", "pawn.ELS", "p.ELS", "pawn.CLPA", "p.CLPA", "{{knowledge}}", "{{ knowledge }}",
				"knowledge_grouped", "knowledge_rules", "knowledge_lore", "knowledge_status", "knowledge_history"
			};
			LongEventHandler.ExecuteWhenFinished((Action)Initialize);
		}

		private static void Initialize()
		{
			if (_initialized)
			{
				return;
			}
			_initialized = true;
			try
			{
				if (!DetectNewAPI())
				{
					Log.Message("[MemoryPatch] New RimTalk API not detected, using legacy Harmony patches");
					return;
				}
				_apiAvailable = true;
				RegisterVariables();
				RegisterPromptEntry();
				Log.Message("[MemoryPatch] ✓ Integrated via RimTalk API v4.0+");
				Log.Message("[MemoryPatch]   - Registered {{pawn.memory}} variable (combined memories)");
				Log.Message("[MemoryPatch]   - Registered {{pawn.ABM}} variable (active buffer - raw)");
				Log.Message("[MemoryPatch]   - Registered {{pawn.ELS}} variable (event log - raw)");
				Log.Message("[MemoryPatch]   - Registered {{pawn.CLPA}} variable (archive - raw)");
				Log.Message("[MemoryPatch]   - Registered {{pawn.matchELS}} variable (event log - matched)");
				Log.Message("[MemoryPatch]   - Registered {{pawn.matchCLPA}} variable (archive - matched)");
				Log.Message("[MemoryPatch]   - Registered {{knowledge}} variable");
				Log.Message("[MemoryPatch]   - Added PromptEntry: Memory & Knowledge Context");
			}
			catch (Exception ex)
			{
				Log.Error("[MemoryPatch] API integration failed: " + ex.Message);
				Log.Message("[MemoryPatch] Falling back to legacy Harmony patches");
				_apiAvailable = false;
			}
		}

		private static bool DetectNewAPI()
		{
			_rimTalkAssembly = GetRimTalkAssembly();
			if (_rimTalkAssembly == null)
			{
				Log.Warning("[MemoryPatch] RimTalk assembly not found");
				return false;
			}
			_promptAPIType = _rimTalkAssembly.GetType("RimTalk.API.RimTalkPromptAPI");
			_promptEntryType = _rimTalkAssembly.GetType("RimTalk.Prompt.PromptEntry");
			_promptRoleType = _rimTalkAssembly.GetType("RimTalk.Prompt.PromptRole");
			_promptPositionType = _rimTalkAssembly.GetType("RimTalk.Prompt.PromptPosition");
			_promptContextType = _rimTalkAssembly.GetType("RimTalk.Prompt.PromptContext") ?? _rimTalkAssembly.GetType("RimTalk.Prompt.MustacheContext");
			if (_promptAPIType == null)
			{
				Log.Message("[MemoryPatch] RimTalk.API.RimTalkPromptAPI not found - old RimTalk version");
				return false;
			}
			MethodInfo method = _promptAPIType.GetMethod("RegisterPawnVariable");
			MethodInfo method2 = _promptAPIType.GetMethod("RegisterContextVariable");
			MethodInfo method3 = _promptAPIType.GetMethod("AddPromptEntry");
			if (method == null || method2 == null || method3 == null)
			{
				Log.Warning("[MemoryPatch] RimTalk API methods not found");
				return false;
			}
			Log.Message("[MemoryPatch] Detected RimTalk API v4.0+ with Scriban/Mustache support");
			return true;
		}

		private static Assembly GetRimTalkAssembly()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				if (assembly.GetName().Name == "RimTalk")
				{
					return assembly;
				}
			}
			return null;
		}

		private static void RegisterVariables()
		{
			MethodInfo method = _promptAPIType.GetMethod("RegisterPawnVariable");
			if (method != null)
			{
				try
				{
					Func<Pawn, string> func = MemoryVariableProvider.GetPawnMemory;
					method.Invoke(null, new object[5] { "RimTalk.MemoryPatch", "memory", func, "Character's personal memories and experiences", 100 });
					if (Prefs.DevMode)
					{
						Log.Message("[MemoryPatch] ✓ Registered {{pawn.memory}} variable");
					}
				}
				catch (Exception ex)
				{
					Log.Warning("[MemoryPatch] Failed to register pawn.memory: " + ex.Message);
				}
				try
				{
					Func<Pawn, string> func2 = MemoryVariableProvider.GetPawnABM;
					method.Invoke(null, new object[5] { "RimTalk.MemoryPatch", "ABM", func2, "Character's active buffer memories (recent conversations)", 100 });
					if (Prefs.DevMode)
					{
						Log.Message("[MemoryPatch] ✓ Registered {{pawn.ABM}} variable");
					}
				}
				catch (Exception ex2)
				{
					Log.Warning("[MemoryPatch] Failed to register pawn.ABM: " + ex2.Message);
				}
				try
				{
					Func<Pawn, string> func3 = MemoryVariableProvider.GetPawnELS;
					method.Invoke(null, new object[5] { "RimTalk.MemoryPatch", "ELS", func3, "Character's event log summary (mid-term memories)", 100 });
					if (Prefs.DevMode)
					{
						Log.Message("[MemoryPatch] ✓ Registered {{pawn.ELS}} variable");
					}
				}
				catch (Exception ex3)
				{
					Log.Warning("[MemoryPatch] Failed to register pawn.ELS: " + ex3.Message);
				}
				try
				{
					Func<Pawn, string> func4 = MemoryVariableProvider.GetPawnCLPA;
					method.Invoke(null, new object[5] { "RimTalk.MemoryPatch", "CLPA", func4, "Character's persona archive (long-term memories)", 100 });
					if (Prefs.DevMode)
					{
						Log.Message("[MemoryPatch] ✓ Registered {{pawn.CLPA}} variable");
					}
				}
				catch (Exception ex4)
				{
					Log.Warning("[MemoryPatch] Failed to register pawn.CLPA: " + ex4.Message);
				}
				try
				{
					Func<Pawn, string> func5 = MemoryVariableProvider.GetPawnMatchELS;
					method.Invoke(null, new object[5] { "RimTalk.MemoryPatch", "matchELS", func5, "Context-matched event log memories (mid-term)", 100 });
					if (Prefs.DevMode)
					{
						Log.Message("[MemoryPatch] ✓ Registered {{pawn.matchELS}} variable");
					}
				}
				catch (Exception ex5)
				{
					Log.Warning("[MemoryPatch] Failed to register pawn.matchELS: " + ex5.Message);
				}
				try
				{
					Func<Pawn, string> func6 = MemoryVariableProvider.GetPawnMatchCLPA;
					method.Invoke(null, new object[5] { "RimTalk.MemoryPatch", "matchCLPA", func6, "Context-matched archive memories (long-term)", 100 });
					if (Prefs.DevMode)
					{
						Log.Message("[MemoryPatch] ✓ Registered {{pawn.matchCLPA}} variable");
					}
				}
				catch (Exception ex6)
				{
					Log.Warning("[MemoryPatch] Failed to register pawn.matchCLPA: " + ex6.Message);
				}
			}
			MethodInfo method2 = _promptAPIType.GetMethod("RegisterContextVariable");
			if (method2 != null)
			{
				RegisterOneContextVariable(method2, "knowledge", KnowledgeVariableProvider.GetMatchedKnowledge, "World knowledge matched to conversation context", 100);
				RegisterOneContextVariable(method2, "knowledge_grouped", KnowledgeVariableProvider.GetGroupedKnowledge, "World knowledge grouped by category headers", 101);
				RegisterOneContextVariable(method2, "knowledge_rules", KnowledgeVariableProvider.GetKnowledgeRules, "Knowledge: rules and instructions", 102);
				RegisterOneContextVariable(method2, "knowledge_lore", KnowledgeVariableProvider.GetKnowledgeLore, "Knowledge: world lore and background", 103);
				RegisterOneContextVariable(method2, "knowledge_status", KnowledgeVariableProvider.GetKnowledgeStatus, "Knowledge: pawn/colonist status", 104);
				RegisterOneContextVariable(method2, "knowledge_history", KnowledgeVariableProvider.GetKnowledgeHistory, "Knowledge: historical events", 105);
				RegisterOneContextVariable(method2, "knowledge_other", KnowledgeVariableProvider.GetKnowledgeOther, "Knowledge: uncategorized", 106);
			}
		}

		private static void RegisterOneContextVariable(MethodInfo registerMethod, string name, Func<object, string> provider, string description, int priority)
		{
			try
			{
				registerMethod.Invoke(null, new object[5] { "RimTalk.MemoryPatch", name, provider, description, priority });
				if (Prefs.DevMode)
				{
					Log.Message("[MemoryPatch] ✓ Registered {{" + name + "}} variable");
				}
			}
			catch (Exception ex)
			{
				Log.Warning("[MemoryPatch] Failed to register " + name + ": " + ex.Message);
			}
		}

		private static void RegisterPromptEntry()
		{
			try
			{
				string deterministicId = GetDeterministicId("RimTalk.MemoryPatch", "Memory & Knowledge Context");
				MethodInfo method = _promptAPIType.GetMethod("GetActivePreset");
				object obj = null;
				if (method != null)
				{
					obj = method.Invoke(null, null);
					if (obj != null)
					{
						MethodInfo method2 = obj.GetType().GetMethod("GetEntry");
						if (method2 != null)
						{
							object obj2 = method2.Invoke(obj, new object[1] { deterministicId });
							if (obj2 != null)
							{
								SetProperty(obj2, "Content", GetMemoryEntryContent());
								Log.Message("[MemoryPatch] ✓ Updated existing PromptEntry: Memory & Knowledge Context");
								DisableChatHistoryIfEnabled(obj);
								return;
							}
						}
						PromptPreset val = (PromptPreset)((obj is PromptPreset) ? obj : null);
						if (val != null && val.Entries != null)
						{
							foreach (PromptEntry entry in val.Entries)
							{
								string content = entry.Content;
								if (string.IsNullOrEmpty(content))
								{
									continue;
								}
								string[] sniffingKeywords = SniffingKeywords;
								foreach (string text in sniffingKeywords)
								{
									if (content.Contains(text))
									{
										Log.Message("[MemoryPatch] Detected custom memory variable '" + text + "' in active preset. Skipping auto-injection.");
										return;
									}
								}
							}
						}
					}
				}
				object obj3 = Activator.CreateInstance(_promptEntryType);
				if (obj3 == null)
				{
					Log.Warning("[MemoryPatch] Failed to create PromptEntry instance");
					return;
				}
				SetProperty(obj3, "SourceModId", "RimTalk.MemoryPatch");
				SetProperty(obj3, "Name", "Memory & Knowledge Context");
				SetProperty(obj3, "Content", GetMemoryEntryContent());
				SetProperty(obj3, "Enabled", true);
				if (_promptRoleType != null)
				{
					object value = Enum.Parse(_promptRoleType, "System");
					SetProperty(obj3, "Role", value);
				}
				if (_promptPositionType != null)
				{
					object value2 = Enum.Parse(_promptPositionType, "Relative");
					SetProperty(obj3, "Position", value2);
				}
				MethodInfo method3 = _promptAPIType.GetMethod("InsertPromptEntryAfterName");
				if (method3 != null)
				{
					object obj4 = method3.Invoke(null, new object[2] { obj3, "Chat History" });
					if (obj4 is bool)
					{
						if ((bool)obj4)
						{
							Log.Message("[MemoryPatch] ✓ Inserted PromptEntry after 'Chat History': Memory & Knowledge Context");
						}
						else
						{
							Log.Message("[MemoryPatch] ✓ Added PromptEntry at end ('Chat History' not found): Memory & Knowledge Context");
						}
						if (obj != null)
						{
							DisableChatHistoryIfEnabled(obj);
						}
					}
					return;
				}
				MethodInfo method4 = _promptAPIType.GetMethod("AddPromptEntry");
				if (method4 != null)
				{
					object obj5 = method4.Invoke(null, new object[1] { obj3 });
					bool flag = default(bool);
					int num;
					if (obj5 is bool)
					{
						flag = (bool)obj5;
						num = 1;
					}
					else
					{
						num = 0;
					}
					if (((uint)num & (flag ? 1u : 0u)) != 0)
					{
						Log.Message("[MemoryPatch] ✓ Added PromptEntry: Memory & Knowledge Context");
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warning("[MemoryPatch] Failed to register PromptEntry: " + ex.Message);
			}
		}

		private static void DisableChatHistoryIfEnabled(object preset)
		{
			try
			{
				if (preset == null)
				{
					return;
				}
				MethodInfo method = preset.GetType().GetMethod("FindEntryIdByName");
				if (method == null)
				{
					if (Prefs.DevMode)
					{
						Log.Warning("[MemoryPatch] FindEntryIdByName method not found");
					}
					return;
				}
				string text = method.Invoke(preset, new object[1] { "Chat History" }) as string;
				if (string.IsNullOrEmpty(text))
				{
					if (Prefs.DevMode)
					{
						Log.Message("[MemoryPatch] 'Chat History' entry not found in preset");
					}
					return;
				}
				MethodInfo method2 = preset.GetType().GetMethod("GetEntry");
				if (method2 == null)
				{
					if (Prefs.DevMode)
					{
						Log.Warning("[MemoryPatch] GetEntry method not found");
					}
					return;
				}
				object obj = method2.Invoke(preset, new object[1] { text });
				if (obj == null)
				{
					if (Prefs.DevMode)
					{
						Log.Warning("[MemoryPatch] Chat History entry is null");
					}
					return;
				}
				PropertyInfo property = obj.GetType().GetProperty("Enabled");
				FieldInfo field = obj.GetType().GetField("Enabled");
				bool flag = false;
				if (property != null)
				{
					flag = (bool)property.GetValue(obj);
				}
				else
				{
					if (!(field != null))
					{
						if (Prefs.DevMode)
						{
							Log.Warning("[MemoryPatch] Enabled property/field not found on PromptEntry");
						}
						return;
					}
					flag = (bool)field.GetValue(obj);
				}
				if (flag)
				{
					if (property != null)
					{
						property.SetValue(obj, false);
					}
					else if (field != null)
					{
						field.SetValue(obj, false);
					}
					Log.Message("[MemoryPatch] ✓ Disabled 'Chat History' to avoid conflict with Memory & Knowledge injection");
				}
				else if (Prefs.DevMode)
				{
					Log.Message("[MemoryPatch] 'Chat History' is already disabled");
				}
			}
			catch (Exception ex)
			{
				Log.Warning("[MemoryPatch] Failed to disable Chat History: " + ex.Message);
				if (Prefs.DevMode)
				{
					Log.Warning("[MemoryPatch] Stack trace: " + ex.StackTrace);
				}
			}
		}

		private static string GetMemoryEntryContent()
		{
			return "---\r\n# Memory Context\r\n{{-for p in pawns }}\r\n## {{ p.name }}'s Memories:\r\n{{ p.memory }}\r\n{{- end }}\r\n\r\n# World Knowledge:\r\n{{knowledge}}\r\n---";
		}

		private static void SetProperty(object obj, string propertyName, object value)
		{
			PropertyInfo property = obj.GetType().GetProperty(propertyName);
			if (property != null && property.CanWrite)
			{
				property.SetValue(obj, value);
				return;
			}
			FieldInfo field = obj.GetType().GetField(propertyName);
			if (field != null)
			{
				field.SetValue(obj, value);
			}
		}

		public static void Cleanup()
		{
			if (!_apiAvailable)
			{
				return;
			}
			try
			{
				string deterministicId = GetDeterministicId("RimTalk.MemoryPatch", "Memory & Knowledge Context");
				MethodInfo methodInfo = _promptAPIType?.GetMethod("RemovePromptEntry");
				if (methodInfo != null)
				{
					methodInfo.Invoke(null, new object[1] { deterministicId });
					Log.Message("[MemoryPatch] Removed PromptEntry: " + deterministicId);
				}
				MethodInfo methodInfo2 = _promptAPIType?.GetMethod("UnregisterAllHooks");
				if (methodInfo2 != null)
				{
					methodInfo2.Invoke(null, new object[1] { "RimTalk.MemoryPatch" });
				}
				Log.Message("[MemoryPatch] Cleaned up RimTalk API registrations");
			}
			catch (Exception ex)
			{
				Log.Warning("[MemoryPatch] Cleanup failed: " + ex.Message);
			}
		}

		private static string GetDeterministicId(string modId, string name)
		{
			if (_promptEntryType != null)
			{
				MethodInfo method = _promptEntryType.GetMethod("GenerateDeterministicId", BindingFlags.Static | BindingFlags.Public);
				if (method != null)
				{
					try
					{
						if (method.Invoke(null, new object[2] { modId, name }) is string result)
						{
							return result;
						}
					}
					catch
					{
					}
				}
			}
			string text = modId.Replace(".", "_").Replace("-", "_").Replace(" ", "_");
			string text2 = name.Replace(" ", "_").Replace("-", "_");
			return "mod_" + text + "_" + text2;
		}
	}
}
namespace RimTalk.Memory.AI
{
	public static class AIClientFactory
	{
		public static List<ApiConfig> GetCurrentCandidateConfigs()
		{
			RimTalkMemoryPatchSettings settings = RimTalkMemoryPatchMod.Settings;
			if (settings == null)
			{
				return new List<ApiConfig>();
			}
			if (settings.UseRimTalkAIConfig)
			{
				return RimTalkApiConfigGetter.GetRimTalkApiConfigs();
			}
			List<ApiConfig> list = new List<ApiConfig>();
			if (settings.ApiConfigs != null)
			{
				foreach (ApiConfig apiConfig in settings.ApiConfigs)
				{
					if (apiConfig != null && apiConfig.IsEnabled && apiConfig.IsValid)
					{
						list.Add(apiConfig);
					}
				}
			}
			return list;
		}

		public static IAIClient BuildClient(ApiConfig config)
		{
			if (config == null)
			{
				return null;
			}
			if (config.Provider != AIProvider.Player2)
			{
				return new OpenAIClient(config);
			}
			return new Player2Client(config);
		}

		public static string GetConfigFingerprint(ApiConfig config)
		{
			if (config == null)
			{
				return "null";
			}
			return $"{config.Provider}|{config.ApiKey}|{config.EffectiveModelName}|{config.URL}";
		}
	}
	public enum AIProvider
	{
		Google,
		OpenAI,
		DeepSeek,
		Grok,
		GLM,
		GLMCoding,
		AlibabaIntl,
		AlibabaCN,
		Player2,
		Custom,
		OpenRouter,
		Claude,
		Moonshot
	}
	public static class AIProviderRegistry
	{
		private static readonly Dictionary<AIProvider, string> _dictProviderToURL = new Dictionary<AIProvider, string>
		{
			{
				AIProvider.Google,
				"https://generativelanguage.googleapis.com/v1beta/openai/chat/completions"
			},
			{
				AIProvider.OpenAI,
				"https://api.openai.com/v1/chat/completions"
			},
			{
				AIProvider.DeepSeek,
				"https://api.deepseek.com/v1/chat/completions"
			},
			{
				AIProvider.Grok,
				"https://api.x.ai/v1/chat/completions"
			},
			{
				AIProvider.GLM,
				"https://api.z.ai/api/paas/v4/chat/completions"
			},
			{
				AIProvider.GLMCoding,
				"https://api.z.ai/api/coding/paas/v4/chat/completions"
			},
			{
				AIProvider.AlibabaIntl,
				"https://dashscope-intl.aliyuncs.com/compatible-mode/v1/chat/completions"
			},
			{
				AIProvider.AlibabaCN,
				"https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions"
			},
			{
				AIProvider.Player2,
				"https://api.player2.game/v1/chat/completions"
			},
			{
				AIProvider.OpenRouter,
				"https://openrouter.ai/api/v1/chat/completions"
			},
			{
				AIProvider.Claude,
				"https://api.anthropic.com/v1/chat/completions"
			},
			{
				AIProvider.Moonshot,
				"https://api.moonshot.ai/v1/chat/completions"
			}
		};

		private static readonly Dictionary<AIProvider, Dictionary<string, string>> _dictProviderToHeaders = new Dictionary<AIProvider, Dictionary<string, string>>
		{
			{
				AIProvider.OpenRouter,
				new Dictionary<string, string>
				{
					{ "HTTP-Referer", "https://github.com/sanguodxj-byte/RimTalk-ExpandMemory" },
					{ "X-Title", "RimTalk-ExpandMemory" }
				}
			},
			{
				AIProvider.Claude,
				new Dictionary<string, string> { { "anthropic-version", "2023-06-01" } }
			}
		};

		public static string GetEndpointUrl(this AIProvider p)
		{
			if (!_dictProviderToURL.TryGetValue(p, out var value))
			{
				return null;
			}
			return value;
		}

		public static Dictionary<string, string> GetExtraHeaders(this AIProvider p)
		{
			if (!_dictProviderToHeaders.TryGetValue(p, out var value))
			{
				return null;
			}
			return value;
		}
	}
	public class AIService : GameComponent
	{
		private static AIService _instance;

		private readonly Queue<(string prompt, Action<string> callback, Action dispose)> _aiRequestQueue = new Queue<(string, Action<string>, Action)>();

		private float _lastActiveTime;

		private const float RequestInterval = 10f;

		private readonly ClientPool _clientPool = new ClientPool();

		private static readonly ClientPool _fallbackClientPool = new ClientPool();

		public AIService(Game game)
		{
			_instance = this;
		}

		public static void EnqueueAIRequest(string prompt, Action<string> callback, Action dispose = null)
		{
			_instance?._aiRequestQueue.Enqueue((prompt, callback, dispose));
		}

		public static async Task<bool> ValidateAIConfigAsync()
		{
			return await (_instance?._clientPool ?? _fallbackClientPool).ValidateAsync();
		}

		public static void ResetClientPool()
		{
			_instance?._clientPool.Reset();
			_fallbackClientPool.Reset();
		}

		public override void GameComponentUpdate()
		{
			ProcessAIRequestQueue();
		}

		private void ProcessAIRequestQueue()
		{
			if (_aiRequestQueue.Count != 0 && !(RealTime.LastRealTime - _lastActiveTime < 10f))
			{
				(string, Action<string>, Action) task = _aiRequestQueue.Dequeue();
				ExecuteTask(task);
				_lastActiveTime = RealTime.LastRealTime;
			}
		}

		private async void ExecuteTask((string prompt, Action<string> callback, Action dispose) task)
		{
			try
			{
				Payload payload = await ((IAIClient)_clientPool).GetChatCompletionAsync(task.prompt);
				if (payload == null)
				{
					MessageUtil.MessageAndError("[RimTalk.Memory.AI] 请求失败，Get a empty payLoad.");
					return;
				}
				if (RimTalkMemoryPatchMod.Settings.EnableAILog)
				{
					Log.Message(payload.ToString());
				}
				if (!payload.IsValid)
				{
					MessageUtil.MessageAndError("[RimTalk.Memory.AI] 请求失败，Get a error payLoad.");
				}
				else
				{
					task.callback?.Invoke(Sanitize(payload.Response));
				}
			}
			finally
			{
				task.dispose?.Invoke();
			}
		}

		private static string Sanitize(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}
			StringBuilder stringBuilder = null;
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
				{
					stringBuilder?.Append(c).Append(text[i + 1]);
					i++;
					continue;
				}
				if (XmlConvert.IsXmlChar(c))
				{
					stringBuilder?.Append(c);
					continue;
				}
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder(text.Length).Append(text, 0, i);
				}
				stringBuilder.Append(' ');
			}
			return stringBuilder?.ToString() ?? text;
		}
	}
	public class ApiConfig : IExposable
	{
		public bool IsEnabled = true;

		public AIProvider Provider = AIProvider.Player2;

		public string ApiKey;

		public string CustomUrl;

		public string CustomModelName;

		public string URL
		{
			get
			{
				if (Provider == AIProvider.Custom)
				{
					if (string.IsNullOrWhiteSpace(CustomUrl))
					{
						return string.Empty;
					}
					string text = CustomUrl.Trim();
					if (!text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !text.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
					{
						text = "http://" + text;
					}
					return text;
				}
				return Provider.GetEndpointUrl() ?? string.Empty;
			}
		}

		public string EffectiveModelName
		{
			get
			{
				if (string.IsNullOrWhiteSpace(CustomModelName))
				{
					if (Provider != AIProvider.Custom)
					{
						return string.Empty;
					}
					return "default";
				}
				return CustomModelName.Trim();
			}
		}

		public bool IsValid
		{
			get
			{
				if (!IsEnabled)
				{
					return false;
				}
				if (Provider == AIProvider.Player2)
				{
					return true;
				}
				string uRL = URL;
				if (string.IsNullOrWhiteSpace(uRL) || !Uri.IsWellFormedUriString(uRL, UriKind.Absolute))
				{
					return false;
				}
				if (string.IsNullOrWhiteSpace(EffectiveModelName))
				{
					return false;
				}
				if (Provider != AIProvider.Custom && string.IsNullOrWhiteSpace(ApiKey))
				{
					return false;
				}
				return true;
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look<bool>(ref IsEnabled, "IsEnabled", true, false);
			Scribe_Values.Look<AIProvider>(ref Provider, "Provider", AIProvider.Google, false);
			Scribe_Values.Look<string>(ref ApiKey, "ApiKey", (string)null, false);
			Scribe_Values.Look<string>(ref CustomUrl, "CustomUrl", (string)null, false);
			Scribe_Values.Look<string>(ref CustomModelName, "CustomModelName", (string)null, false);
		}
	}
	public class Payload
	{
		public string URL { get; set; }

		public string Model { get; set; }

		public string Request { get; set; }

		public string Response { get; set; }

		public int? TokenCount { get; set; }

		public string ErrorMessage { get; set; }

		public bool IsValid { get; set; }

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("=== RIMTALK-EXPANDMEMORY AI REPORT ===");
			stringBuilder.AppendLine("URL:      " + URL);
			stringBuilder.AppendLine("Model:    " + Model);
			stringBuilder.AppendLine($"Tokens:   {TokenCount}");
			if (!IsValid)
			{
				stringBuilder.AppendLine("Error:    " + ErrorMessage);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("--- REQUEST PAYLOAD ---");
			stringBuilder.AppendLine(Request);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("--- RESPONSE PAYLOAD ---");
			stringBuilder.AppendLine(Response);
			stringBuilder.AppendLine("==========================================");
			return stringBuilder.ToString();
		}
	}
	public static class EmbeddingService
	{
		private static Dictionary<string, float[]> embeddingCache = new Dictionary<string, float[]>();

		private const int MAX_CACHE_SIZE = 500;

		private static bool isInitialized = false;

		private static string apiKey = "";

		private static string apiUrl = "";

		private static string provider = "";

		private static int embeddingDimension = 1024;

		public static void Initialize()
		{
			if (isInitialized)
			{
				return;
			}
			try
			{
				Log.Message("[Embedding] v3.3.2.27: Semantic embedding功能已移除，使用SuperKeywordEngine替代");
			}
			catch (Exception ex)
			{
				Log.Error("[Embedding] Init failed: " + ex.Message);
				isInitialized = false;
			}
		}

		public static bool IsAvailable()
		{
			return false;
		}

		public static async Task<float[]> GetEmbeddingAsync(string text)
		{
			if (!IsAvailable())
			{
				return null;
			}
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			string cacheKey = GenerateCacheKey(text);
			lock (embeddingCache)
			{
				if (embeddingCache.TryGetValue(cacheKey, out var value))
				{
					return value;
				}
			}
			float[] array = await CallEmbeddingAPIAsync(text);
			if (array != null)
			{
				lock (embeddingCache)
				{
					if (embeddingCache.Count >= 500)
					{
						foreach (string item in embeddingCache.Keys.Take(50).ToList())
						{
							embeddingCache.Remove(item);
						}
					}
					embeddingCache[cacheKey] = array;
				}
			}
			return array;
		}

		public static async Task<Dictionary<string, float[]>> GetEmbeddingsBatchAsync(List<string> texts)
		{
			Dictionary<string, float[]> results = new Dictionary<string, float[]>();
			if (!IsAvailable() || texts == null || texts.Count == 0)
			{
				return results;
			}
			for (int i = 0; i < texts.Count; i += 20)
			{
				List<string> list = texts.Skip(i).Take(20).ToList();
				foreach (string text in list)
				{
					try
					{
						float[] array = await GetEmbeddingAsync(text);
						if (array != null)
						{
							results[text] = array;
						}
					}
					catch (Exception ex)
					{
						Log.Warning("[Embedding] Failed to get embedding for text: " + ex.Message);
					}
				}
				if (i + 20 < texts.Count)
				{
					await Task.Delay(100);
				}
			}
			return results;
		}

		public static float CosineSimilarity(float[] vectorA, float[] vectorB)
		{
			if (vectorA == null || vectorB == null)
			{
				return 0f;
			}
			if (vectorA.Length != vectorB.Length)
			{
				Log.Error($"[Embedding] Vector dimension mismatch: {vectorA.Length} vs {vectorB.Length}");
				return 0f;
			}
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			for (int i = 0; i < vectorA.Length; i++)
			{
				num += vectorA[i] * vectorB[i];
				num2 += vectorA[i] * vectorA[i];
				num3 += vectorB[i] * vectorB[i];
			}
			if (num2 == 0f || num3 == 0f)
			{
				return 0f;
			}
			return num / (float)(Math.Sqrt(num2) * Math.Sqrt(num3));
		}

		private static async Task<float[]> CallEmbeddingAPIAsync(string text)
		{
			_ = 1;
			try
			{
				if (provider == "Google")
				{
					return await CallGeminiEmbeddingAsync(text);
				}
				return await CallOpenAIStyleEmbeddingAsync(text);
			}
			catch (Exception ex)
			{
				Log.Error("[Embedding] API call failed: " + ex.Message);
				return null;
			}
		}

		private static async Task<float[]> CallOpenAIStyleEmbeddingAsync(string text)
		{
			if (string.IsNullOrEmpty(apiKey))
			{
				Log.Error("[Embedding] API Key is empty! Please configure it in Mod Settings.");
				return null;
			}
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
			request.Method = "POST";
			request.ContentType = "application/json";
			request.Headers["Authorization"] = "Bearer " + apiKey;
			request.Timeout = 10000;
			string model = ((provider == "DeepSeek") ? "deepseek-embedding" : "text-embedding-ada-002");
			string s = BuildOpenAIEmbeddingRequest(text, model);
			byte[] bodyRaw = Encoding.UTF8.GetBytes(s);
			request.ContentLength = bodyRaw.Length;
			using (Stream stream = await request.GetRequestStreamAsync())
			{
				await stream.WriteAsync(bodyRaw, 0, bodyRaw.Length);
			}
			try
			{
				using HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());
				using StreamReader streamReader = new StreamReader(response.GetResponseStream());
				return ParseOpenAIEmbeddingResponse(await streamReader.ReadToEndAsync());
			}
			catch (WebException ex)
			{
				if (ex.Response != null)
				{
					using HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response;
					using StreamReader streamReader2 = new StreamReader(httpWebResponse.GetResponseStream());
					string arg = streamReader2.ReadToEnd();
					Log.Error($"[Embedding] API Error {httpWebResponse.StatusCode}: {arg}");
				}
				throw;
			}
		}

		private static async Task<float[]> CallGeminiEmbeddingAsync(string text)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
			request.Method = "POST";
			request.ContentType = "application/json";
			request.Timeout = 10000;
			string s = BuildGeminiEmbeddingRequest(text);
			byte[] bodyRaw = Encoding.UTF8.GetBytes(s);
			request.ContentLength = bodyRaw.Length;
			using (Stream stream = await request.GetRequestStreamAsync())
			{
				await stream.WriteAsync(bodyRaw, 0, bodyRaw.Length);
			}
			using HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());
			using StreamReader streamReader = new StreamReader(response.GetResponseStream());
			return ParseGeminiEmbeddingResponse(await streamReader.ReadToEndAsync());
		}

		private static string BuildOpenAIEmbeddingRequest(string text, string model)
		{
			string text2 = text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("{");
			stringBuilder.Append("\"model\":\"" + model + "\",");
			stringBuilder.Append("\"input\":\"" + text2 + "\"");
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}

		private static string BuildGeminiEmbeddingRequest(string text)
		{
			string text2 = text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("{");
			stringBuilder.Append("\"content\":{");
			stringBuilder.Append("\"parts\":[{");
			stringBuilder.Append("\"text\":\"" + text2 + "\"");
			stringBuilder.Append("}]");
			stringBuilder.Append("}");
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}

		private static float[] ParseOpenAIEmbeddingResponse(string responseText)
		{
			try
			{
				int num = responseText.IndexOf("\"embedding\":");
				if (num == -1)
				{
					return null;
				}
				int num2 = responseText.IndexOf('[', num);
				int num3 = responseText.IndexOf(']', num2);
				if (num2 == -1 || num3 == -1)
				{
					return null;
				}
				string[] array = responseText.Substring(num2 + 1, num3 - num2 - 1).Split(',', StringSplitOptions.None);
				float[] array2 = new float[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					if (!float.TryParse(array[i].Trim(), out array2[i]))
					{
						Log.Error($"[Embedding] Failed to parse float at index {i}");
						return null;
					}
				}
				return array2;
			}
			catch (Exception ex)
			{
				Log.Error("[Embedding] Parse error: " + ex.Message);
				return null;
			}
		}

		private static float[] ParseGeminiEmbeddingResponse(string responseText)
		{
			try
			{
				int num = responseText.IndexOf("\"values\":");
				if (num == -1)
				{
					return null;
				}
				int num2 = responseText.IndexOf('[', num);
				int num3 = responseText.IndexOf(']', num2);
				if (num2 == -1 || num3 == -1)
				{
					return null;
				}
				string[] array = responseText.Substring(num2 + 1, num3 - num2 - 1).Split(',', StringSplitOptions.None);
				float[] array2 = new float[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					if (!float.TryParse(array[i].Trim(), out array2[i]))
					{
						Log.Error($"[Embedding] Failed to parse float at index {i}");
						return null;
					}
				}
				return array2;
			}
			catch (Exception ex)
			{
				Log.Error("[Embedding] Parse error: " + ex.Message);
				return null;
			}
		}

		private static string GenerateCacheKey(string text)
		{
			int hashCode = text.GetHashCode();
			return $"{provider}_{hashCode}";
		}

		public static void ClearCache()
		{
			lock (embeddingCache)
			{
				int count = embeddingCache.Count;
				embeddingCache.Clear();
				Log.Message($"[Embedding] Cleared {count} cached embeddings");
			}
		}

		public static EmbeddingCacheStats GetCacheStats()
		{
			lock (embeddingCache)
			{
				return new EmbeddingCacheStats
				{
					CachedCount = embeddingCache.Count,
					MaxCacheSize = 500,
					Provider = provider,
					Dimension = embeddingDimension,
					IsInitialized = isInitialized
				};
			}
		}

		public static EmbeddingServiceWrapper GetInstance()
		{
			return new EmbeddingServiceWrapper();
		}

		public static float[] GetEmbedding(string text)
		{
			try
			{
				Task<float[]> embeddingAsync = GetEmbeddingAsync(text);
				embeddingAsync.Wait(5000);
				return embeddingAsync.Result;
			}
			catch (Exception ex)
			{
				Log.Warning("[Embedding] Sync GetEmbedding failed: " + ex.Message);
				return null;
			}
		}
	}
	public class EmbeddingServiceWrapper
	{
		public float[] GetEmbedding(string text)
		{
			return EmbeddingService.GetEmbedding(text);
		}

		public async Task<float[]> GetEmbeddingAsync(string text)
		{
			return await EmbeddingService.GetEmbeddingAsync(text);
		}
	}
	public class EmbeddingCacheStats
	{
		public int CachedCount;

		public int MaxCacheSize;

		public string Provider;

		public int Dimension;

		public bool IsInitialized;
	}
}
namespace RimTalk.Memory.AI.Integration
{
	public static class Player2AuthHelper
	{
		private const string FallbackPlayer2Website = "https://player2.game";

		public static bool IsAuthenticating
		{
			get
			{
				try
				{
					return Player2AuthService.IsAuthenticating;
				}
				catch
				{
					return false;
				}
			}
		}

		public static string ApprovalUrl
		{
			get
			{
				try
				{
					return Player2AuthService.ApprovalUrl;
				}
				catch
				{
					return null;
				}
			}
		}

		public static void StartAuth(Action<string> onKeyReceived = null)
		{
			try
			{
				Player2AuthService.StartAuth((Action<string>)delegate(string key)
				{
					onKeyReceived?.Invoke(key);
				});
			}
			catch (Exception ex)
			{
				Log.Warning("[RimTalk.Memory.AI] 调用 RimTalk Player2AuthService 失败: " + ex.Message + "，回退至打开官网。");
				Application.OpenURL("https://player2.game");
			}
		}

		public static void Cancel()
		{
			try
			{
				Player2AuthService.Cancel();
			}
			catch (Exception ex)
			{
				Log.Warning("[RimTalk.Memory.AI] 取消 Player2 授权失败: " + ex.Message);
			}
		}

		public static void ReopenApprovalPage()
		{
			try
			{
				string approvalUrl = ApprovalUrl;
				if (!string.IsNullOrEmpty(approvalUrl))
				{
					Application.OpenURL(approvalUrl);
					return;
				}
			}
			catch
			{
			}
			Application.OpenURL("https://player2.game");
		}

		public static void OpenWebsite()
		{
			Application.OpenURL("https://player2.game");
		}
	}
	public static class RimTalkApiConfigGetter
	{
		public static ApiConfig GetActiveConfig()
		{
			RimTalkSettings val = Settings.Get();
			if (val == null)
			{
				return null;
			}
			ApiConfig activeConfig = val.GetActiveConfig();
			if (activeConfig == null)
			{
				return null;
			}
			return Convert(activeConfig);
		}

		public static List<ApiConfig> GetRimTalkApiConfigs()
		{
			RimTalkSettings val = Settings.Get();
			if (val == null)
			{
				return new List<ApiConfig>();
			}
			if (val.UseSimpleConfig)
			{
				ApiConfig activeConfig = val.GetActiveConfig();
				if (activeConfig == null)
				{
					return new List<ApiConfig>();
				}
				return new List<ApiConfig>(1) { Convert(activeConfig) };
			}
			if (!val.UseCloudProviders)
			{
				ApiConfig localConfig = val.LocalConfig;
				if (localConfig == null || !localConfig.IsValid())
				{
					return new List<ApiConfig>();
				}
				return new List<ApiConfig>(1) { Convert(localConfig) };
			}
			List<ApiConfig> cloudConfigs = val.CloudConfigs;
			if (cloudConfigs == null || cloudConfigs.Count <= 0)
			{
				return new List<ApiConfig>();
			}
			int count = cloudConfigs.Count;
			int num = Math.Max(0, val.CurrentCloudConfigIndex % count);
			List<ApiConfig> list = new List<ApiConfig>(count);
			for (int i = 0; i < count; i++)
			{
				ApiConfig val2 = cloudConfigs[(num + i) % count];
				if (val2 != null && val2.IsValid())
				{
					list.Add(Convert(val2));
				}
			}
			return list;
		}

		private static ApiConfig Convert(ApiConfig rimTalkConfig)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Invalid comparison between Unknown and I4
			AIProvider aIProvider = rimTalkConfig.Provider.Convert();
			string text = rimTalkConfig.GetEffectiveModelName();
			if (string.IsNullOrWhiteSpace(text))
			{
				text = aIProvider switch
				{
					AIProvider.Google => "gemma-4-26b-a4b-it", 
					AIProvider.Player2 => "Default", 
					_ => "default", 
				};
			}
			ApiConfig apiConfig = new ApiConfig();
			apiConfig.IsEnabled = rimTalkConfig.IsEnabled;
			apiConfig.Provider = aIProvider;
			apiConfig.ApiKey = rimTalkConfig.ApiKey;
			ApiConfig apiConfig2 = apiConfig;
			AIProvider provider = rimTalkConfig.Provider;
			bool flag = provider - 10 <= 1;
			apiConfig2.CustomUrl = (flag ? rimTalkConfig.BaseUrl : null);
			apiConfig.CustomModelName = text;
			return apiConfig;
		}

		private static AIProvider Convert(this AIProvider provider)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Expected I4, but got Unknown
			return (int)provider switch
			{
				0 => AIProvider.Google, 
				1 => AIProvider.OpenAI, 
				2 => AIProvider.DeepSeek, 
				3 => AIProvider.Grok, 
				4 => AIProvider.GLM, 
				5 => AIProvider.GLMCoding, 
				6 => AIProvider.AlibabaIntl, 
				7 => AIProvider.AlibabaCN, 
				8 => AIProvider.OpenRouter, 
				13 => AIProvider.Claude, 
				14 => AIProvider.Moonshot, 
				9 => AIProvider.Player2, 
				_ => AIProvider.Custom, 
			};
		}
	}
}
namespace RimTalk.Memory.AI.Client
{
	public class ClientPool : IAIClient
	{
		private readonly Dictionary<string, IAIClient> _clientCache = new Dictionary<string, IAIClient>();

		private readonly object _lock = new object();

		public async Task<Payload> GetChatCompletionAsync(string prompt)
		{
			List<ApiConfig> candidates = AIClientFactory.GetCurrentCandidateConfigs();
			if (candidates.Count == 0)
			{
				Log.Warning("[RimTalk.Memory.AI.Client] 没有检测到任何可用的 AI 配置（跟随 RimTalk 或独立配置均未就绪）。");
				return new Payload
				{
					URL = "None",
					Model = "None",
					Request = prompt,
					ErrorMessage = "No available AI configuration found."
				};
			}
			Payload result = null;
			for (int i = 0; i < candidates.Count; i++)
			{
				ApiConfig config = candidates[i];
				IAIClient orCreateClient = GetOrCreateClient(config);
				if (orCreateClient == null)
				{
					continue;
				}
				try
				{
					Payload payload = await orCreateClient.GetChatCompletionAsync(prompt);
					if (payload != null && payload.IsValid)
					{
						return payload;
					}
					result = payload;
					Log.Warning($"[RimTalk.Memory.AI.Client] 配置 [{i + 1}/{candidates.Count}] ({config.Provider}/{config.EffectiveModelName}) 响应失败。");
				}
				catch (Exception ex)
				{
					Log.Warning($"[RimTalk.Memory.AI.Client] 配置 [{i + 1}/{candidates.Count}] ({config.Provider}/{config.EffectiveModelName}) 抛出异常: {ex.Message}");
					result = new Payload
					{
						URL = config.URL,
						Model = config.EffectiveModelName,
						Request = prompt,
						ErrorMessage = ex.Message
					};
				}
			}
			Log.Error($"[RimTalk.Memory.AI.Client] 所有已尝试的 {candidates.Count} 个 AI 配置均执行失败。");
			return result;
		}

		public async Task<bool> ValidateAsync()
		{
			List<ApiConfig> candidates = AIClientFactory.GetCurrentCandidateConfigs();
			if (candidates.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < candidates.Count; i++)
			{
				ApiConfig config = candidates[i];
				IAIClient orCreateClient = GetOrCreateClient(config);
				if (orCreateClient == null)
				{
					continue;
				}
				try
				{
					if (await orCreateClient.ValidateAsync())
					{
						return true;
					}
				}
				catch (Exception ex)
				{
					Log.Warning($"[RimTalk.Memory.AI.Client] 验证配置 [{i + 1}/{candidates.Count}] 时发生异常: {ex.Message}");
				}
			}
			return false;
		}

		public void Reset()
		{
			lock (_lock)
			{
				_clientCache.Clear();
			}
		}

		private IAIClient GetOrCreateClient(ApiConfig config)
		{
			string configFingerprint = AIClientFactory.GetConfigFingerprint(config);
			lock (_lock)
			{
				if (_clientCache.TryGetValue(configFingerprint, out var value) && value != null)
				{
					return value;
				}
				IAIClient iAIClient = AIClientFactory.BuildClient(config);
				if (iAIClient != null)
				{
					_clientCache[configFingerprint] = iAIClient;
				}
				return iAIClient;
			}
		}
	}
	public interface IAIClient
	{
		Task<Payload> GetChatCompletionAsync(string prompt);

		Task<bool> ValidateAsync();
	}
	public class OpenAIClient : IAIClient
	{
		private class GameExitException : Exception
		{
		}

		private class ConnectTimeoutException : Exception
		{
		}

		private class ReadTimeoutException : Exception
		{
		}

		private class ResponseException : Exception
		{
			public long StatusCode { get; }

			public string RawResponse { get; }

			public ResponseException(long statusCode, string rawResponse)
			{
				StatusCode = statusCode;
				RawResponse = rawResponse;
			}
		}

		private readonly Uri _url;

		private readonly string _apiKey;

		private readonly string _model;

		private readonly Dictionary<string, string> _extraHeaders;

		private const string DefaultPath = "/v1/chat/completions";

		private const float ConnectTimeoutSec = 120f;

		private const float ReadTimeoutSec = 60f;

		private const float PollIntervalSec = 0.1f;

		private static readonly int[] RetryDelayMs = new int[2] { 4000, 8000 };

		private static int MaxRetrys => RetryDelayMs.Length;

		public OpenAIClient(ApiConfig config)
		{
			string text = config.URL.Trim().TrimEnd('/');
			Uri uri = new Uri(text, UriKind.Absolute);
			string text2 = uri.AbsolutePath.TrimEnd('/');
			_url = ((text2.Length == 0) ? new Uri(text + "/v1/chat/completions", UriKind.Absolute) : ((text2 == "/v1") ? new Uri(text + "/chat/completions", UriKind.Absolute) : uri));
			_apiKey = config.ApiKey;
			_model = config.EffectiveModelName;
			_extraHeaders = config.Provider.GetExtraHeaders();
		}

		public async Task<Payload> GetChatCompletionAsync(string prompt)
		{
			bool isAILogEnabled = RimTalkMemoryPatchMod.Settings.EnableAILog;
			string jsonContent = BuildRequestJson(prompt);
			Payload payload = (isAILogEnabled ? new Payload
			{
				URL = _url.OriginalString,
				Model = _model,
				Request = jsonContent
			} : new Payload());
			Exception ex = null;
			for (int retry = 0; retry <= MaxRetrys; retry++)
			{
				if (retry > 0)
				{
					int num = RetryDelayMs[retry - 1];
					Log.Warning($"[RimTalk.Memory.AI.Client] Attempt {retry}/{MaxRetrys + 1} failed (model={_model}). Retrying in {num} ms.");
					await Task.Delay(num);
				}
				try
				{
					return ParseSuccessResponse(await SendRequestAsync(jsonContent), payload);
				}
				catch (GameExitException)
				{
					Log.Error("[RimTalk.Memory.AI.Client] 中途退出，请求终止");
					if (isAILogEnabled)
					{
						payload.ErrorMessage = "Game exited during request";
					}
					return payload;
				}
				catch (ConnectTimeoutException ex3)
				{
					Log.Warning($"[RimTalk.Memory.AI.Client] Connection timeout after {120f} seconds");
					ex = ex3;
				}
				catch (ReadTimeoutException ex4)
				{
					Log.Warning($"[RimTalk.Memory.AI.Client] Read timeout after {60f} seconds");
					ex = ex4;
				}
				catch (ResponseException ex5)
				{
					long statusCode = ex5.StatusCode;
					string rawResponse = ex5.RawResponse;
					switch (statusCode)
					{
					case 429L:
						Log.Warning("[RimTalk.Memory.AI.Client] Quota exceeded for model " + _model);
						ex = ex5;
						break;
					case 500L:
					case 501L:
					case 502L:
					case 503L:
					case 504L:
					case 505L:
					case 506L:
					case 507L:
					case 508L:
					case 509L:
					case 510L:
					case 511L:
					case 512L:
					case 513L:
					case 514L:
					case 515L:
					case 516L:
					case 517L:
					case 518L:
					case 519L:
					case 520L:
					case 521L:
					case 522L:
					case 523L:
					case 524L:
					case 525L:
					case 526L:
					case 527L:
					case 528L:
					case 529L:
					case 530L:
					case 531L:
					case 532L:
					case 533L:
					case 534L:
					case 535L:
					case 536L:
					case 537L:
					case 538L:
					case 539L:
					case 540L:
					case 541L:
					case 542L:
					case 543L:
					case 544L:
					case 545L:
					case 546L:
					case 547L:
					case 548L:
					case 549L:
					case 550L:
					case 551L:
					case 552L:
					case 553L:
					case 554L:
					case 555L:
					case 556L:
					case 557L:
					case 558L:
					case 559L:
					case 560L:
					case 561L:
					case 562L:
					case 563L:
					case 564L:
					case 565L:
					case 566L:
					case 567L:
					case 568L:
					case 569L:
					case 570L:
					case 571L:
					case 572L:
					case 573L:
					case 574L:
					case 575L:
					case 576L:
					case 577L:
					case 578L:
					case 579L:
					case 580L:
					case 581L:
					case 582L:
					case 583L:
					case 584L:
					case 585L:
					case 586L:
					case 587L:
					case 588L:
					case 589L:
					case 590L:
					case 591L:
					case 592L:
					case 593L:
					case 594L:
					case 595L:
					case 596L:
					case 597L:
					case 598L:
					case 599L:
						Log.Warning($"[RimTalk.Memory.AI.Client] HTTP {statusCode} error for model {_model}");
						ex = ex5;
						break;
					case 401L:
					case 403L:
						Log.Warning("[RimTalk.Memory.AI.Client] Authentication failed for model " + _model);
						if (isAILogEnabled)
						{
							payload.ErrorMessage = ExtractErrorMessage(rawResponse) ?? "Authentication failed";
							payload.Response = rawResponse;
						}
						return payload;
					default:
						Log.Warning($"[RimTalk.Memory.AI.Client] HTTP {statusCode} error for model {_model}");
						if (isAILogEnabled)
						{
							payload.ErrorMessage = ExtractErrorMessage(rawResponse) ?? $"HTTP {statusCode} error";
							payload.Response = rawResponse;
						}
						return payload;
					}
				}
			}
			Log.Warning($"[RimTalk.Memory.AI.Client] All {MaxRetrys} attempts exhausted (model={_model}).");
			if (isAILogEnabled)
			{
				if (ex is ResponseException { RawResponse: var rawResponse2 } ex6)
				{
					payload.Response = rawResponse2;
					payload.ErrorMessage = ExtractErrorMessage(rawResponse2) ?? ((ex6.StatusCode == 429) ? "Quota exceeded" : $"HTTP {ex6.StatusCode} error");
				}
				else
				{
					string errorMessage = ((ex is ConnectTimeoutException) ? "Connection timeout" : ((!(ex is ReadTimeoutException)) ? null : "Read timeout"));
					payload.ErrorMessage = errorMessage;
				}
			}
			return payload;
		}

		public async Task<bool> ValidateAsync()
		{
			string jsonContent = BuildRequestJson("ping");
			try
			{
				await SendRequestAsync(jsonContent);
				return true;
			}
			catch (ConnectTimeoutException)
			{
				MessageUtil.MessageAndError($"[RimTalk.Memory.AI.Client] ValidateAsync connection timeout after {120f} seconds for model {_model}");
			}
			catch (ReadTimeoutException)
			{
				MessageUtil.MessageAndError($"[RimTalk.Memory.AI.Client] ValidateAsync read timeout after {60f} seconds for model {_model}");
			}
			catch (ResponseException ex3)
			{
				MessageUtil.MessageAndError($"[RimTalk.Memory.AI.Client] ValidateAsync HTTP {ex3.StatusCode} error for model {_model}");
			}
			catch (Exception ex4)
			{
				MessageUtil.MessageAndError("[RimTalk.Memory.AI.Client] ValidateAsync unexpected error for model " + _model + ": " + ex4.Message);
			}
			return false;
		}

		private string BuildRequestJson(string prompt)
		{
			return JsonUtil.SerializeToJson(new OpenAIRequest
			{
				Model = _model,
				Messages = new List<Message>(1)
				{
					new Message
					{
						Role = "user",
						Content = (prompt ?? string.Empty)
					}
				}
			});
		}

		private async Task<string> SendRequestAsync(string jsonContent, DownloadHandler downloadHandler = null)
		{
			UnityWebRequest webRequest = new UnityWebRequest(_url, "POST");
			try
			{
				webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonContent));
				DownloadHandler obj = downloadHandler;
				if (obj == null)
				{
					DownloadHandlerBuffer val = new DownloadHandlerBuffer();
					DownloadHandler val2 = (DownloadHandler)val;
					downloadHandler = (DownloadHandler)val;
					obj = val2;
				}
				webRequest.downloadHandler = obj;
				webRequest.SetRequestHeader("Content-Type", "application/json");
				if (!string.IsNullOrEmpty(_apiKey))
				{
					webRequest.SetRequestHeader("Authorization", "Bearer " + _apiKey);
				}
				Dictionary<string, string> extraHeaders = _extraHeaders;
				if (extraHeaders != null && extraHeaders.Count > 0)
				{
					foreach (KeyValuePair<string, string> extraHeader in _extraHeaders)
					{
						webRequest.SetRequestHeader(extraHeader.Key, extraHeader.Value);
					}
				}
				bool hasStartedReceiving = false;
				float inactivityTimer = 0f;
				ulong lastBytes = 0uL;
				UnityWebRequestAsyncOperation asyncOp = webRequest.SendWebRequest();
				while (!((AsyncOperation)asyncOp).isDone)
				{
					if (Current.Game == null)
					{
						webRequest.Abort();
						throw new GameExitException();
					}
					await Task.Delay(100);
					ulong downloadedBytes = webRequest.downloadedBytes;
					inactivityTimer = ((downloadedBytes > lastBytes) ? 0f : (inactivityTimer + 0.1f));
					lastBytes = downloadedBytes;
					hasStartedReceiving = hasStartedReceiving || downloadedBytes != 0;
					if (!hasStartedReceiving && inactivityTimer > 120f)
					{
						webRequest.Abort();
						throw new ConnectTimeoutException();
					}
					if (hasStartedReceiving && inactivityTimer > 60f)
					{
						webRequest.Abort();
						throw new ReadTimeoutException();
					}
				}
				string text = downloadHandler.text;
				long responseCode = webRequest.responseCode;
				bool flag = responseCode >= 400;
				if (!flag)
				{
					Result result = webRequest.result;
					bool flag2 = result - 2 <= 1;
					flag = flag2;
				}
				if (flag)
				{
					throw new ResponseException(responseCode, text);
				}
				return text;
			}
			finally
			{
				((IDisposable)webRequest)?.Dispose();
			}
		}

		private Payload ParseSuccessResponse(string responseText, Payload payload)
		{
			if (!string.IsNullOrWhiteSpace(responseText) && JsonUtil.TryDeserializeFromJson<OpenAIResponse>(responseText, out var result, out var _))
			{
				List<Choice> choices = result.Choices;
				if (choices != null && choices.Count != 0 && result?.Error == null)
				{
					payload.IsValid = true;
					payload.Response = choices[0]?.Message?.Content;
					payload.TokenCount = result?.Usage?.TotalTokens;
					return payload;
				}
			}
			if (RimTalkMemoryPatchMod.Settings.EnableAILog)
			{
				payload.ErrorMessage = "解析错误";
				payload.Response = responseText;
			}
			return payload;
		}

		private static string ExtractErrorMessage(string jsonResponse)
		{
			if (string.IsNullOrEmpty(jsonResponse))
			{
				return null;
			}
			if (JsonUtil.TryDeserializeFromJson<ErrorResponse>(jsonResponse, out var result, out var exception))
			{
				ErrorDetail errorDetail = result?.Error;
				if (errorDetail != null)
				{
					return FormatError(errorDetail);
				}
			}
			if (JsonUtil.TryDeserializeFromJson<ErrorDetail>(jsonResponse, out var result2, out exception) && result2 != null)
			{
				return FormatError(result2);
			}
			return null;
		}

		private static string FormatError(ErrorDetail detail)
		{
			string text = detail.Message;
			if (string.IsNullOrEmpty(text))
			{
				text = detail.Status;
			}
			if (string.IsNullOrEmpty(text))
			{
				text = detail.Type;
			}
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			if (string.IsNullOrEmpty(detail.Code))
			{
				return text;
			}
			return "[" + detail.Code + "] " + text;
		}
	}
	public sealed class Player2Client : IAIClient
	{
		private const string GameClientId = "01a05876-8d6b-7376-92ef-0e51b45d130c";

		private const string DefaultRemoteBaseUrl = "https://api.player2.game/v1";

		private string _apiKey;

		private string _baseUrl;

		public Player2Client(ApiConfig config)
		{
			_apiKey = config.ApiKey;
			_baseUrl = "https://api.player2.game/v1";
		}

		public async Task<Payload> GetChatCompletionAsync(string prompt)
		{
			string json = JsonUtil.SerializeToJson(new Player2Request
			{
				Messages = new List<Player2Message>(1)
				{
					new Player2Message
					{
						Role = "user",
						Content = (prompt ?? string.Empty)
					}
				}
			});
			Payload payload = new Payload
			{
				Model = "Player2",
				Request = json
			};
			try
			{
				if (string.IsNullOrEmpty(_apiKey))
				{
					await EnsureLocalConnectionAsync();
				}
				if (string.IsNullOrWhiteSpace(_apiKey))
				{
					throw new InvalidOperationException("Player2 local app is unavailable.");
				}
				string text = await SendRequestAsync(json);
				payload.URL = _baseUrl + "/chat/completions";
				if (JsonUtil.TryDeserializeFromJson<OpenAIResponse>(text, out var result, out var _))
				{
					List<Choice> list = result?.Choices;
					if (list != null && list.Count > 0)
					{
						string text2 = result.Choices[0]?.Message?.Content;
						if (text2 != null)
						{
							payload.IsValid = true;
							payload.Response = text2;
							payload.TokenCount = result.Usage?.TotalTokens;
							return payload;
						}
					}
				}
				payload.ErrorMessage = "Player2 response could not be parsed.";
				payload.Response = text;
				return payload;
			}
			catch (Exception ex)
			{
				payload.URL = _baseUrl + "/chat/completions";
				payload.ErrorMessage = ex.Message;
				return payload;
			}
		}

		public async Task<bool> ValidateAsync()
		{
			try
			{
				return (await GetChatCompletionAsync("ping")).IsValid;
			}
			catch
			{
				return false;
			}
		}

		private Task<string> SendRequestAsync(string json)
		{
			return SendRequestToUrlAsync(_baseUrl + "/chat/completions", json, _apiKey);
		}

		private async Task EnsureLocalConnectionAsync()
		{
			(string, string)? tuple = await TryGetLocalConnectionAsync();
			if (tuple.HasValue)
			{
				_apiKey = tuple.Value.Item1;
				_baseUrl = tuple.Value.Item2;
			}
		}

		private static async Task<string> SendRequestToUrlAsync(string url, string json, string apiKey)
		{
			UnityWebRequest request = new UnityWebRequest(url, "POST");
			try
			{
				request.uploadHandler = (UploadHandler)new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
				request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
				request.SetRequestHeader("Content-Type", "application/json");
				request.SetRequestHeader("Authorization", "Bearer " + apiKey);
				request.SetRequestHeader("player2-game-key", "01a05876-8d6b-7376-92ef-0e51b45d130c");
				request.timeout = 120;
				await SendWebRequestAsync(request);
				Result result = request.result;
				if (result - 2 <= 1)
				{
					string text = request.downloadHandler.text;
					throw new InvalidOperationException(string.IsNullOrWhiteSpace(text) ? request.error : text);
				}
				return request.downloadHandler.text;
			}
			finally
			{
				((IDisposable)request)?.Dispose();
			}
		}

		private static async Task<(string Key, string BaseUrl)?> TryGetLocalConnectionAsync()
		{
			foreach (string baseUrl in GetCandidateBaseUrls())
			{
				try
				{
					UnityWebRequest health = UnityWebRequest.Get(baseUrl + "/health");
					try
					{
						health.timeout = 2;
						await SendWebRequestAsync(health);
						Result result = health.result;
						if (result - 2 <= 1)
						{
							continue;
						}
						UnityWebRequest login = new UnityWebRequest(baseUrl + "/login/web/01a05876-8d6b-7376-92ef-0e51b45d130c", "POST");
						try
						{
							login.uploadHandler = (UploadHandler)new UploadHandlerRaw(Encoding.UTF8.GetBytes("{}"));
							login.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
							login.SetRequestHeader("Content-Type", "application/json");
							login.timeout = 3;
							await SendWebRequestAsync(login);
							result = login.result;
							if (result - 2 <= 1)
							{
								goto end_IL_011a;
							}
							if (JsonUtil.TryDeserializeFromJson<Player2LoginResponse>(login.downloadHandler.text, out var result2, out var _) && !string.IsNullOrWhiteSpace(result2?.P2Key))
							{
								Log.Message("[RimTalk.Memory.AI] Player2 local app detected at " + baseUrl + ".");
								return (result2.P2Key, baseUrl);
							}
							goto end_IL_003b;
							end_IL_011a:;
						}
						finally
						{
							((IDisposable)login)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)health)?.Dispose();
					}
					end_IL_003b:;
				}
				catch
				{
				}
			}
			return null;
		}

		private static IEnumerable<string> GetCandidateBaseUrls()
		{
			List<string> list = new List<string>();
			try
			{
				string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "game.player2.client", "api.port");
				if (File.Exists(path) && int.TryParse(File.ReadAllText(path).Trim(), out var result) && result > 0)
				{
					list.Add($"http://127.0.0.1:{result}/v1");
					list.Add($"http://localhost:{result}/v1");
				}
			}
			catch
			{
			}
			list.Add("http://127.0.0.1:4315/v1");
			list.Add("http://127.0.0.1:4316/v1");
			list.Add("http://localhost:4315/v1");
			list.Add("http://localhost:4316/v1");
			return list;
		}

		private static Task SendWebRequestAsync(UnityWebRequest request)
		{
			TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>();
			((AsyncOperation)request.SendWebRequest()).completed += delegate
			{
				completion.TrySetResult(result: true);
			};
			return completion.Task;
		}
	}
}
namespace RimTalk.Memory.AI.Client.Dto
{
	[DataContract]
	public class OpenAIRequest
	{
		[DataMember(Name = "model")]
		public string Model { get; set; }

		[DataMember(Name = "messages")]
		public List<Message> Messages { get; set; } = new List<Message>();

		[DataMember(Name = "temperature", EmitDefaultValue = false)]
		public double? Temperature { get; set; }
	}
	[DataContract]
	public class Message
	{
		[DataMember(Name = "role")]
		public string Role { get; set; }

		[DataMember(Name = "content")]
		public string Content { get; set; }
	}
	[DataContract]
	public class OpenAIResponse
	{
		[DataMember(Name = "choices")]
		public List<Choice> Choices { get; set; }

		[DataMember(Name = "usage")]
		public Usage Usage { get; set; }

		[DataMember(Name = "error")]
		public ErrorDetail Error { get; set; }
	}
	[DataContract]
	public class Choice
	{
		[DataMember(Name = "index", EmitDefaultValue = false)]
		public int Index { get; set; }

		[DataMember(Name = "message")]
		public Message Message { get; set; }

		[DataMember(Name = "finish_reason", EmitDefaultValue = false)]
		public string FinishReason { get; set; }
	}
	[DataContract]
	public class Usage
	{
		[DataMember(Name = "prompt_tokens", EmitDefaultValue = false)]
		public int PromptTokens { get; set; }

		[DataMember(Name = "completion_tokens", EmitDefaultValue = false)]
		public int CompletionTokens { get; set; }

		[DataMember(Name = "total_tokens", EmitDefaultValue = false)]
		public int TotalTokens { get; set; }
	}
	[DataContract]
	public class ErrorDetail
	{
		[DataMember(Name = "message", EmitDefaultValue = false)]
		public string Message { get; set; }

		[DataMember(Name = "code", EmitDefaultValue = false)]
		public string Code { get; set; }

		[DataMember(Name = "status", EmitDefaultValue = false)]
		public string Status { get; set; }

		[DataMember(Name = "type", EmitDefaultValue = false)]
		public string Type { get; set; }
	}
	[DataContract]
	public class ErrorResponse
	{
		[DataMember(Name = "error")]
		public ErrorDetail Error { get; set; }
	}
	[DataContract]
	public sealed class Player2LoginResponse
	{
		[DataMember(Name = "p2Key")]
		public string P2Key { get; set; }
	}
	[DataContract]
	public sealed class Player2Request
	{
		[DataMember(Name = "messages")]
		public List<Player2Message> Messages { get; set; } = new List<Player2Message>();
	}
	[DataContract]
	public sealed class Player2Message
	{
		[DataMember(Name = "role")]
		public string Role { get; set; }

		[DataMember(Name = "content")]
		public string Content { get; set; }
	}
}
