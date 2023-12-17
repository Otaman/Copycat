namespace Copycat;

[AttributeUsage(AttributeTargets.Class)]
public class DecorateAttribute : Attribute{}

[AttributeUsage(AttributeTargets.Method)]
public class TemplateAttribute : Attribute{}