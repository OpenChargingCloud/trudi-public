const electron = require('electron')
// Module to control application life.
const app = electron.app
// Module to create native browser window.
const BrowserWindow = electron.BrowserWindow

const path = require('path')
const url = require('url')
const crypto = require('crypto');
const os = require('os');

const backendPathWindows = '../TRuDI.Backend/bin/dist/win10-x64';
const backendPathLinux = '../TRuDI.Backend/bin/dist/linux';

// createBackendHash(backendPathWindows);

// Parse command line options.
const argv = process.argv.slice(1)
const options = { testConfigFile: null }
for (let i = 0; i < argv.length; i++) {

    writeLog(`arg ${i}: ${argv[i]}`);
    if (argv[i].match(/^--test=/)) {
        options.testConfigFile = argv[i].split('=')[1]
        break
    }
}

// Keep a global reference of the window object, if you don't, the window will
// be closed automatically when the JavaScript object is garbage collected.
let mainWindow

function createWindow () {
  // Create the browser window.
  mainWindow = new BrowserWindow({width: 1024, height: 700})

  // mainWindow.setMenu(null);
  
  // and load the index.html of the app.
  
  mainWindow.loadURL("http://localhost:5000");
  
  /*
  mainWindow.loadURL(url.format({
    pathname: path.join(__dirname, 'index.html'),
    protocol: 'file:',
    slashes: true
  }))
*/
  // Open the DevTools.
  //mainWindow.webContents.openDevTools()

  // Emitted when the window is closed.
  mainWindow.on('closed', function () {
    // Dereference the window object, usually you would store windows
    // in an array if your app supports multi windows, this is the time
    // when you should delete the corresponding element.
    mainWindow = null
  })
}

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.on('ready', startBackendService)

// Quit when all windows are closed.
app.on('window-all-closed', function () {
  // On OS X it is common for applications and their menu bar
  // to stay active until the user quits explicitly with Cmd + Q
  if (process.platform !== 'darwin') {
    app.quit()
  }
})

app.on('activate', function () {
  // On OS X it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  if (mainWindow === null) {
    createWindow()
  }
})


function createBackendHash(backendPath) {
    var fs = require('fs');
    var walk = function (dir, done) {
        var results = [];
        fs.readdir(dir, function (err, list) {
            if (err) return done(err);
            var i = 0;
            (function next() {
                var file = list[i++];
                if (!file) return done(null, results);
                file = dir + '/' + file;
                fs.stat(file, function (err, stat) {
                    if (stat && stat.isDirectory()) {
                        walk(file, function (err, res) {
                            results = results.concat(res);
                            next();
                        });
                    } else {
                        results.push(file);
                        next();
                    }
                });
            })();
        });
    };

    walk(backendPath, function (err, results) {
        if (err) throw err;

        var hash = crypto.createHash('sha256');
        hash.setEncoding('hex');

        var i = 0;
        (function next() {
            var file = results[i++];
            if (!file) {
                hash.end();
                console.log('calculated hash value: ' + hash.read());
                return;
            }

            var stream = fs.createReadStream(file);
            stream.on('data',
                function(data) {
                    hash.update(data);
                });

            stream.on('end', function() {
                next();
            });
        })();
    });
}



var backendServiceProcess = null;

// run the backend server
function startBackendService() {
  var proc = require('child_process').spawn;
  
  var apipath = path.join(__dirname, '..\\TRuDI.Backend\\bin\\dist\\win10-x64\\TRuDI.Backend.exe')
  var workpath = path.join(__dirname, '..\\TRuDI.Backend\\bin\\dist\\win10-x64\\')
  
  if (os.platform() === 'linux') {
      apipath = path.join(__dirname, backendPathLinux, 'TRuDI.Backend')
      workpath = path.join(__dirname, backendPathWindows)
  }

  if (options.testConfigFile == null) {
      backendServiceProcess = proc(apipath, { cwd: workpath })
  } else {
      backendServiceProcess = proc(apipath, ['--test=' + options.testConfigFile], { cwd: workpath })
  }
  
  backendServiceProcess.stdout.on('data', (data) => {
    writeLog(`stdout: ${data}`);
    if (mainWindow == null) {
      createWindow();
    }
  });
}

//Kill process when electron exits
process.on('exit', function () {
  writeLog('exit');
  backendServiceProcess.kill();
});

function writeLog(msg){
  console.log(msg);
}
