# Schritte zur Erstellung eines Live Linux ISO-Image

Diese Beschreibung basiert auf der Anleitung "Live-Ubuntu selbstgebaut" aus dem [c't Heft 11/2016](https://www.heise.de/ct/ausgabe/2016-11-Selbstgemachtes-Live-Ubuntu-fuer-DVD-und-USB-Stick-3198759.html), des Autors Mirko Dölle.

Als Grundlage wurde die amd64-Variante der Ubuntu-Installations-DVD (die TRuDI ist eine 64-Bit-Applikation) der LTS Version 16.04 verwendet.


Legen Sie im Benutzerverzeichnis zuerst ein neues Arbeitsverzeichnis, z.B. ``TRuDI_LiveCD`` an. 
- In diesem Verzeichnis legen Sie dann ein neues Verzeichnis namens ``iso`` an.
-  Darin legen sie ein neues Verzeichnis namens ``casper`` an, und kopieren Sie, von der Installations-DVD, die Verzeichnisse ``.disk``, ``boot``, ``isolinux`` und ``EFI`` dorthin. Kopieren Sie auch die letzte Version von TRuDI Software in das Arbeitsverzeichnis.

Erzeugen Sie nun mit dem Tool ``debootstrap`` das Live-System in einem neuen Verzeichnis namens ``squashfs`` in unserem Arbeitsverzeichnis. 

```
~/TRuDI_LiveCD$ sudo debootstrap --arch amd64 xenial squashfs
```

Um das erzeugte Live-System in Ihr laufendes System einzubinden, führen Sie folgende Befehle aus:

```
~/TRuDI_LiveCD$ sudo mount --bind /dev squashfs/dev
~/TRuDI_LiveCD$ sudo mount -t devpts devpts squashfs/dev/pts
~/TRuDI_LiveCD$ sudo mount -t proc proc squashfs/proc
~/TRuDI_LiveCD$ sudo mount -t sysfs sysfs squashfs/sys
```

Um Pakete über die Offizielle Quellen beziehen zu können, führen Sie folgendes aus:

```
~/TRuDI_LiveCD$ sudo cp /etc/resolv.conf squashfs/etc
~/TRuDI_LiveCD$ sudo cp /etc/apt/sources.list squashfs/etc/apt
```

Nun kann man die Quellen, und danach auch die Softwarepakete aktualisieren:

```
~/TRuDI_LiveCD$ sudo chroot squashfs apt update
~/TRuDI_LiveCD$ sudo chroot squashfs apt upgrade
```

Installieren Sie nun folgende essentielle Pakete:

```
~/TRuDI_LiveCD$ sudo chroot squashfs apt install linux-image-generic
~/TRuDI_LiveCD$ sudo chroot squashfs apt install tzdata
~/TRuDI_LiveCD$ sudo chroot squashfs apt install console-setup
~/TRuDI_LiveCD$ sudo chroot squashfs apt install casper
~/TRuDI_LiveCD$ sudo chroot squashfs apt install ubiquity-casper
~/TRuDI_LiveCD$ sudo chroot squashfs apt install lupin-casper
~/TRuDI_LiveCD$ sudo chroot squashfs apt install --no-install-recommends ubuntu-desktop
```

Für die deutsche Sprachunterstützung sind folgende Pakete nötig: 

```
~/TRuDI_LiveCD$ sudo chroot squashfs apt install language-pack-de
~/TRuDI_LiveCD$ sudo chroot squashfs apt install language-pack-gnome-de
~/TRuDI_LiveCD$ sudo chroot squashfs apt install wngerman
~/TRuDI_LiveCD$ sudo chroot squashfs apt install wogerman
~/TRuDI_LiveCD$ sudo chroot squashfs apt install wswiss
```

Setzen Sie Deutsch als Standardsprache wie folgt:
```
~/TRuDI_LiveCD$ sudo chroot squashfs update-locale LANG=de_DE.UTF-8 LANGUAGE=de_DE LC_ALL=de_DE.UTF-8
```

Ändern sie folgende Datei, um die deutsche Tastatur als Standard beim Bootvorgang einzustellen:

```
$ sudo nano squashfs/etc/default/keyboard 
```

Der Dateiinhalt sollte wie folgt aussehen:

```
XKBMODEL="pc105"
XKBLAYOUT="de,us"
XKBVARIANT=""
XKBOPTIONS="" 
BACKSPACE="guess"
```

Kopieren Sie das Installationspaket der aktuelle TRuDI-Version in den ``squashfs`` Verzeichnisbaum und führen Sie die Installation aus. (alle abhängigen Pakete werden automatisch mitinstalliert):

```
~/TRuDI_LiveCD$ sudo cp TRuDI-1.0.38_amd64.deb ./squashfs/usr/share/
~/TRuDI_LiveCD$ sudo chroot squashfs apt install /usr/share/TRuDI-1.0.38_amd64.deb
~/TRuDI_LiveCD$ sudo rm ./squashfs/usr/share/TRuDI-1.0.38_amd64.deb
```

Eine Desktopverknüpfung für die TRuDI legt man im Verzeichnis squashfs/etc/skel/ an, da ein Benutzer beim Live System immer dynamisch angelegt wird:

```
~/TRuDI_LiveCD$ sudo mkdir squashfs/etc/skel/Desktop
~/TRuDI_LiveCD$ sudo touch squashfs/etc/skel/Desktop/TRuDI.desktop
~/TRuDI_LiveCD$ sudo nano squashfs/etc/skel/Desktop/TRuDI.desktop
```

Der Dateiinhalt sollte folgendermaßen aussehen:

```
[Desktop Entry]
Name=TRuDI
Exec=trudi
Icon=/usr/share/backgounds/trudi/icon.png
Terminal=false
Type=Application
```

Es muss noch ein Icon für die Verknüpfung eingerichtet werden (Es wird angenommen, dass Sie eine Datei namens icon.png bereits in das Arbeitsverzeichnis kopiert haben):

```
~/TRuDI_LiveCD$ sudo mkdir squashfs/usr/share/backgounds/trudi
~/TRuDI_LiveCD$ sudo cp icon.png squashfs/usr/share/backgounds/trudi/icon.png
```

### Optionale Schritte

#### Festes Benutzerkonto einrichten

Die Standardversion des Ubuntu Live Systems legt bei jedem Start einen Benutzer namens ``ubuntu`` dynamisch an. 
Dieser Benutzer hat standardmäßig kein Passwort und kann mit ``sudo`` Administratorrechte bekommen. 
Es ist daher ratsam einen festen Benutzer mit eingeschränkten Benutzerrechten auf dem System einzurichten. 
Standardmäßig kann dieser Benutzer keine Aktionen die Systemadministratorrechte benötigen ausführen.

Neuer Benutzer wird mit dem Kommando ``adduser`` erstellt. 
Benutzername und Passwort kann man auf ``trudi`` setzen und alle weiteren Fragen überspringen.

```
~/TRuDI_LiveCD$ sudo chroot squashfs adduser trudi
```

#### Erscheinungsbild anpassen

Sie können das Aussehens des Live-Systems individuell anpassen. Man kann z.B. das Aussehen der Benutzeroberfläche 
für die Benutzeranmeldung anpassen, indem man eigenen Hintergrund und eigenes Logo verwendet.

Das Hintergrundbild und Logo sollten in Verzeichnisse ``squashfs/usr/share/backgrounds/``, 
bzw.  ``squashfs/usr/share/unity-greeter/`` kopiert werden. Dann ändert man folgende Datei:

```
~/TRuDI_LiveCD$ sudo nano squashfs/usr/share/glib-2.0/schemas/10_unity_greeter_background.gschema.override
```

Der Dateiinhalt sollte folgendermaßen aussehen:

```
[com.canonical.unity-greeter]
draw-user-backgrounds=false
background='/usr/share/backgrounds/trudi_background.png'
logo='/usr/share/unity-greeter/trudi_greeter_logo.png'
```


## ISO-Image Fertigstellen

Erstellen Sie nun das ISO-Image wie folgt. Das Ergebnis ist eine neue Datei namens live.iso in Ihrem Arbeitsverzeichnis: 

```
~/TRuDI_LiveCD$ sudo chroot squashfs update-initramfs -k all -c
~/TRuDI_LiveCD$ sudo zcat squashfs/boot/initrd.img* | lzma -9c > iso/casper/initrd.lz
~/TRuDI_LiveCD$ sudo cp squashfs/boot/vmlinuz* iso/casper/vmlinuz.efi
~/TRuDI_LiveCD$ sudo umount squashfs/dev/pts squashfs/dev squashfs/proc squashfs/sys
~/TRuDI_LiveCD$ sudo rm squashfs/etc/resolv.conf 
~/TRuDI_LiveCD$ sudo mksquashfs squashfs iso/casper/filesystem.squashfs -noappend
~/TRuDI_LiveCD$ sudo genisoimage -cache-inodes -r -J -l -b isolinux/isolinux.bin -c isolinux/boot.cat -no-emul-boot -boot-load-size 4 -boot-info-table -eltorito-alt-boot -e boot/grub/efi.img -no-emul-boot -o live.iso iso
```
