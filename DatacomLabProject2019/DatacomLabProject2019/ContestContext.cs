namespace DatacomLabProject2019
{
    using System.Collections.Generic;
    using System.Data.Entity;

    public class ContestDBInitializer : DropCreateDatabaseAlways<ContestContext>
    {
        protected override void Seed(ContestContext context)
        {
            IList<Question> defaultQuestions = new List<Question>();

            defaultQuestions.Add(new Question() { Text = ">>> Soru:1-) Karabuk'un plaka numarasi kactir?\n A:78 B:36 C:39 D:41\n\n", Answer = "A" });
            defaultQuestions.Add(new Question() { Text = ">>> Soru:2-) Asagidakilerden hangisi Birlesmis Milletler Cocuklara Yardim Fonu'nun kisaltilmis ismidir?\n A:Uhw B:Unicef C:Unf D:Nato\n\n", Answer = "B" });
            defaultQuestions.Add(new Question() { Text = ">>> Soru:3-) Istiklal Marsimizin bestesi kime aittir?\n A:Mehmet Akif Ersoy B:Yahya Kemal Beyatli C:Osman Zeki Ungor D:Namik Kemal\n\n", Answer = "C" });


            context.Questions.AddRange(defaultQuestions);

            base.Seed(context);
        }
    }

    public class ContestContext : DbContext
    {
        public ContestContext()
        {
            Database.SetInitializer(new ContestDBInitializer());
        }

        public virtual DbSet<Question> Questions { get; set; }
    }
}