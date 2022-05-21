using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.ModelIndex
{
    public class IndexSearch
    {
        public void GetExactly<T>(Expression<Func<T, string>> expression, string value)
        {
            var body = expression.Body;
            if (body == null) throw new ArgumentException("The search expression doesnt have body");
            var memberExpression=body as MemberExpression;
            if (memberExpression == null) throw new ArgumentException("The expression is not a member expression");
            var attribute=memberExpression?.Member?.CustomAttributes?.Where(x => x.AttributeType == typeof(AggregateModelIndexAttribute)).FirstOrDefault();
            if (attribute == null) throw new ArgumentException("The property used in the expression doesnt have an AggreateModelIndexAttribute");
            var memberInfo = memberExpression?.Member;
            if (memberInfo == null) throw new ArgumentException("The properrty cannot be found");
            var modelType = memberInfo.DeclaringType;
            if (modelType == null) throw new ArgumentException("We cannot find the model type by in the expression");
            string className = modelType.Name;
            string propertyName = memberInfo.Name;
            var property=modelType.GetProperties().Where(x => x.Name == propertyName).FirstOrDefault();
            if (property == null) throw new ArgumentException("Cannot find the property");
            var modelIndexAttribute=property.GetCustomAttributes(typeof(AggregateModelIndexAttribute), true).First() as AggregateModelIndexAttribute;


        }
    }
}
