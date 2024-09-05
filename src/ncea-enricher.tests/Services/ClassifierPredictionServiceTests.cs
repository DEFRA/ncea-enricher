using Microsoft.Extensions.DependencyInjection;
using Ncea.Enricher.Constants;
using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Tests.Clients;
using Ncea.Enricher.Models.ML;
using FluentAssertions;
using Moq;
using Ncea.Enricher.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Configuration;

namespace Ncea.Enricher.Tests.Services;

public class ClassifierPredictionServiceTests
{
    private IServiceProvider _serviceProvider;
    private IClassifierPredictionService _classifierPredictionService;
    private Mock<ILogger<ClassifierPredictionService>> _mocklogger;
    private IClassifierVocabularyProvider _classifierVocabularyProvider;
    private PredictionEnginePool<ModelInputTheme, ModelOutput> _themePredictionPool;
    private PredictionEnginePool<ModelInputCategory, ModelOutput> _categoryPredictionPool;
    private PredictionEnginePool<ModelInputSubCategory, ModelOutput> _subCategoryPredictionPool;
    private IConfiguration _configuration;

    public ClassifierPredictionServiceTests()
    {
        _serviceProvider = ServiceProviderForTests.Get();
        _mocklogger = new Mock<ILogger<ClassifierPredictionService>>()!;
        _classifierPredictionService = _serviceProvider.GetService<IClassifierPredictionService>()!;
        _classifierVocabularyProvider = _serviceProvider.GetService<IClassifierVocabularyProvider>()!;
        _themePredictionPool = _serviceProvider.GetService<PredictionEnginePool<ModelInputTheme, ModelOutput>>()!;
        _categoryPredictionPool = _serviceProvider.GetService<PredictionEnginePool<ModelInputCategory, ModelOutput>>()!;
        _subCategoryPredictionPool = _serviceProvider.GetService<PredictionEnginePool<ModelInputSubCategory, ModelOutput>>()!;
        _configuration = _serviceProvider.GetService<IConfiguration>()!;
        _mocklogger.Setup(x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                )
            );
    }

    [Fact]
    public async Task GivenPredictTheme_WhenInputValuesAreNullOrEmpty_ThenReturnOutputWithPredictionLabelEmpty()
    {
        //Arrange
        var input = new ModelInputTheme();

        //Act
        var result = await _classifierPredictionService.PredictTheme(input, CancellationToken.None);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ModelOutput>();
    }

    [Fact]
    public async Task GivenPredictTheme_WhenInputValuesAreNotNullOrEmpty_ThenReturnOutputWithPredictionLabelEmpty()
    {
        //Arrange
        var input = new ModelInputTheme()
        {
            Title = "test-title",
            Abstract = "test-abstract",
            Lineage = "test-lineage",
            Topics = "test-topics",
            Keywords = "test-keywords",
            AltTitle = "test-alttitle",
            Theme = "test-theme"
        };

        //Act
        var result = await _classifierPredictionService.PredictTheme(input, CancellationToken.None);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ModelOutput>();
    }

    [Fact]
    public async Task GivenPredictTheme_WhenInputThemeIsAsset_ThenReturnOutputWithPredictionLabelEmpty()
    {
        //Arrange
        var input = new ModelInputTheme()
        {
            Title = "test-title",
            Abstract = "Natural Assets are the living and non-living elements of ecosystems including soils, freshwater, minerals, air and oceans. We often group assets by broad habitat types (e.g. woodland or pelagic) but can also group them by the components that make up ecosystems  (e.g. by species that span multiple habitats,  soils and sediments).",
            Lineage = "test-lineage",
            Topics = "test-topics",
            Keywords = "test-keywords",
            AltTitle = "test-alttitle"
        };

        //Act
        var result = await _classifierPredictionService.PredictTheme(input, CancellationToken.None);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ModelOutput>();
    }

    [Fact]
    public async Task GivenPredictTheme_WhenInputThemeIsPressure_ThenReturnOutputWithPredictionLabelEmpty()
    {
        //Arrange
        var input = new ModelInputTheme()
        {
            Title = "test-title",
            Abstract = "Benthic monitoring of the sea bed adjacent to the fish farm as a requirement of the CAR licence for the site. Grab samples collected in proximity to the farm and at two reference locations. Samples analysed for: benthos, redox, psa, carbon and sea louse medicine residues.",
            Lineage = "test-lineage",
            Topics = "test-topics",
            Keywords = "test-keywords",
            AltTitle = "test-alttitle"
        };

        //Act
        var result = await _classifierPredictionService.PredictTheme(input, CancellationToken.None);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ModelOutput>();
    }

    [Fact]
    public async Task GivenPredictTheme_WhenInputThemeIsBenefit_ThenReturnOutputWithPredictionLabelEmpty()
    {
        //Arrange
        var input = new ModelInputTheme()
        {
            Title = "test-title",
            Abstract = "An oil and gas industry site survey for a jack-up rig, drilling hazard and debris clearance acquired under licence P025 in February 2011. The block number traversed was 53/2.",
            Lineage = "test-lineage",
            Topics = "test-topics",
            Keywords = "test-keywords",
            AltTitle = "test-alttitle"
        };

        //Act
        var result = await _classifierPredictionService.PredictTheme(input, CancellationToken.None);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ModelOutput>();
    }

    [Fact]
    public async Task GivenPredictTheme_WhenInputThemeIsValuation_ThenReturnOutputWithPredictionLabelEmpty()
    {
        //Arrange
        var input = new ModelInputTheme()
        {
            Title = "Marine Scotland Reports - Marine Environment - Scotland's Marine Atlas",
            Abstract = "Scotland‚s vision is for ‚clean, healthy, safe, productive, biologically diverse marine and coastal environments, managed to meet the long term needs of nature and people‚. This assessment of the condition of Scotland‚s seas has been based on scientific evidence from data and analysis, supported by expert judgement.Climate Change and Ocean AcidificationThe climate shows considerable variation over short and long timescales. However, in recent years there has been a marked increase in the concentration of carbon dioxide (CO2) in the atmosphere of the Earth and, at the same time, sea surface temperature has risen as have sea levels. Changes in the biological components of the seas have been observed including earlier plankton blooms, a northward movement of some species and a reductionin seabird populations, all of which have been linked to climate change.At the same time, the seas are becoming more acidic, the consequences of which, especially for calcareous organisms, could be significant. Reductions in the emissions of greenhouse gases are required and Scotland has set ambitious targets. However, even with such reductions, it is highly likely that further impacts of climate change on the marine environment will be observed.Clean and SafeScotland‚s seas are mainly clean and safe, although there are some localised areas where there is contamination or hazards to human health. For example, sediments in several harbours and estuaries remain contaminated with hazardous substances, a legacy of past industrial discharges. Water quality in the Forth and Clyde estuaries is compromised by discharges of industrial effluent and treated sewage although effluent treatment hasimproved resulting in returning populations of residential and migratory fish. The use of historical contaminants such as polychlorinated biphenyls (PCBs) and tributyl tin (TBT) has been banned, and monitoring continues to assess their continued environmental decline.Information is being gathered on a range of other contaminants, including endocrine disrupters and brominated flame retardants, to assess their environmental impacts. Diffuse inputs of nutrients and bacteria have given rise, respectively, to some localised issues in small east coast estuaries and at bathing beaches. Action plans have been put in place to tackle these issues. Concerns such as marine litter and underwater noise havebecome more broadly recognised and will be addressed through the operational response to the Marine Strategy Framework Directive. Generally the effects of noise remain unquantified and unknown.Healthy and Biologically DiverseScotland‚s seas support a diverse array of habitats and species and contain nationally and internationally important populations of certain species such as the northern feather star, the burrowing sea anemone, the northern sea fan and cold water corals.There is evidence that certain habitats have been impacted, for example shallow and shelf subtidal sediments (including burrowed mud habitats). This stems largely from the effects of fishing over large areas of the seabed and more localised impacts from activities such as aquaculture.The low abundance of some demersal commercial fish species across the west coast of Scotland is a major concern and is being addressed through various initiatives. Improved knowledge of fishing activity and its impact on the marine environment would be beneficial.Establishment of new fisheries should only be undertaken following careful assessment of the viability and future sustainability of the fishery, especially given the sensitivity of some, particularly deep water, species to fishing and against a background of historic over-exploitation.Sharks, skates and rays face further declines and are severely depleted all around the coast, although the number of sightings of basking sharks has increased in recent years especially in the Minches and Malin Sea. These declines are largely the consequence of historically unsustainable catches in both target and non-target fisheries and their long-lived, very low fecundity life cycle. Many of these, for example, porbeagle andcommon skate, can no longer be targeted commercially.Populations of some seabirds, harbour seals and some fish species have declined. Possible reasons include climate change, a number of different human activities and competition from other species. These declines may be associated with broader changes in the food web. For example, the decline in availability of sandeels has had a major influence on recent changes in seabird numbers on the east coast and in the Northern Isles.Although, in general, the current assessment for cetaceans suggests there are no specific concerns, this has been made against a background of a very high level of uncertainty and little power to detect concerns if they currently exist.ProductiveScotland‚s seas are economically productive. Official figures show that the core marine sector, less the extraction of oil and gas, contributed ¬£3.6 billion of Gross Value Added (GVA) in 2008 (at 2009 prices), about 3.5% of overall Scottish GVA. About 39,800 people were employed, 1.6% of Scottish employment. The extraction of oil and gas had a GVA of ¬£13.3 billion in 2007 (at 2009 prices). Fishing takes place in all Scottish sea areas but some, such as Hebrides, North Scotland Coast and East and West Shetland, are far more economically productive than others. Aquaculture, both finfish and shellfish, predominates on the west coast and the islands.Sixteen major ports handle about 98% of all port traffic with liquid bulk, mainly oil and gas, accounting for 69%. There is significant commercial shipping both to ports and for transit through Scottish waters, as well as domestic and international ferry activity. The seas are also used extensively by the Royal Navy and other armed forces, for exercises and operations, sometimes including international partners.Other activities include cooling water abstraction for power stations and the disposal of treated urban waste water, industrial effluent and dredge spoil. Seabed telecommunications cables carry millions of internet and phone call connections, thereby providing a major communications network.The seas are also used for leisure and recreation, particularly sailing, angling and other sporting activities. Scotland‚s historic environment and natural and cultural heritage attract many tourists.The enormous potential of marine renewable energy generation from offshore wind, waves and tides has started to be harnessed. There is also potential for storage of carbon dioxide under the seabed, in ‚carbon capture and storage‚ schemes.Also see http://www.scotland.gov.uk/Resource/Doc/345830/0115128.pdf for maps and further details.",
            Lineage = "Aim of Scotland‚s Marine AtlasThe first ‚Marine Atlas‚ presents data spatially to assist with the introduction of marine planning. It does so based around the elements of the government‚s vision for the sea: clean and safe, healthy and biologically diverse and productive.The Atlas presents the assessment of condition and summary of significant pressures and the impacts of humanactivity required for the national marine plan. It also represents a contribution to the initial assessment required for the Marine Strategy Framework Directive (MSFD)(1) by July 2012.Area covered and scale of dataThe Marine (Scotland) Act 2010(2) and the Marine and Coastal Access Act 2 009(3) provide for marine planning of Scottish waters out to 200 nautical miles and give new marine conservation responsibilities. So this Atlas presents data over this whole area, including for policy areas which are reserved to Westminster, to ensure that policies developed in the national marine plan are informed by the fullest data possible. The Atlas refers to avariety of boundaries and spatial scales: ‚Ä¢ 200 nautical miles ‚Äì the maximum limit for fisheries and renewable energy powers. Marine planning and natureconservation powers can extend further.‚Ä¢ 12 nautical mile territorial sea ‚Äì limit of Scotland as defined in the Scotland Act.‚Ä¢ 3 nautical miles - the limit to which Water Framework Directive measures have been implemented in Scotland.Data can be aggregated to different scales for different purposes, so its presentation has to consider the purpose.This Atlas presents data at the most relevant scale available to illustrate the main issues for national marine planning. There is provision for future marine plans to be developed for Scottish marine regions. These regions have yet to be decided. However, the 15 sea areas used in this Atlas, based on areas previously adopted for certain environmental monitoring programmes, are likely to be of a similar scale as marine regions.The data from these 15 areas can be presented regionally and also reasonably aggregated to form a national picture and to develop information for the two main areas required for the MSFD initial assessment: the Greater North Sea (Area II) and the Celtic Seas (Area III) which are existing sea areas used by OSPAR (the Oslo Paris Convention for the Protection of the North East Atlantic)(4).How this Atlas has been compiledThe Atlas has taken Scotland‚s Seas: Towards Understanding their State, published in 2008(5), and developed itscontents to provide a spatial assessment where possible. Much effort has been taken to map data sets and present graphs around them. It is recognised that there are some areas where this is not yet possible so the provision of spatial data suitable for marine planning will be an evolutionary process. A data annex available online, lists the various data sources used for this Atlas.The preparation of the Atlas has been a collaborative effort. Marine Scotland led the work on chapters 2, 5 and 6,with SEPA in the lead for chapter 3 and SNH, with assistance from JNCC, leading chapter 4. Each contributor also developed the appropriate part of the overall assessment. A valuable contribution was also provided by the Marine Alliance for Science and Technology Scotland (MASTS)(6) to all work. The Agri-Food &amp; Biosciences Institute; Centre for Environment, Fisheries &amp; Aquaculture Science; Countryside Council for Wales; Department for Environment, Food and Rural Affairs; Environment Agency; National Oceanography Centre; Natural England and Northern Ireland Environment Agency all reviewed Atlas material.A variety of data sources have been used. The key data sets are from the range of established monitoring programmes undertaken by the various contributing organisations for their main responsibilities, for example, to meet legal obligations. For much of the productive seas data, existing data from regularly published government statistics have been used. Most data have required some re-working to be presented at the level ofthe sea areas used.Data used for Charting Progress 2 (CP2)(7), published in July 2010, have also been heavily relied on here. CP2 was the second assessment of the UK seas and involved the same contributing Scottish scientists as this Atlas. This Atlas draws on the four CP2 Feeder Reports adding more detail where necessary and appropriate to mapScottish seas.To make the Atlas easily accessible and to focus on portraying information spatially, the material has been restricted to summaries and short text, complimented by images and graphs. For many topics, further information isavailable from a variety of sources and these are referenced. Full references are available in an online annex.Also see http://www.scotland.gov.uk/Resource/Doc/345830/0115121.pdf for more detail.\r\n",
            Topics = "environment,biota,economy,inlandWaters,oceans,transportation,utilitiesCommunication",
            Keywords = "",
            AltTitle = "Scotland's Marine Atlas: Information for the national marine plan. Marine Scotland",
            Theme = "lvl1-003 Valuation"
        };

        //Act
        var result = await _classifierPredictionService.PredictTheme(input, CancellationToken.None);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ModelOutput>();
    }

    [Fact]
    public void GivenPredictCategory_WhenInvalidModelNameIsUsedforPrediction_ThenThrowException()
    {
        //Arrange
        var input = new ModelInputCategory();
        var invalidModelName = "InvalidModel";

        //Act
        var _mockClassifierPredictionService = new ClassifierPredictionService(
            _themePredictionPool,
            _categoryPredictionPool,
            _subCategoryPredictionPool,
            _classifierVocabularyProvider,
            _mocklogger.Object,
            _configuration)!;
        _mockClassifierPredictionService.PredictCategory(invalidModelName, input);

        //Assert
        _mocklogger.Verify(x => x.Log(LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void GivenPredictCategory_WhenInputValuesAreNullOrEmpty_ThenReturnOutputWithPredictionLabelEmpty()
    {
        //Arrange
        var input = new ModelInputCategory();

        //Act
        var result = _classifierPredictionService.PredictCategory(TrainedModels.lvl1_001, input);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ModelOutput>();
    }

    [Fact]
    public void GivenPredictCategory_WhenInputValuesAreNotNullOrEmpty_ThenReturnOutputWithPredictionLabelEmpty()
    {
        //Arrange
        var input = new ModelInputCategory()
        {
            Title = "test-title",
            Abstract = "test-abstract",
            Lineage = "test-lineage",
            Topics = "test-topics",
            Keywords = "test-keywords",
            AltTitle = "test-alttitle",
            Theme = "test-theme",
            CategoryL2 = "test-category"
        };

        //Act
        var result = _classifierPredictionService.PredictCategory(TrainedModels.lvl1_001, input);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ModelOutput>();
    }

    [Fact]
    public void GivenPredictSubCategory_WhenInputValuesAreNullOrEmpty_ThenReturnOutputWithPredictionLabelEmpty()
    {
        //Arrange
        var input = new ModelInputSubCategory();

        //Act
        var result = _classifierPredictionService.PredictSubCategory(TrainedModels.lv2_001, input);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ModelOutput>();
    }

    [Fact]
    public void GivenPredictSubCategory_WhenInvalidModelNameIsUsedforPrediction_ThenThrowException()
    {
        //Arrange
        var input = new ModelInputSubCategory();
        var invalidModelName = "InvalidModel";

        //Act
        var _mockClassifierPredictionService = new ClassifierPredictionService(
            _themePredictionPool,
            _categoryPredictionPool,
            _subCategoryPredictionPool,
            _classifierVocabularyProvider,
            _mocklogger.Object,
            _configuration)!;
        _mockClassifierPredictionService.PredictSubCategory(invalidModelName, input);

        //Assert
        _mocklogger.Verify(x => x.Log(LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void GivenPredictSubCategory_WhenInputValuesAreNotNullOrEmpty_ThenReturnOutputWithPredictionLabelEmpty()
    {
        //Arrange
        var input = new ModelInputSubCategory()
        {
            Title = "test-title",
            Abstract = "test-abstract",
            Lineage = "test-lineage",
            Topics = "test-topics",
            Keywords = "test-keywords",
            AltTitle = "test-alttitle",
            Theme = "test-theme",
            CategoryL2 = "test-category",
            SubCategoryL3 = "test-sub-category"
        };

        //Act
        var result = _classifierPredictionService.PredictSubCategory(TrainedModels.lv2_001, input);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ModelOutput>();
    }
}
