CREATE DATABASE library;

SELECT current_date() FROM dual;

USE library;

CREATE TABLE tPermissionLookup(
	iID INT PRIMARY KEY,
    sName VARCHAR(10)
);

INSERT INTO tPermissionLookup (iID, sName) VALUES (0, 'Owner'), (1, 'Editor'), (2, 'Viewer'), (3, 'None');
SELECT * FROM tPermissionLookup;

SELECT * FROM AspNetUsers;


CREATE TABLE tLibrary(
	iID INT PRIMARY KEY AUTO_INCREMENT,
    sName VARCHAR(80),
    dtCreated DATETIME DEFAULT CURRENT_TIMESTAMP
);
ALTER TABLE tLibrary ADD COLUMN sOwner VARCHAR(256);

CREATE TABLE tPermission(
	sUserID VARCHAR(256) NOT NULL,
    iLibraryID INT NOT NULL,
    iPermissionLevel INT DEFAULT 0,
    FOREIGN KEY (sUserID) REFERENCES AspNetUsers (Id),
    FOREIGN KEY (iLibraryID) REFERENCES tLibrary (iID)
);
    
CREATE TABLE tCollection(
	iID INT PRIMARY KEY AUTO_INCREMENT,
    iLibraryID INT NOT NULL,
    iParentCollectionID INT,
    sName VARCHAR(80),
    sDescription VARCHAR(255),
    FOREIGN KEY (iLibraryID) REFERENCES tLibrary (iID)
);

CREATE TABLE tBook(
	iID INT PRIMARY KEY AUTO_INCREMENT,
    iLibraryID INT NOT NULL,
    sTitle VARCHAR(255),
    sSynopsis VARCHAR(1023),
    dtAdded DATETIME,
    dtPublished DATETIME,
    FOREIGN KEY (iLibraryID) REFERENCES tLibrary (iID)
);

CREATE TABLE tAuthor(
	iID INT PRIMARY KEY AUTO_INCREMENT,
    sFirstName VARCHAR(40),
    sLastName VARCHAR(40)
);

CREATE TABLE tBookAuthorXREF(
	iBookID INT NOT NULL,
    iAuthorID INT NOT NULL,
    iListPosition INT,
    FOREIGN KEY (iBookID) REFERENCES tBook (iID),
    FOREIGN KEY (iAuthorID) REFERENCES tAuthor (iID),
    PRIMARY KEY (iBookID, iAuthorID)
);

CREATE TABLE tCollectionBookXREF(
	iCollectionID INT NOT NULL,
    iBookID INT NOT NULL,
    FOREIGN KEY (iCollectionID) REFERENCES tCollection (iID),
    FOREIGN KEY (iBookID) REFERENCES tBook (iID),
    PRIMARY KEY (iCollectionID, iBookID)
);


CREATE TABLE tLibraryInvite(
	iID INT PRIMARY KEY AUTO_INCREMENT,
    sInviterID VARCHAR(256) NOT NULL,
    sRecipientID VARCHAR(256) NOT NULL,
    dtSent DATETIME,
    FOREIGN KEY (sInviterID) REFERENCES AspNetUsers (Id),
    FOREIGN KEY (sRecipientID) REFERENCES AspNetUsers (Id)
);