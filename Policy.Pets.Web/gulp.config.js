module.exports = function () {
    var client = './src/client/';
    var clientApp = client + 'app/';
    var clientTests = client + 'tests/unit/';
    var report = './report/';
    var root = './';
    var server = './src/server/';
    var specRunnerFile = 'specs.html';
    var temp = './.tmp/';
    var tempApp = temp + 'app/';
    var tempTests = temp + 'tests/';
    var wiredep = require('wiredep');
    var bowerFiles = wiredep({ devDependencies: true })['js'];

    var config = {
        /**
         * Files paths
         */
        alljs: [
            tempApp + '**/*.js'
        ],
        build: './../webapp/',
        client: client,
        css: [
            './bower_components/bootstrap/dist/css/bootstrap.css',
            temp + '**/*.css',
            client + './styles/**/*.css'
        ], /*css: temp + 'styles.css',*/
        fonts: [
            './bower_components/font-awesome/fonts/**/*.*',
            client + 'fonts/**/*.*'
        ],
        html: clientApp + '**/*.html',
        htmltemplates: clientApp + '**/*.html',
        images: client + 'images/**/*.*',
        index: client + 'index.html',
        js: [
            client + 'js/*.js',
            //tempApp + 'settings.js',
            //tempApp + 'common/**/*.js',
            client + 'js/**/*.js',
            tempApp + 'models/BaseItem.js',
            tempApp + 'models/**/*.js',
            tempApp + 'directives/**/*.js',
            tempApp + 'services/**/*.js',
            tempApp + 'filters/**/*.js',
            tempApp + 'controllers/**/*.js',
            tempApp + 'configs/**/*.js',
            tempApp + 'radixapp.js',
            tempApp + '**/*.js',
            //'!' + tempApp + '_all.js',
            '!' + tempApp + 'interfaces/**/*.js',
            '!' + tempApp + '**/*.spec.js'
        ],
        less: client + 'styles/**/*.less', /*less: client + 'styles/styles.less',*/
        ts: [
            client + '**/*.ts'
            //clientTests + '**/*.ts',
            //client + 'test-helpers/**/*.ts'
        ],
        tslibs: 'ts_definitions/**/*.ts',
        report: report,
        root: root,
        server: server,
        temp: temp,

        /**
         * optimized files
         */
        optimized: {
            app: 'app.js',
            lib: 'lib.js'
        },

        /**
         * template cache
         */
        templateCache: {
            file: 'templates.js',
            options: {
                module: 'policyApp',
                standAlone: false,
                root: 'app/'
            }
        },

        /**
         * browser sync
         */
        browserReloadDelay: 1000,

        /**
         * Bower and NPM locations
         */
        bower: {
            json: require('./bower.json'),
            directory: './bower_components/',
            ignorePath: '../..'
        },
        packages: [
            './package.json',
            './bower.json'
        ],

        /**
         * specs.html, our HTML spec runner
         */
        specRunner: client + specRunnerFile,
        specRunnerFile: specRunnerFile,
        testlibraries: [
           /*'node_modules/jasmine-core/lib/jasmine-core/jasmine.js',
           'node_modules/jasmine-core/lib/jasmine-core/jasmine-html.js',
           'node_modules/jasmine-core/lib/jasmine-core/boot.js', */
           'node_modules/mocha/mocha.js',
           'node_modules/chai/chai.js',
           'node_modules/mocha-clean/index.js',
           'node_modules/sinon-chai/lib/sinon-chai.js'
        ],
        specs: [tempTests + '**/*.spec.js'],

        /**
         * Karma and testing settings
         */
        specHelpers: [
            client + 'test-helpers/bind-polyfill.js',
            temp + 'test-helpers/*.js'
        ],
        serverIntegrationSpecs: [client + 'tests/server-integration/**/*.spec.js'],

        /**
         * Node settings
         */
        defaultPort: 7203,
        nodeServer: './src/server/app.js',
        webinf: 'WEB-INF/*'
    };

    config.getWiredepDefaultOptions = function () {
        var options = {
            bowerJson: config.bower.json,
            directory: config.bower.directory,
            ignorePath: config.bower.ignorePath
        };
        return options;
    };

    config.karma = getKarmaOptions();

    return config;

    ////////////////

    function getKarmaOptions() {
        var options = {
            files: [].concat(
                bowerFiles,
                config.specHelpers,
                //tempApp + 'settings.js',
                //tempApp + 'common/**/*.js',
                //client + 'js/**/*.js',
                //tempApp + 'models/BaseItem.js',
                //tempApp + 'models/**/*.js',
                //tempApp + 'directives/**/*.js',
                tempApp + 'services/**/*.js',
                tempApp + 'controllers/**/*.js',
                tempApp + 'configs/**/*.js',
                tempApp + '**/*.js',
                tempTests + '**/*.spec.js',
                temp + config.templateCache.file
                //config.serverIntegrationSpecs
            ),
            exclude: [],
            coverage: {
                //a common output directory
                dir: report + 'coverage',
                reporters: [
                    { type: 'html', subdir: 'report-html' },
                    { type: 'lcov', subdir: 'report-lcov' },
                    { type: 'text-summary' /*, subdir: '.', file: 'text-summary.txt'*/ }
                ],
                threshold: {
                    statements: 10,
                    branches: 10,
                    functions: 10,
                    lines: 10
                }
            },
            preprocessors: {}
        };
        options.preprocessors[tempApp + '**/!(*.spec)+(.js)'] = ['coverage'];
        return options;
    }
};
