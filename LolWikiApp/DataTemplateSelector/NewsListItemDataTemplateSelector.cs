using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LolWikiApp
{
    public abstract class DataTemplateSelector :ContentControl
    {
        public virtual DataTemplate SelectDataTemplate(object item, DependencyObject container)
        {
            return null;
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            ContentTemplate = SelectDataTemplate(newContent, this);
        }
    }

    public class NewsListItemDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NormalNewsDataTemplate { get; set; }
        public DataTemplate FlipViewNewsDataTemplate { get; set; }

        public override DataTemplate SelectDataTemplate(object item, DependencyObject container)
        {
            var news = item as NewsListInfo;

            if (news != null)
            {
                if (news.IsFlipNews)
                {
                    return FlipViewNewsDataTemplate;
                }
                return NormalNewsDataTemplate;
            }

            return base.SelectDataTemplate(item, container);
        }
    }
}
