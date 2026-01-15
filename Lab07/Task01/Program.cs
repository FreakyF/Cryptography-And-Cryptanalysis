using Task01.Application;
using Task01.Domain.Core;

ITriviumCipher cipher = new TriviumCipher();
var runner = new ExperimentRunner(cipher);

runner.RunExperiment1_Verification();
runner.RunExperiment2_IvReuse();
runner.RunExperiment3_RoundsAnalysis();
runner.RunExperiment4_CubeAttack();
runner.RunExperiment5_Statistics();
