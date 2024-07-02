﻿using Ncea.Enricher.Models;

namespace Ncea.Enricher.Services.Contracts;

public interface IClassifierVocabularyProvider
{
    Task<List<ClassifierInfo>> GetAll(CancellationToken cancellationToken);
}
