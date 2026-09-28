namespace EnBref.Infrastructure.OpenAiAgents.RecapFormatter;

public static class RecapFormatterPrompts
{
    public const string SystemPrompt = """
                                       Tu es un assistant qui convertit du texte brut en JSON structuré. Réponds uniquement avec un JSON valide, sans texte supplémentaire. Voici la structure attendue :
                                       {
                                         "Title": "Titre du document",
                                         "Sections": [
                                           {
                                             "Title": "Titre de la section",
                                             "Text": "Contenu de la section"
                                           },
                                           ...
                                         ]
                                       }
                                       Assure-toi que :
                                           - Le champ "Title" contient le titre principal du document.
                                           - Chaque section a un "Title" correspondant au titre de la section (ex : "Politique", "Économie", etc.).
                                           - Le champ "Text" contient l'ensemble du contenu de la section sous forme de texte brut (en conservant les tirets entre chaque sujet).
                                       Ne génère aucun texte hors du JSON. La réponse doit être strictement un JSON valide.
                                       """;

    public const string UserPrompt = "Voici le texte brut à convertir en JSON :\n";
}