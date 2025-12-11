using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace LMStudioExampleFormApp
{
    public class ParametersLMStudio
    {
        public string type { get; set; }
        public Dictionary<string, object> properties { get; set; }
        public bool additionalProperties { get; set; }
        public List<string>? required { get; set; }

        public ParametersLMStudio(string type, Dictionary<string, object> properties, List<string>? required = null, bool additionalProperties = false)
        {
            this.type = type;
            this.properties = properties;
            this.additionalProperties = additionalProperties;
            this.required = required;
        }
    }

    public class FunctionLMStudio
    {
        public string name { get; set; }
        public string description { get; set; }
        public ParametersLMStudio parameters { get; set; }

        public FunctionLMStudio(string name, string description, ParametersLMStudio parameters)
        {
            this.name = name;
            this.description = description;
            this.parameters = parameters;
        }
    }

    public class ToolLMStudio
    {
        public string type { get; set; }
        public FunctionLMStudio function { get; set; }

        public ToolLMStudio(string name, string description, ParametersLMStudio parameters)
        {
            this.type = "function";
            this.function = new FunctionLMStudio(name, description, parameters);
        }
    }

    public class ToolBuilder
    {
        private string? _name;
        private string? _description;
        private string _type = "object";
        private Dictionary<string, object> _properties = new();
        private List<string> _requiredFields = new();
        private bool _additionalProperties = false;
        private string? _instructionHeader;
        private List<string> _keywords = new();
        private List<string> _constraints = new();
        private List<string> _instructions = new();

        public ToolBuilder AddToolName(string name)
        {
            _name = name ?? throw new ArgumentException("Tool name cannot be null.");
            return this;
        }

        public ToolBuilder AddDescription(string description)
        {
            _description = description;
            return this;
        }

        public ToolBuilder SetAdditionalProperties(bool allowAdditional)
        {
            _additionalProperties = allowAdditional;
            return this;
        }

        /// <summary>
        /// Adds constraints that limit when or how the tool should be used.
        /// </summary>
        /// <param name="constraints">One or more constraints to add</param>
        /// <returns>The builder instance for method chaining</returns>
        public ToolBuilder AddConstraint(params string[] constraints)
        {
            if (constraints != null)
            {
                foreach (var constraint in constraints)
                {
                    if (!string.IsNullOrEmpty(constraint))
                    {
                        _constraints.Add(constraint);
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Adds keywords that help categorize or identify the tool's purpose.
        /// </summary>
        /// <param name="keywords">One or more keywords to add</param>
        /// <returns>The builder instance for method chaining</returns>
        public ToolBuilder AddKeyWords(params string[] keywords)
        {
            if (keywords != null)
            {
                foreach (var keyword in keywords)
                {
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        _keywords.Add(keyword);
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Sets the header for the instructions section.
        /// </summary>
        /// <param name="instructionHeader">The header text for instructions</param>
        /// <returns>The builder instance for method chaining</returns>
        public ToolBuilder AddInstructionHeader(string instructionHeader)
        {
            if (!string.IsNullOrEmpty(instructionHeader))
            {
                _instructionHeader = instructionHeader;
            }
            return this;
        }

        /// <summary>
        /// Adds a specific instruction for using the tool.
        /// </summary>
        /// <param name="instruction">The instruction to add</param>
        /// <returns>The builder instance for method chaining</returns>
        public ToolBuilder AddInstructions(string instruction)
        {
            if (!string.IsNullOrEmpty(instruction))
            {
                _instructions.Add(instruction);
            }
            return this;
        }

        public NestedObjectBuilderLMStudio AddNestedObject(string objectName, string objectDescription, bool isRequired = true, bool isArray = false)
        {
            return new NestedObjectBuilderLMStudio(this, null, objectName, objectDescription, isRequired, isArray);
        }

        public ToolBuilder AddProperty(
            string fieldName,
            string fieldType,
            string fieldDescription,
            bool isRequired = false,
            Dictionary<string, string>? items = null)
        {
            var propertyDef = new Dictionary<string, object>
            {
                { "type", fieldType },
                { "description", fieldDescription }
            };

            if (items != null)
            {
                propertyDef["items"] = items;
            }

            _properties[fieldName] = propertyDef;

            if (isRequired)
            {
                _requiredFields.Add(fieldName);
            }

            return this;
        }

        internal void SetNestedObject(string objectName, Dictionary<string, object> properties, bool isRequired, bool isArray)
        {
            var props = properties["properties"] as Dictionary<string, object>;
            bool isSingleProperty = props?.Count == 1;

            if (isArray)
            {
                var arraySchema = new Dictionary<string, object>
                {
                    { "type", "array" },
                    { "items", properties }
                };
                _properties[objectName] = arraySchema;
            }
            else
            {
                _properties[objectName] = properties;
            }

            if (isRequired)
            {
                _requiredFields.Add(objectName);
            }
        }

        public ToolLMStudio Build()
        {
            if (string.IsNullOrWhiteSpace(_name))
            {
                throw new InvalidOperationException("Tool name must be set before building.");
            }

            var descriptionBuilder = new StringBuilder();
            descriptionBuilder.Append(_description?.Trim() ?? "This tool processes input data and generates output.");

            // Format and add keywords section
            if (_keywords.Count > 0)
            {
                descriptionBuilder.Append("\n\nKeywords:");
                foreach (var keyword in _keywords)
                {
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        descriptionBuilder.Append($"\n- {keyword}");
                    }
                }
            }

            // Format and add constraints section
            if (_constraints.Count > 0)
            {
                descriptionBuilder.Append("\n\nConstraints:");
                int count = 1;

                foreach (var constraint in _constraints)
                {
                    if (!string.IsNullOrWhiteSpace(constraint))
                    {
                        descriptionBuilder.Append($"\n{count}. {constraint}");
                        count++;
                    }
                }
            }

            // Format and add instructions section
            if (_instructions.Count > 0)
            {
                descriptionBuilder.Append("\n\nInstructions:");
                int count = 1;

                if (!string.IsNullOrWhiteSpace(_instructionHeader))
                {
                    descriptionBuilder.Append($"\n# {_instructionHeader} #");
                }

                foreach (var instruction in _instructions)
                {
                    if (!string.IsNullOrWhiteSpace(instruction))
                    {
                        descriptionBuilder.Append($"\n{count}. {instruction}");
                        count++;
                    }
                }
            }

            var inputSchema = new ParametersLMStudio(
                type: _type,
                properties: _properties,
                additionalProperties: _additionalProperties,
                required: _requiredFields.Any() ? _requiredFields : null
            );

            return new ToolLMStudio(
                name: _name,
                description: descriptionBuilder.ToString()?.Trim() ?? "This tool processes input data and generates output.",
                parameters: inputSchema
            );
        }
    }

    public class NestedObjectBuilderLMStudio
    {
        private readonly ToolBuilder? _parentBuilder;
        private readonly NestedObjectBuilderLMStudio? _parentNestedBuilder;
        private readonly string _objectName;
        private readonly string _objectDescription;
        private readonly Dictionary<string, object> _properties = new();
        private readonly List<string> _requiredFields = new();
        private readonly bool _isArray;
        private readonly bool _isRequired;

        public NestedObjectBuilderLMStudio(
            ToolBuilder? parentBuilder,
            NestedObjectBuilderLMStudio? parentNestedBuilder,
            string objectName,
            string objectDescription,
            bool isRequired,
            bool isArray)
        {
            _parentBuilder = parentBuilder;
            _parentNestedBuilder = parentNestedBuilder;
            _objectName = objectName;
            _objectDescription = objectDescription;
            _isArray = isArray;
            _isRequired = isRequired;
        }

        public NestedObjectBuilderLMStudio AddNestedObject(string objectName, string objectDescription, bool isRequired = true, bool isArray = false)
        {
            return new NestedObjectBuilderLMStudio(null, this, objectName, objectDescription, isRequired, isArray);
        }

        public NestedObjectBuilderLMStudio AddProperty(
            string fieldName,
            string fieldType,
            string fieldDescription,
            bool isRequired = false,
            Dictionary<string, string>? items = null)
        {
            var propertyDef = new Dictionary<string, object>
            {
                { "type", fieldType },
                { "description", fieldDescription }
            };

            if (items != null)
            {
                propertyDef["items"] = items;
            }

            _properties[fieldName] = propertyDef;

            if (isRequired)
            {
                _requiredFields.Add(fieldName);
            }

            return this;
        }

        private Dictionary<string, object> BuildDefinition()
        {
            var objectDefinition = new Dictionary<string, object>
            {
                { "type", "object" },
                { "description", _objectDescription },
                { "properties", _properties }
            };

            if (_requiredFields.Count > 0)
            {
                objectDefinition["required"] = _requiredFields;
            }

            return objectDefinition;
        }

        public NestedObjectBuilderLMStudio EndNestedObject()
        {
            var definition = BuildDefinition();
            if (_parentNestedBuilder != null)
            {
                _parentNestedBuilder._properties[_objectName] = definition;
                return _parentNestedBuilder;
            }
            return this;
        }

        public ToolBuilder EndObject()
        {
            if (_parentBuilder == null)
            {
                throw new InvalidOperationException("Cannot end object without a parent builder");
            }

            var objectDefinition = BuildDefinition();
            _parentBuilder.SetNestedObject(_objectName, objectDefinition, _isRequired, _isArray);
            return _parentBuilder;
        }
    }

    public static class ToolStringOutputLMStudio
    {
        public static string GenerateToolJson(ToolLMStudio tool)
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true
            };
            return JsonSerializer.Serialize(tool, options);
        }
    }





}
