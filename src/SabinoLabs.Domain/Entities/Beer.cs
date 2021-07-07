using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SabinoLabs.Domain.Entities
{
    [Table("beer")]
    public class Beer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string Name { get; set; }
        public string Ibu { get; set; }
        public string Style { get; set; }
        public string Description { get; set; }
        public string AlcoholTenor { get; set; }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Beer beer = obj as Beer;
            if (beer?.Id == null || beer?.Id == 0 || Id == 0)
            {
                return false;
            }

            return EqualityComparer<long>.Default.Equals(Id, beer.Id);
        }

        public override int GetHashCode() => HashCode.Combine(Id);

        public override string ToString() =>
            "Beer{" +
            $"ID='{Id}'" +
            $", Name='{Name}'" +
            $", Ibu='{Ibu}'" +
            $", Style='{Style}'" +
            $", Description='{Description}'" +
            $", AlcoholTenor='{AlcoholTenor}'" +
            "}";
    }
}
