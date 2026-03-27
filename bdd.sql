CREATE TABLE `utilisateur` (
    `idUtilisateur` INT AUTO_INCREMENT PRIMARY KEY,
    `nom` VARCHAR(100),
    `prenom` VARCHAR(100),
    `email` VARCHAR(100),
    `mdp` VARCHAR(100)
);

CREATE TABLE `budget` (
    `idBudget` INT AUTO_INCREMENT PRIMARY KEY,
    `solde_actuelle` DECIMAL(15, 2),
    `id_user` INT NOT NULL,
    FOREIGN KEY (`id_user`) REFERENCES `utilisateur`(`idUtilisateur`)
);

CREATE TABLE `type` (
    `idType` INT AUTO_INCREMENT PRIMARY KEY,
    `labelType` VARCHAR(10)
);

CREATE TABLE `depense` (
    `idDepense` INT AUTO_INCREMENT PRIMARY KEY,
    `montant` DECIMAL(15, 2),
    `id_type` INT NOT NULL,
    `id_budget` INT NOT NULL,
    FOREIGN KEY (`id_type`) REFERENCES `type`(`idType`),
    FOREIGN KEY (`id_budget`) REFERENCES `budget`(`idBudget`)
);

CREATE TABLE `revenue` (
    `idRevenue` INT AUTO_INCREMENT PRIMARY KEY,
    `montant` DECIMAL(15, 2),
    `id_type` INT NOT NULL,
    `id_budget` INT NOT NULL,
    FOREIGN KEY (`id_type`) REFERENCES `type`(`idType`),
    FOREIGN KEY (`id_budget`) REFERENCES `budget`(`idBudget`)
);
