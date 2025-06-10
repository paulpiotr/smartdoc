using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Orchestration;

namespace SmartDoc.SemanticKernel.Skills
{
    public static class SkillsFactory
    {
        public static (ISKFunction retrieval, ISKFunction answer) Create(ISKFunctionFactory factory)
        {
            var retrieval = factory.CreateSemanticFunction(new SemanticFunctionConfig
            {
                Name = "RetrieveFragments",
                Description = "Retrieves top 5 relevant document fragments.",
                PromptTemplate = @"Zapytanie: {{query}}

Znajdź 5 najbardziej relewantnych fragmentów dokumentacji. Zwróć JSON:
[
    {
      \"id\": \"{{this.key}}\",
      \"text\": \"{{this.text}}\",
      \"source\": \"{{this.metadata.source}}\"
    }
]"
            });

            var answer = factory.CreateSemanticFunction(new SemanticFunctionConfig
            {
                Name = "GenerateAnswer",
                Description = "Generates answer from retrieved fragments.",
                PromptTemplate = @"Na podstawie poniższych fragmentów:

{{#each retrieved}}
- (z {{this.source}}) {{this.text}}
{{/each}}

Odpowiedz na pytanie: {{question}}"
            });

            return (retrieval, answer);
        }
    }
}
