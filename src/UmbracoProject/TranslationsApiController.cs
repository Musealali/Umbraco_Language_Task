using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Forms.Web.Attributes;

namespace UmbracoProject;

[Route("/v{version:apiVersion}/")]
[ApiController]
[ApiVersion("1.0")]
[MapToApi("my-api-v1")]
public class TranslationsApiController : Controller
{

    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly ILocalizationService _localizationService;

    public TranslationsApiController(IUmbracoContextAccessor umbracoContextAccessor,
        ILocalizationService localizationService)
    {
        _umbracoContextAccessor = umbracoContextAccessor;
        _localizationService = localizationService;
    }

    [HttpGet("allLanguages/{currentCultureIsoCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAllLanguagesInCurrentCultureIsoCode(string currentCultureIsoCode)
    {
        Dictionary<string, LanguageNode> languageNodes = new Dictionary<string, LanguageNode>();

        if (!_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbraco))
            return StatusCode(StatusCodes.Status500InternalServerError, "Umbraco context currently unavailable.");

        var contentType = umbraco.Content.GetContentType("Language");

        IEnumerable<IPublishedContent>? content = umbraco.Content.GetByContentType(contentType);

        IEnumerable<ILanguage> languages = _localizationService.GetAllLanguages();

        string defaultIsoCode = "en-us";
        
        var foundLanguage = languages.FirstOrDefault(lang => lang.IsoCode == currentCultureIsoCode.Split('-')[0]);

        currentCultureIsoCode = foundLanguage != null ? foundLanguage.IsoCode : defaultIsoCode;
        

        foreach (ILanguage language in languages)
        {
            var itemWithLanguageCode =
                content?.FirstOrDefault(item => item.Value<string>("LanguageIsocode") == language.IsoCode.ToLower());

            if (itemWithLanguageCode.Cultures != null)
            {
                string isoCode = itemWithLanguageCode.Cultures[language.IsoCode.ToLower()].Culture;
                string nativeDisplayName = itemWithLanguageCode.Cultures[language.IsoCode.ToLower()].Name;
                string translatedDisplayName = itemWithLanguageCode.Cultures[currentCultureIsoCode.ToLower()].Name;

                if (!languageNodes.ContainsKey(isoCode))
                {
                    if (isoCode == currentCultureIsoCode)
                    {
                        languageNodes[isoCode] = new LanguageNode
                        {
                            isSelected = true,
                            displayName = nativeDisplayName == translatedDisplayName ? nativeDisplayName : $"{nativeDisplayName} ({translatedDisplayName})"
                        };

                    }
                    else
                    {
                        languageNodes[isoCode] = new LanguageNode
                        {
                            isSelected = false,
                            displayName = $"{nativeDisplayName} ({translatedDisplayName})"
                        };
                    }
                }
            }
        }
        return Ok(languageNodes);
    }

    [HttpGet("translations/{currentLanguageIsoCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetTranslationForCurrentLanguageIsoCode(string currentLanguageIsoCode)
    {
        IEnumerable<ILanguage> languages = _localizationService.GetAllLanguages();
        
        var foundLanguage = languages.FirstOrDefault(lang => lang.CultureInfo.TwoLetterISOLanguageName == currentLanguageIsoCode.Split('-').First());

        if (foundLanguage == null)
        {
            return StatusCode(StatusCodes.Status404NotFound, "Language was not found in the CMS");
        }
        
        var dictionaryItems = _localizationService.GetRootDictionaryItems();

        var translations = new Dictionary<string, string>();

        foreach (var item in dictionaryItems)
        {
            var translation = item.Translations.FirstOrDefault(t => t.LanguageIsoCode == foundLanguage.IsoCode);

            if (translation != null)
            {
                translations[item.ItemKey] = translation.Value;
            }
        }
        
        return Ok(translations);
    }
}

public class LanguageNode
{
    public bool isSelected { get; set; }
    public string displayName { get; set; }
}
