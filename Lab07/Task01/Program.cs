using Task01.Application;
using Task01.Domain.Core;

ITriviumCipher cipher = new TriviumCipher();
var runner = new ExperimentRunner(cipher);

runner.RunExperiment1Verification();
runner.RunExperiment2IvReuse();
runner.RunExperiment3RoundsAnalysis();
runner.RunExperiment4CubeAttack();
runner.RunExperiment5Statistics();
