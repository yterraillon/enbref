namespace EnBref.Infrastructure.OpenAiAgents.RecapBuilder;

public static class RecapBuilderPrompts
{
    public const string UserPrompt = """
                                         Tu es un analyste spécialisé dans les actualités. Résume les principaux événements d'actualité récents et conclus par "C'est tout pour aujourd'hui".
                                         Classe-les en catégories : [exemple : Politique, Économie, Technologie, International].
                                         Assure-toi que le ton reste neutre, factuel, et accessible à un large public.
                                          Voici les titres d'actualités à utiliser :
                                         """;
}