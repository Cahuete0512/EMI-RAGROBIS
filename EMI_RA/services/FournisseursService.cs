﻿using EMI_RA.DAL;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMI_RA
{
    public class FournisseursService : IFournisseursService
    {
        private Fournisseurs_Depot_DAL depot = new Fournisseurs_Depot_DAL();
        private ProduitsServices produitsService = new ProduitsServices();
        private AssoProduitsFournisseursServices assoProduitsFournisseursServices = new AssoProduitsFournisseursServices();

        public List<Fournisseurs> GetAllFournisseurs()
        {
            var fournisseurs = depot.GetAll()
                .Select(f => new Fournisseurs(f.IdFournisseurs,
                                              f.Societe,
                                              f.CiviliteContact,
                                              f.NomContact,
                                              f.PrenomContact,
                                              f.Email,
                                              f.Adresse
                        ))
                .ToList();
            
            return fournisseurs;
        }

        public Fournisseurs GetFournisseursByID(int idFournisseurs)
        {
            var f = depot.GetByID(idFournisseurs);

            return new Fournisseurs(f.IdFournisseurs,
                                    f.Societe,
                                    f.CiviliteContact,
                                    f.NomContact,
                                    f.PrenomContact,
                                    f.Email,
                                    f.Adresse);
        }

        public Fournisseurs Insert(Fournisseurs f)
        {
            var fournisseur = new Fournisseurs_DAL(f.IdFournisseurs,
                                                   f.Societe,
                                                   f.CiviliteContact,
                                                   f.NomContact,
                                                   f.PrenomContact,
                                                   f.Email,
                                                   f.Adresse);
            depot.Insert(fournisseur);
            f.IdFournisseurs = fournisseur.IdFournisseurs;

            return f;
        }

        public Fournisseurs Update(Fournisseurs f)
        {
            var fournisseur = new Fournisseurs_DAL(f.IdFournisseurs,
                                                   f.Societe,
                                                   f.CiviliteContact,
                                                   f.NomContact,
                                                   f.PrenomContact,
                                                   f.Email,
                                                   f.Adresse);
            depot.Update(fournisseur);

            return f;
        }

        public void Delete(Fournisseurs f)
        {
            var fournisseur = new Fournisseurs_DAL(f.IdFournisseurs,
                                                      f.Societe,
                                                      f.CiviliteContact,
                                                      f.NomContact,
                                                      f.PrenomContact,
                                                      f.Email,
                                                      f.Adresse);
            depot.Delete(fournisseur);

        }

        public void alimenterCatalogue(int idFournisseurs, IFormFile csvFile)
        {
            Fournisseurs fournisseurs = this.GetFournisseursByID(idFournisseurs);

            // récupérer les produits en lien avec le fournisseur
            List<Produits> produitsExistantsListe = produitsService.GetByIdFournisseur(idFournisseurs);

            // récupérer les produits du fichier csv
            List<Produits> produitsCsvListe = recupProduitsCsv(csvFile, idFournisseurs);

            // pour les produits du csv qui n'existent pas en BDD -> création du produit en BDD et de la liaison
            foreach (var produitCsv in produitsCsvListe)
            {
                List<Produits> produitsCorrespondants = produitsExistantsListe.Where(p => p.Reference.Equals(produitCsv.Reference)).ToList();

                if(produitsCorrespondants.Count == 0)
                {
                    Produits produitALier = produitsService.GetByRef(produitCsv.Reference);
                    if (produitALier == null)
                    {
                        produitALier = produitsService.Insert(produitCsv);
                    } else if (!produitALier.Disponible)
                    {
                        // Si le produits n'était pas disponible, on le rend disponible
                        produitALier.Disponible = true;
                        produitsService.Update(produitALier);
                    }
                    // pour les produits qui sont dans le fichier et non en BDD -> création du lien
                    assoProduitsFournisseursServices.Insert(new AssoProduitsFournisseurs(idFournisseurs, produitALier.ID));
                }
            }
            // les produits qui sont dans la BDD et non dans le fichier -> suppression du lien BDD
            foreach (var produitExistant in produitsExistantsListe)
            {
                List<Produits> produitCorrespondant = produitsCsvListe.Where(p => p.Reference.Equals(produitExistant.Reference)).ToList();

                if (produitCorrespondant.Count == 0)
                {
                    assoProduitsFournisseursServices.Delete(produitExistant.ID, idFournisseurs);
                    List<AssoProduitsFournisseurs> assoProduitsFournisseursListe = assoProduitsFournisseursServices.GetByIdProduit(produitExistant.ID);
                    if(assoProduitsFournisseursListe.Count == 0)
                    {
                        produitExistant.Disponible = false;
                        produitsService.Update(produitExistant);
                    }
                }
            }
        }

        private List<Produits> recupProduitsCsv(IFormFile csvFile, int idFournisseurs)
        {
            List<Produits> produitsListe = new List<Produits>();
            using (StreamReader reader = new StreamReader(csvFile.OpenReadStream()))
            {
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    string reference = values[0];
                    string libelle = values[1];
                    string marque = values[2];

                    Produits produits = new Produits(libelle, marque, reference, true);
                    produitsListe.Add(produits);
                }
            }

            return produitsListe;
        }
    }
}